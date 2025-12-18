using System;
using System.Collections.Generic;
using System.Text;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace Aigamo.ResXGenerator.Tools;

/// <summary>
/// Parser for .NET fully qualified type names using the sly parser generator.
/// Grammar based on: https://learn.microsoft.com/en-us/dotnet/fundamentals/reflection/specifying-fully-qualified-type-names
/// </summary>
internal class TypeNameParser
{
	public enum TypeNameToken
	{
		[Lexeme(@"[a-zA-Z_][a-zA-Z0-9_]*")]
		Identifier = 1,

		[Lexeme(@"\.")]
		Dot = 2,

		[Lexeme(@",")]
		Comma = 3,

		[Lexeme(@"`[0-9]+")]
		GenericArity = 4,

		[Lexeme(@"\[")]
		OpenBracket = 5,

		[Lexeme(@"\]")]
		CloseBracket = 6,

		[Lexeme(@"\+")]
		Plus = 7,

		[Lexeme(@"\*")]
		Asterisk = 8,

		[Lexeme(@"&")]
		Ampersand = 9,

		[Lexeme(@"=")]
		Equals = 10,

		[Lexeme(@"[0-9]+")]
		Number = 11,

		[Lexeme(@"[ \t]+", isSkippable: true)]
		Whitespace = 100,
	}

	[ParserRoot("typeSpec")]
	public class TypeNameParserDefinition
	{
		// ==========================================
		// Top-level: typeSpec
		// ==========================================

		[Production("typeSpec: simpleTypeSpec Ampersand")]
		public ParsedTypeName ReferenceType(ParsedTypeName simpleType, Token<TypeNameToken> ampersand)
		{
			simpleType.IsReference = true;
			return simpleType;
		}

		[Production("typeSpec: simpleTypeSpec")]
		public ParsedTypeName TypeSpecSimple(ParsedTypeName simpleType)
		{
			return simpleType;
		}

		// ==========================================
		// simpleTypeSpec: base type with suffixes (pointers and/or arrays)
		// ==========================================

		[Production("simpleTypeSpec: baseType pointerSuffix arraySuffix")]
		public ParsedTypeName SimpleTypeSpecWithPointerAndArray(ParsedTypeName baseType, ParsedTypeName pointerSuffix, ParsedTypeName arraySuffix)
		{
			baseType.PointerDepth = pointerSuffix.PointerDepth;
			baseType.ArrayRanks.AddRange(arraySuffix.ArrayRanks);
			return baseType;
		}

		[Production("simpleTypeSpec: baseType pointerSuffix")]
		public ParsedTypeName SimpleTypeSpecWithPointer(ParsedTypeName baseType, ParsedTypeName suffix)
		{
			baseType.PointerDepth = suffix.PointerDepth;
			return baseType;
		}

		[Production("simpleTypeSpec: baseType arraySuffix")]
		public ParsedTypeName SimpleTypeSpecWithArray(ParsedTypeName baseType, ParsedTypeName suffix)
		{
			baseType.ArrayRanks.AddRange(suffix.ArrayRanks);
			return baseType;
		}

		[Production("simpleTypeSpec: baseType")]
		public ParsedTypeName SimpleTypeSpecBase(ParsedTypeName baseType)
		{
			return baseType;
		}

		// ==========================================
		// pointerSuffix: one or more asterisks
		// ==========================================

		[Production("pointerSuffix: Asterisk pointerSuffix")]
		public ParsedTypeName PointerSuffixCons(Token<TypeNameToken> asterisk, ParsedTypeName rest)
		{
			rest.PointerDepth++;
			return rest;
		}

		[Production("pointerSuffix: Asterisk")]
		public ParsedTypeName PointerSuffixSingle(Token<TypeNameToken> asterisk)
		{
			return new ParsedTypeName { PointerDepth = 1 };
		}

		// ==========================================
		// arraySuffix: one or more array specs
		// ==========================================

		[Production("arraySuffix: arraySpec arraySuffix")]
		public ParsedTypeName ArraySuffixCons(ParsedTypeName spec, ParsedTypeName rest)
		{
			var result = new ParsedTypeName();
			result.ArrayRanks.AddRange(spec.ArrayRanks);
			result.ArrayRanks.AddRange(rest.ArrayRanks);
			return result;
		}

		[Production("arraySuffix: arraySpec")]
		public ParsedTypeName ArraySuffixSingle(ParsedTypeName spec)
		{
			return spec;
		}

		// ==========================================
		// arraySpec: single array dimension specification
		// ==========================================

		[Production("arraySpec: OpenBracket commas CloseBracket")]
		public ParsedTypeName ArraySpecMultiDim(Token<TypeNameToken> open, ParsedTypeName commas, Token<TypeNameToken> close)
		{
			var result = new ParsedTypeName();
			result.ArrayRanks.Add(new ArrayRank() { Rank = commas.GenericArity });
			return result;
		}

		[Production("arraySpec: OpenBracket Asterisk CloseBracket")]
		public ParsedTypeName ArraySpecUnknownBound(Token<TypeNameToken> open, Token<TypeNameToken> asterisk, Token<TypeNameToken> close)
		{
			var result = new ParsedTypeName();
			result.ArrayRanks.Add(new ArrayRank() { Rank = 1, IsUnknownBound = true });
			return result;
		}

		[Production("arraySpec: OpenBracket CloseBracket")]
		public ParsedTypeName ArraySpecSingleDim(Token<TypeNameToken> open, Token<TypeNameToken> close)
		{
			var result = new ParsedTypeName();
			result.ArrayRanks.Add(new ArrayRank() { Rank = 1 });
			return result;
		}

		// ==========================================
		// commas: count commas for multi-dimensional arrays
		// ==========================================

		[Production("commas: Comma commas")]
		public ParsedTypeName CommasCons(Token<TypeNameToken> comma, ParsedTypeName rest)
		{
			rest.GenericArity++;
			return rest;
		}

		[Production("commas: Comma")]
		public ParsedTypeName CommasSingle(Token<TypeNameToken> comma)
		{
			return new ParsedTypeName { GenericArity = 2 }; // One comma = 2 dimensions
		}

		// ==========================================
		// baseType: type with optional generics and assembly
		// ==========================================

		[Production("baseType: qualifiedName GenericArity OpenBracket genericArgs CloseBracket Comma assemblySpec")]
		public ParsedTypeName BaseTypeGenericWithAssembly(
			ParsedTypeName qualifiedName,
			Token<TypeNameToken> arity,
			Token<TypeNameToken> open,
			ParsedTypeName args,
			Token<TypeNameToken> close,
			Token<TypeNameToken> comma,
			ParsedTypeName assembly)
		{
			var arityStr = arity.Value.Substring(1);
			if (int.TryParse(arityStr, out var arityValue))
			{
				qualifiedName.GenericArity = arityValue;
			}
			qualifiedName.GenericArguments.AddRange(args.GenericArguments);
			qualifiedName.AssemblyName = assembly.AssemblyName;
			qualifiedName.AssemblyProperties = assembly.AssemblyProperties;
			return qualifiedName;
		}

		[Production("baseType: qualifiedName GenericArity OpenBracket genericArgs CloseBracket")]
		public ParsedTypeName BaseTypeGeneric(
			ParsedTypeName qualifiedName,
			Token<TypeNameToken> arity,
			Token<TypeNameToken> open,
			ParsedTypeName args,
			Token<TypeNameToken> close)
		{
			var arityStr = arity.Value.Substring(1);
			if (int.TryParse(arityStr, out var arityValue))
			{
				qualifiedName.GenericArity = arityValue;
			}
			qualifiedName.GenericArguments.AddRange(args.GenericArguments);
			return qualifiedName;
		}

		[Production("baseType: qualifiedName Comma assemblySpec")]
		public ParsedTypeName BaseTypeWithAssembly(ParsedTypeName qualifiedName, Token<TypeNameToken> comma, ParsedTypeName assembly)
		{
			qualifiedName.AssemblyName = assembly.AssemblyName;
			qualifiedName.AssemblyProperties = assembly.AssemblyProperties;
			return qualifiedName;
		}

		[Production("baseType: qualifiedName")]
		public ParsedTypeName BaseTypeSimple(ParsedTypeName qualifiedName)
		{
			return qualifiedName;
		}

		// ==========================================
		// genericArgs: generic type arguments
		// ==========================================

		[Production("genericArgs: genericArg Comma genericArgs")]
		public ParsedTypeName GenericArgsCons(ParsedTypeName first, Token<TypeNameToken> comma, ParsedTypeName rest)
		{
			var result = new ParsedTypeName();
			result.GenericArguments.Add(first);
			result.GenericArguments.AddRange(rest.GenericArguments);
			return result;
		}

		[Production("genericArgs: genericArg")]
		public ParsedTypeName GenericArgsSingle(ParsedTypeName arg)
		{
			var result = new ParsedTypeName();
			result.GenericArguments.Add(arg);
			return result;
		}

		[Production("genericArg: OpenBracket typeSpec CloseBracket")]
		public ParsedTypeName GenericArgBracketed(Token<TypeNameToken> open, ParsedTypeName type, Token<TypeNameToken> close)
		{
			return type;
		}

		[Production("genericArg: qualifiedName")]
		public ParsedTypeName GenericArgSimple(ParsedTypeName type)
		{
			return type;
		}

		// ==========================================
		// assemblySpec: assembly name with optional properties
		// ==========================================

		[Production("assemblySpec: Identifier Comma assemblyProps")]
		public ParsedTypeName AssemblySpecWithProps(Token<TypeNameToken> name, Token<TypeNameToken> comma, ParsedTypeName props)
		{
			var result = new ParsedTypeName();
			result.AssemblyName = name.Value;
			result.AssemblyProperties = props.AssemblyProperties;
			return result;
		}

		[Production("assemblySpec: Identifier")]
		public ParsedTypeName AssemblySpecSimple(Token<TypeNameToken> name)
		{
			return new ParsedTypeName { AssemblyName = name.Value };
		}

		// ==========================================
		// assemblyProps: assembly properties
		// ==========================================

		[Production("assemblyProps: assemblyProp Comma assemblyProps")]
		public ParsedTypeName AssemblyPropsCons(ParsedTypeName prop, Token<TypeNameToken> comma, ParsedTypeName rest)
		{
			var result = new ParsedTypeName();
			result.AssemblyProperties = rest.AssemblyProperties ?? new Dictionary<string, string>();
			if (prop.AssemblyProperties != null)
			{
				foreach (var kv in prop.AssemblyProperties)
				{
					result.AssemblyProperties[kv.Key] = kv.Value;
				}
			}
			return result;
		}

		[Production("assemblyProps: assemblyProp")]
		public ParsedTypeName AssemblyPropsSingle(ParsedTypeName prop)
		{
			return prop;
		}

		[Production("assemblyProp: Identifier Equals propValue")]
		public ParsedTypeName AssemblyProp(Token<TypeNameToken> name, Token<TypeNameToken> equals, ParsedTypeName value)
		{
			var result = new ParsedTypeName();
			result.AssemblyProperties = new Dictionary<string, string>
			{
				{ name.Value, value.Namespace ?? string.Empty }
			};
			return result;
		}

		// ==========================================
		// propValue: property value (identifier/number with dots)
		// ==========================================

		[Production("propValue: Identifier Dot propValue")]
		public ParsedTypeName PropValueIdentifierDot(Token<TypeNameToken> first, Token<TypeNameToken> dot, ParsedTypeName rest)
		{
			return new ParsedTypeName { Namespace = first.Value + "." + rest.Namespace };
		}

		[Production("propValue: Number Dot propValue")]
		public ParsedTypeName PropValueNumberDot(Token<TypeNameToken> first, Token<TypeNameToken> dot, ParsedTypeName rest)
		{
			return new ParsedTypeName { Namespace = first.Value + "." + rest.Namespace };
		}

		[Production("propValue: Identifier")]
		public ParsedTypeName PropValueIdentifier(Token<TypeNameToken> id)
		{
			return new ParsedTypeName { Namespace = id.Value };
		}

		[Production("propValue: Number")]
		public ParsedTypeName PropValueNumber(Token<TypeNameToken> num)
		{
			return new ParsedTypeName { Namespace = num.Value };
		}

		// ==========================================
		// qualifiedName: namespace.type+nested
		// ==========================================

		[Production("qualifiedName: Identifier Dot qualifiedName")]
		public ParsedTypeName QualifiedNameDot(Token<TypeNameToken> first, Token<TypeNameToken> dot, ParsedTypeName rest)
		{
			var result = new ParsedTypeName();

			if (rest.TypeNames.Count > 0)
			{
				// rest has type names, first is namespace part
				if (!string.IsNullOrEmpty(rest.Namespace))
				{
					result.Namespace = first.Value + "." + rest.Namespace;
				}
				else
				{
					result.Namespace = first.Value;
				}
				result.TypeNames.AddRange(rest.TypeNames);
			}
			else
			{
				// This shouldn't happen with proper grammar
				result.TypeNames.Add(first.Value);
			}

			return result;
		}

		[Production("qualifiedName: Identifier Plus nestedTypes")]
		public ParsedTypeName QualifiedNameNested(Token<TypeNameToken> first, Token<TypeNameToken> plus, ParsedTypeName nested)
		{
			var result = new ParsedTypeName();
			result.TypeNames.Add(first.Value);
			result.TypeNames.AddRange(nested.TypeNames);
			return result;
		}

		[Production("qualifiedName: Identifier")]
		public ParsedTypeName QualifiedNameSingle(Token<TypeNameToken> id)
		{
			var result = new ParsedTypeName();
			result.TypeNames.Add(id.Value);
			return result;
		}

		// ==========================================
		// nestedTypes: nested type chain
		// ==========================================

		[Production("nestedTypes: Identifier Plus nestedTypes")]
		public ParsedTypeName NestedTypesCons(Token<TypeNameToken> id, Token<TypeNameToken> plus, ParsedTypeName rest)
		{
			var result = new ParsedTypeName();
			result.TypeNames.Add(id.Value);
			result.TypeNames.AddRange(rest.TypeNames);
			return result;
		}

		[Production("nestedTypes: Identifier")]
		public ParsedTypeName NestedTypesSingle(Token<TypeNameToken> id)
		{
			var result = new ParsedTypeName();
			result.TypeNames.Add(id.Value);
			return result;
		}
	}

	public struct ArrayRank
	{
		public int Rank;
		public bool IsUnknownBound;
	}

	/// <summary>
	/// Result of parsing a .NET type name
	/// </summary>
	public class ParsedTypeName
	{
		/// <summary>
		/// The namespace of the type (e.g., "System.Collections.Generic")
		/// </summary>
		public string? Namespace { get; set; }

		/// <summary>
		/// The type names (for nested types, contains multiple entries)
		/// First is the outer type, last is the innermost nested type
		/// </summary>
		public List<string> TypeNames { get; } = new();

		/// <summary>
		/// The generic arity (number of generic type parameters)
		/// </summary>
		public int GenericArity { get; set; }

		/// <summary>
		/// The generic type arguments (for closed generic types)
		/// </summary>
		public List<ParsedTypeName> GenericArguments { get; } = new();

		/// <summary>
		/// Pointer depth (e.g., 2 for "int**")
		/// </summary>
		public int PointerDepth { get; set; }

		/// <summary>
		/// Array ranks (e.g., [1, 2] for "int[][,]" - jagged array of 2D arrays)
		/// </summary>
		public List<ArrayRank> ArrayRanks { get; } = new();

		/// <summary>
		/// Whether this is a reference type (ends with &amp;)
		/// </summary>
		public bool IsReference { get; set; }

		/// <summary>
		/// Assembly name (if specified)
		/// </summary>
		public string? AssemblyName { get; set; }

		/// <summary>
		/// Assembly properties (Version, Culture, PublicKeyToken, etc.)
		/// </summary>
		public Dictionary<string, string>? AssemblyProperties { get; set; }

		/// <summary>
		/// Gets the simple type name (innermost type for nested types)
		/// </summary>
		public string SimpleName => TypeNames.Count > 0 ? TypeNames[TypeNames.Count - 1] : string.Empty;

		/// <summary>
		/// Gets the full type name without assembly qualification
		/// </summary>
		public string FullName
		{
			get
			{
				var sb = new StringBuilder();
				if (!string.IsNullOrEmpty(Namespace))
				{
					sb.Append(Namespace);
					sb.Append('.');
				}
				for (int i = 0; i < TypeNames.Count; i++)
				{
					if (i > 0)
					{
						sb.Append('+');
					}
					sb.Append(TypeNames[i]);
				}
				if (GenericArity > 0)
				{
					sb.Append('`');
					sb.Append(GenericArity);
				}
				sb.Append('*', PointerDepth);
				for (int i = 0; i < ArrayRanks.Count; i++)
				{
					if (ArrayRanks[i].Rank == 1 && ArrayRanks[i].IsUnknownBound)
						sb.Append("[*]");
					else
						sb.Append('[').Append(',', ArrayRanks[i].Rank - 1).Append(']');
				}
				if (IsReference)
				{
					sb.Append('&');
				}

				return sb.ToString();
			}
		}

		public override string ToString() => FullName;

		public string ToCSharp()
		{
			var sb = new StringBuilder();
			if (IsReference)
			{
				sb.Append("ref ");
			}
			if (!string.IsNullOrEmpty(Namespace))
			{
				sb.Append(Namespace);
				sb.Append('.');
			}
			for (int i = 0; i < TypeNames.Count; i++)
			{
				if (i > 0)
				{
					sb.Append('.');
				}
				sb.Append(TypeNames[i]);
			}
			if (GenericArity != 0)
			{
				sb.Append('<');
				for (int i = 0; i < GenericArity; i++)
				{
					if (GenericArguments.Count > i)
					{
						sb.Append(GenericArguments[i].ToCSharp());
					}
					if (i + 1 < GenericArity)
					{
						sb.Append(',');
					}
				}
				sb.Append('>');
			}
			sb.Append('*', PointerDepth);
			for (int i = 0; i < ArrayRanks.Count; i++)
			{
				sb.Append('[').Append(',', ArrayRanks[i].Rank - 1).Append(']');
			}
			return sb.ToString();
		}
	}

	private Parser<TypeNameToken, ParsedTypeName>? _parser;

	/// <summary>
	/// Builds the parser instance
	/// </summary>
	public BuildResult<Parser<TypeNameToken, ParsedTypeName>> BuildParser()
	{
		var parserInstance = new TypeNameParserDefinition();
		var builder = new ParserBuilder<TypeNameToken, ParsedTypeName>();
		return builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "typeSpec");
	}

	/// <summary>
	/// Parses a .NET type name string
	/// </summary>
	/// <param name="typeName">The type name to parse</param>
	/// <returns>The parsed result or null if parsing failed</returns>
	public ParseResult<TypeNameToken, ParsedTypeName>? Parse(string typeName)
	{
		if (_parser == null)
		{
			var buildResult = BuildParser();
			if (buildResult.IsError)
			{
				return null;
			}
			_parser = buildResult.Result;
		}

		return _parser.Parse(typeName);
	}

	/// <summary>
	/// Tries to parse a .NET type name string
	/// </summary>
	/// <param name="typeName">The type name to parse</param>
	/// <param name="result">The parsed result if successful</param>
	/// <returns>True if parsing succeeded</returns>
	public bool TryParse(string typeName, [NotNullWhen(true)] out ParsedTypeName? result)
	{
		result = null;
		var parseResult = Parse(typeName);
		if (parseResult == null || parseResult.IsError)
		{
			return false;
		}
		result = parseResult.Result;
		return true;
	}
}

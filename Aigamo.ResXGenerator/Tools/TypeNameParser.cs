using System;
using System.Collections.Generic;
using System.Text;

namespace Aigamo.ResXGenerator.Tools;

/// <summary>
/// Parser for .NET fully qualified type names.
/// Hand-written recursive descent parser with no third-party dependencies.
/// Grammar based on: https://learn.microsoft.com/en-us/dotnet/fundamentals/reflection/specifying-fully-qualified-type-names
/// </summary>
internal class TypeNameParser
{
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
				if (GenericArguments.Count > 0)
				{
					sb.Append('[');
					for (int i = 0; i < GenericArguments.Count; i++)
					{
						sb.Append('[');
						sb.Append(GenericArguments[i].FullName);
						sb.Append(']');
						if (i < GenericArguments.Count - 1)
						{
							sb.Append(',');
						}
					}
					sb.Append(']');
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

	/// <summary>
	/// Tries to parse a .NET type name string
	/// </summary>
	/// <param name="typeName">The type name to parse</param>
	/// <param name="result">The parsed result if successful</param>
	/// <returns>True if parsing succeeded</returns>
	public bool TryParse(string typeName, [NotNullWhen(true)] out ParsedTypeName? result)
	{
		result = null;
		if (string.IsNullOrEmpty(typeName))
		{
			return false;
		}

		var scanner = new Scanner(typeName);
		var parsed = scanner.ParseTypeSpec();
		if (parsed is null)
		{
			return false;
		}

		// The whole input must be consumed; trailing characters mean the type name was malformed.
		scanner.SkipWhitespace();
		if (!scanner.Eof)
		{
			return false;
		}

		result = parsed;
		return true;
	}

	/// <summary>
	/// A recursive descent scanner over a single type name string.
	/// A fresh instance is used per parse, so parsing is inherently reentrant and thread-safe.
	/// </summary>
	private sealed class Scanner
	{
		private readonly string _s;
		private int _pos;

		public Scanner(string s) => _s = s;

		public bool Eof => _pos >= _s.Length;

		private char Current => _s[_pos];

		public void SkipWhitespace()
		{
			while (_pos < _s.Length && (_s[_pos] == ' ' || _s[_pos] == '\t'))
			{
				_pos++;
			}
		}

		private bool Peek(char c)
		{
			SkipWhitespace();
			return !Eof && Current == c;
		}

		private bool Consume(char c)
		{
			SkipWhitespace();
			if (!Eof && Current == c)
			{
				_pos++;
				return true;
			}
			return false;
		}

		// typeSpec := simpleTypeSpec ( ',' assemblySpec )? '&'?
		public ParsedTypeName? ParseTypeSpec()
		{
			var type = ParseSimpleTypeSpec();
			if (type is null)
			{
				return null;
			}

			if (Peek(','))
			{
				Consume(',');
				var assembly = ParseAssemblySpec();
				if (assembly is null)
				{
					return null;
				}
				type.AssemblyName = assembly.AssemblyName;
				type.AssemblyProperties = assembly.AssemblyProperties;
			}

			if (Consume('&'))
			{
				type.IsReference = true;
			}

			return type;
		}

		// simpleTypeSpec := baseType '*'* arraySpec*
		private ParsedTypeName? ParseSimpleTypeSpec()
		{
			var type = ParseBaseType();
			if (type is null)
			{
				return null;
			}

			while (Peek('*'))
			{
				Consume('*');
				type.PointerDepth++;
			}

			while (TryParseArraySpec(out var rank))
			{
				type.ArrayRanks.Add(rank);
			}

			return type;
		}

		// arraySpec := '[' ']' | '[' '*' ']' | '[' ','+ ']'
		private bool TryParseArraySpec(out ArrayRank rank)
		{
			rank = default;
			SkipWhitespace();
			if (Eof || Current != '[')
			{
				return false;
			}

			var save = _pos;
			_pos++; // consume '['

			SkipWhitespace();
			if (!Eof && Current == '*')
			{
				_pos++;
				if (Consume(']'))
				{
					rank = new ArrayRank { Rank = 1, IsUnknownBound = true };
					return true;
				}
				_pos = save;
				return false;
			}

			var commas = 0;
			while (Peek(','))
			{
				Consume(',');
				commas++;
			}

			if (Consume(']'))
			{
				rank = new ArrayRank { Rank = commas + 1 };
				return true;
			}

			_pos = save;
			return false;
		}

		// baseType := qualifiedName ( '`' number ( genericArgs )? )?
		private ParsedTypeName? ParseBaseType()
		{
			var type = ParseQualifiedName();
			if (type is null)
			{
				return null;
			}

			SkipWhitespace();
			if (!Eof && Current == '`')
			{
				_pos++; // consume '`'
				var start = _pos;
				while (!Eof && char.IsDigit(Current))
				{
					_pos++;
				}
				if (_pos == start)
				{
					return null; // '`' must be followed by the arity
				}
				type.GenericArity = int.Parse(_s.Substring(start, _pos - start));

				if (IsGenericArgsAhead() && !ParseGenericArgs(type))
				{
					return null;
				}
			}

			return type;
		}

		// A '[' following the arity marker starts the generic argument list only when it is not an array
		// spec, i.e. when it is not immediately followed by ']', ',' or '*'.
		private bool IsGenericArgsAhead()
		{
			SkipWhitespace();
			if (Eof || Current != '[')
			{
				return false;
			}

			var i = _pos + 1;
			while (i < _s.Length && (_s[i] == ' ' || _s[i] == '\t'))
			{
				i++;
			}
			if (i >= _s.Length)
			{
				return false;
			}

			var c = _s[i];
			return c != ']' && c != ',' && c != '*';
		}

		// genericArgs := '[' genericArg ( ',' genericArg )* ']'
		private bool ParseGenericArgs(ParsedTypeName type)
		{
			if (!Consume('['))
			{
				return false;
			}

			while (true)
			{
				var arg = ParseGenericArg();
				if (arg is null)
				{
					return false;
				}
				type.GenericArguments.Add(arg);

				if (Peek(','))
				{
					Consume(',');
					continue;
				}
				break;
			}

			return Consume(']');
		}

		// genericArg := '[' typeSpec ']' | baseType
		private ParsedTypeName? ParseGenericArg()
		{
			if (Peek('['))
			{
				Consume('[');
				var inner = ParseTypeSpec();
				if (inner is null || !Consume(']'))
				{
					return null;
				}
				return inner;
			}

			return ParseBaseType();
		}

		// qualifiedName := identifier ( '.' identifier )* ( '+' identifier )*
		// The dotted segments before the last one form the namespace; the last dotted segment plus any
		// '+'-separated segments form the (possibly nested) type name chain.
		private ParsedTypeName? ParseQualifiedName()
		{
			var first = ParseIdentifier();
			if (first is null)
			{
				return null;
			}

			var dotted = new List<string> { first };
			while (Peek('.'))
			{
				Consume('.');
				var next = ParseIdentifier();
				if (next is null)
				{
					return null;
				}
				dotted.Add(next);
			}

			var nested = new List<string>();
			while (Peek('+'))
			{
				Consume('+');
				var next = ParseIdentifier();
				if (next is null)
				{
					return null;
				}
				nested.Add(next);
			}

			var result = new ParsedTypeName();
			if (dotted.Count > 1)
			{
				result.Namespace = string.Join(".", dotted.GetRange(0, dotted.Count - 1));
			}
			result.TypeNames.Add(dotted[dotted.Count - 1]);
			result.TypeNames.AddRange(nested);
			return result;
		}

		// identifier := [A-Za-z_][A-Za-z0-9_]*
		private string? ParseIdentifier()
		{
			SkipWhitespace();
			if (Eof)
			{
				return null;
			}

			var c = Current;
			if (!(char.IsLetter(c) || c == '_'))
			{
				return null;
			}

			var start = _pos;
			_pos++;
			while (!Eof && (char.IsLetterOrDigit(Current) || Current == '_'))
			{
				_pos++;
			}
			return _s.Substring(start, _pos - start);
		}

		// assemblySpec := assemblyName ( ',' assemblyProp )*
		// assemblyProp := key '=' value
		// Stops at the enclosing ']' (nested generic argument), '&', or end of input.
		private ParsedTypeName? ParseAssemblySpec()
		{
			var name = ReadAssemblySegment();
			if (name.Length == 0)
			{
				return null;
			}

			var result = new ParsedTypeName { AssemblyName = name };

			while (Peek(','))
			{
				Consume(',');
				var segment = ReadAssemblySegment();
				var eq = segment.IndexOf('=');
				if (eq < 0)
				{
					continue; // not a key=value property; ignore
				}
				var key = segment.Substring(0, eq).Trim();
				var value = segment.Substring(eq + 1).Trim();
				result.AssemblyProperties ??= new Dictionary<string, string>();
				result.AssemblyProperties[key] = value;
			}

			return result;
		}

		// Reads up to the next ',', ']', '&' or end of input. Assembly names and property values never
		// contain these characters, so this is sufficient for the display-name grammar.
		private string ReadAssemblySegment()
		{
			SkipWhitespace();
			var start = _pos;
			while (!Eof && Current != ',' && Current != ']' && Current != '&')
			{
				_pos++;
			}
			return _s.Substring(start, _pos - start).TrimEnd();
		}
	}
}

using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Aigamo.ResXGenerator.Tools;

public class IntegrityValidator
{
	private readonly HashSet<string> _alreadyAddedMembers = [];
	private TypeNameParser _typeNameParser = new();
	public List<Diagnostic> ErrorsAndWarnings { get; } = [];

	public bool ValidateMember(FallBackItem fallBackItem, GenFileOptions options) => ValidateMember(fallBackItem, options, options.ClassName);

	public bool ValidateMember(FallBackItem fallBackItem, GenFileOptions options, string className)
	{
		var valid = true;

		if (!_alreadyAddedMembers.Add(fallBackItem.Key))
		{
			ErrorsAndWarnings.Add(Diagnostic.Create(
				descriptor: Analyser.DuplicateWarning,
				location: Utilities.LocateMember(fallBackItem, options),
				fallBackItem.Key
			));

			valid = false;
		}

		if (fallBackItem.Key == className)
		{
			ErrorsAndWarnings.Add(Diagnostic.Create(
				descriptor: Analyser.MemberSameAsClassWarning,
				location: Utilities.LocateMember(fallBackItem, options),
				fallBackItem.Key
			));

			valid = false;
		}

		return valid;
	}

	/// <summary>
	/// Cannot have static members/class with a class instance
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	public bool ValidateInconsistentModificator(GenFileOptions options)
	{
		if (options.StaticClass == options.StaticMembers) return true;

		ErrorsAndWarnings.Add(Diagnostic.Create(
			descriptor: Analyser.MemberWithStaticError,
			location: Location.Create(
				filePath: options.GroupedFile.MainFile.File.Path,
				textSpan: new TextSpan(),
				lineSpan: new LinePositionSpan()
			)
		));

		return false;
	}

	public bool ValidateLocalizationModifiers(GenFileOptions options)
	{
		var valid = true;
		if (options.GenerationType != GenerationType.StringLocalizer) return valid;
		if (options.StaticClass || options.StaticMembers)
		{
			ErrorsAndWarnings.Add(Diagnostic.Create(
				descriptor: Analyser.LocalizerStaticError,
				location: Location.Create(
					filePath: options.GroupedFile.MainFile.File.Path,
					textSpan: new TextSpan(),
					lineSpan: new LinePositionSpan()
				)
			));
			valid = false;
		}

		if (options.PartialClass)
		{
			ErrorsAndWarnings.Add(Diagnostic.Create(
				descriptor: Analyser.LocalizerPartialError,
				location: Location.Create(
					filePath: options.GroupedFile.MainFile.File.Path,
					textSpan: new TextSpan(),
					lineSpan: new LinePositionSpan())));

			valid = false;
		}

		return valid;
	}

	public bool ValidateInconsistentNameSpace(GenFileOptions options)
	{
		if (options.GenerationType != GenerationType.StringLocalizer ||
			options.CustomToolNamespace.IsNullOrEmpty() ||
			options.CustomToolNamespace == options.LocalNamespace)
			return true;

		ErrorsAndWarnings.Add(Diagnostic.Create(
			descriptor: Analyser.LocalizationIncoherentNamespace,
			location: Location.Create(
				filePath: options.GroupedFile.MainFile.File.Path,
				textSpan: new TextSpan(),
				lineSpan: new LinePositionSpan()
			)
		));

		return false;

	}

	internal (bool valid, TypeNameParser.ParsedTypeName? typeName) ValidateTypeName(FallBackItem fallbackItem, GenFileOptions options)
	{
		// If no type is specified, this is a string value
		if (string.IsNullOrEmpty(fallbackItem.Type))
		{
			return (true, null);
		}

		return ValidateTypeName(fallbackItem, options, fallbackItem.Type!);
	}

	internal (bool valid, TypeNameParser.ParsedTypeName? typeName) ValidateTypeName(FallBackItem fallbackItem, GenFileOptions options, string inputType)
	{
		// Ensure the type name parses successfully
		if (!_typeNameParser.TryParse(inputType, out var parsedTypeName))
		{
			ErrorsAndWarnings.Add(Diagnostic.Create(
				descriptor: Analyser.TypeNameParseError,
				location: Utilities.LocateMember(fallbackItem, options),
				fallbackItem.Key
			));
			return (false, null);
		}

		// The type name cannot be a pointer or reference
		if (parsedTypeName!.IsReference ||
			parsedTypeName.PointerDepth != 0 ||
			parsedTypeName.ArrayRanks.Count > 1 ||
			(parsedTypeName.ArrayRanks.Count == 1 && parsedTypeName.ArrayRanks[0].Rank != 1))
		{
			// These are expected to be exceedingly rare, so we can just issue a generic type name error
			ErrorsAndWarnings.Add(Diagnostic.Create(
				descriptor: Analyser.TypeNameParseError,
				location: Utilities.LocateMember(fallbackItem, options),
				fallbackItem.Key
			));
			return (false, null);
		}

		return (true, parsedTypeName);
	}

	internal (bool valid, TypeNameParser.ParsedTypeName? typeName) ValidateResXFileRefValue(FallBackItem fallbackItem, GenFileOptions options)
	{
		// ResXFileRef indicates that the string is of the form "<file path>;<type name>[;<encoding]", meaning that the
		// resource compiler actually compiles the contents of the file for the resource. The type used to cast the
		// resource is specified after the first semicolon. However the file name could contain a semicolon and in that
		// case is specified in quotes. See the source of Converter.ResXFileRef for parsing reference.

		string valueTypeName;
		if (fallbackItem.Value.StartsWith("\""))
		{
			var nextQuote = fallbackItem.Value.LastIndexOf('\"');
			if (nextQuote < 1 || nextQuote + 2 > fallbackItem.Value.Length || fallbackItem.Value[nextQuote + 1] != ';')
			{
				// Invalid ResXFileRef format
				ErrorsAndWarnings.Add(Diagnostic.Create(
					descriptor: Analyser.ResXFileRefParseError,
					location: Utilities.LocateMember(fallbackItem, options),
					fallbackItem.Key
				));
				return (false, null);
			}
			valueTypeName = fallbackItem.Value.Substring(nextQuote + 2);
		}
		else
		{
			var firstSemicolon = fallbackItem.Value.IndexOf(';');
			if (firstSemicolon < 0 || firstSemicolon + 1 >= fallbackItem.Value.Length)
			{
				// Invalid ResXFileRef format
				ErrorsAndWarnings.Add(Diagnostic.Create(
					descriptor: Analyser.ResXFileRefParseError,
					location: Utilities.LocateMember(fallbackItem, options),
					fallbackItem.Key
				));
				return (false, null);
			}
			valueTypeName = fallbackItem.Value.Substring(firstSemicolon + 1);
		}

		var nextSemicolon = valueTypeName.IndexOf(";");
		if (nextSemicolon >= 0)
		{
			valueTypeName = valueTypeName.Substring(0, nextSemicolon);
		}

		return ValidateTypeName(fallbackItem, options, valueTypeName);
	}

	internal bool ValidateTypeForCodeGen(TypeNameParser.ParsedTypeName? typeName, FallBackItem fallbackItem, GenFileOptions options)
	{
		// For code generation scenarios only pure strings are allowed right now. Null is treated as string by default.

		if (typeName is not null && typeName is not { FullName: "System.String" })
		{
			ErrorsAndWarnings.Add(Diagnostic.Create(
				descriptor: Analyser.TypeNotSupportedForCodeGen,
				location: Utilities.LocateMember(fallbackItem, options),
				fallbackItem.Key
			));
			return false;
		}
		return true;
	}

}

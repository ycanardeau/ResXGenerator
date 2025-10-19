using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Aigamo.ResXGenerator.Tools;

public class IntegrityValidator
{
	private readonly HashSet<string> _alreadyAddedMembers = [];
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

		if (fallBackItem.Key != className) return valid;

		ErrorsAndWarnings.Add(Diagnostic.Create(
			descriptor: Analyser.MemberSameAsClassWarning,
			location: Utilities.LocateMember(fallBackItem, options),
			fallBackItem.Key
		));

		valid = false;

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
}

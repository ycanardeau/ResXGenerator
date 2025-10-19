using System.Text.RegularExpressions;

namespace Aigamo.ResXGenerator.Tools;

internal static class RegexDefinitions
{
	public static readonly Regex ValidMemberNamePattern = new(
		pattern: @"^[\p{L}\p{Nl}_][\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]*$",
		options: RegexOptions.Compiled | RegexOptions.CultureInvariant
	);

	public static readonly Regex InvalidMemberNameSymbols = new(
		pattern: @"[^\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]",
		options: RegexOptions.Compiled | RegexOptions.CultureInvariant
	);

	public static readonly Regex NewLine = new(
		pattern: @"\r\n|\n\r|\n|\r",
		options: RegexOptions.Compiled | RegexOptions.CultureInvariant
	);
}

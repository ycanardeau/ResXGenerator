using System.Text.RegularExpressions;

namespace Aigamo.ResXGenerator.Tools;

internal static class RegexDefinitions
{
	public static readonly Regex ValidMemberNamePattern = new(@"^[\p{L}\p{Nl}_][\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
	public static readonly Regex InvalidMemberNameSymbols = new(@"[^\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
	public static readonly Regex NewLine = new(@"\r\n|\n\r|\n|\r", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
}

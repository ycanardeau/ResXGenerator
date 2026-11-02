using Aigamo.ResXGenerator.Tools;

namespace Aigamo.ResXGenerator.Extensions;

internal static class StringExtensions
{
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

	public static string ToXmlCommentSafe(this string input) => input.ToXmlCommentSafe(string.Empty);
	public static string ToXmlCommentSafe(this string input, string indent)
	{
		var lines = input.Trim().XmlEscape().GetCodeLines();
		return string.Join($"{Constants.NewLine}{indent}/// ", lines);
	}
	public static string Indent(this string input, int level = 1)
	{
		var indent = new string('\t', level);
		return string.Join($"{Constants.NewLine}{indent}", input.GetCodeLines());
	}

	public static IEnumerable<string> GetCodeLines(this string input) => RegexDefinitions.NewLine.Split(input);

	public static string NormalizeLineEndings(this string input, string newLine = Constants.NewLine) =>
		RegexDefinitions.NewLine.Replace(input, newLine);
	public static (string memberName, bool resourceAccessByName) ToValidMemberName(this string input) =>
		RegexDefinitions.ValidMemberNamePattern.IsMatch(input) ? (input, true) : (RegexDefinitions.InvalidMemberNameSymbols.Replace(input, "_"), false);
	public static string XmlEscape(this string input) => input.Replace("<", "&lt;").Replace(">", "&gt;");
}

using System.Globalization;
using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Models;
using Aigamo.ResXGenerator.Tools;
#nullable disable

namespace Aigamo.ResXGenerator.Generators;

public sealed class ComboGenerator : GeneratorBase<CultureInfoCombo>, IComboGenerator
{
	private const string OutputStringFilenameFormat = "Aigamo.ResXGenerator.{0}.g.cs";
	private static readonly Dictionary<int, List<int>> s_allChildren = new();
	private StringBuilderGeneratorHelper Helper { get; set; }

	/// <summary>
	/// Build all CultureInfo children
	/// </summary>
	static ComboGenerator()
	{
		var all = CultureInfo.GetCultures(CultureTypes.AllCultures);

		all.ForEach(cultureInfo =>
		{
			if (cultureInfo.LCID == 4096 || cultureInfo.IsNeutralCulture || cultureInfo.Name.IsNullOrEmpty())
				return;

			var parent = cultureInfo.Parent;
			if (!s_allChildren.TryGetValue(parent.LCID, out var v))
				s_allChildren[parent.LCID] = v = [];
			v.Add(cultureInfo.LCID);
		});
	}

	public override GeneratedOutput Generate(CultureInfoCombo options, CancellationToken cancellationToken = default)
	{
		Init(options);
		Helper = new StringBuilderGeneratorHelper();

		var definedLanguages = Options.GetDefinedLanguages();

		Helper.AppendHeader("Aigamo.ResXGenerator");

		Helper.AppendLine("internal static partial class Helpers");
		Helper.AppendLine("{");

		Helper.Append("\tpublic static string GetString_");
		var functionNamePostFix = Helper.AppendLanguages(definedLanguages);
		Helper.Append("(string fallback");
		definedLanguages.ForEach(ci =>
		{
			Helper.Append(", ");
			Helper.Append("string ");
			Helper.Append(ci.Name);
		});

		GeneratedFileName = string.Format(OutputStringFilenameFormat, functionNamePostFix);

		Helper.Append(") => ");
		Helper.Append(Constants.SystemGlobalization);
		Helper.AppendLine(".CultureInfo.CurrentUICulture.LCID switch");
		Helper.AppendLine("\t{");
		var already = new HashSet<int>();
		definedLanguages.ForEach(ci =>
		{
			var findParents = FindParents(ci.LCID).Except(already).ToList();
			findParents
				.Select(parent =>
				{
					already.Add(parent);
					return $"\t\t{parent} => {ci.Name.Replace('-', '_')},";
				})
				.ForEach(l => Helper.AppendLine(l));
		});

		Helper.AppendLine("\t\t_ => fallback");
		Helper.AppendLine("\t};");
		Helper.AppendLine("}");

		return Helper.GetOutput(GeneratedFileName, Validator);
	}

	private static IEnumerable<int> FindParents(int toFind) => s_allChildren.TryGetValue(toFind, out var v) ? v.Prepend(toFind) : [toFind];
}

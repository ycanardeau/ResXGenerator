using System.Collections.Immutable;
using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Models;
using Aigamo.ResXGenerator.Tools;
using Microsoft.CodeAnalysis.CSharp;
#nullable disable

namespace Aigamo.ResXGenerator.Generators;

public sealed class CodeGenerator : GeneratorBase<GenFileOptions>, IResXGenerator
{
	private StringBuilderGeneratorHelper Helper { get; set; }

	public override GeneratedOutput Generate(GenFileOptions options, CancellationToken cancellationToken = default)
	{
		Init(options);

		Helper = new StringBuilderGeneratorHelper(Options);

		Content = Options.GroupedFile.MainFile.File.GetText(cancellationToken);
		if (Content is null)
		{
			GeneratedFileName = Options.GroupedFile.MainFile.File.Path;
			Helper.Append("//ERROR reading file:");
			return Helper.GetOutput(GeneratedFileName, Validator);
		}

		GeneratedFileName = $"{Options.LocalNamespace}.{Options.ClassName}.g.cs";

		Helper.AppendHeader(options.CustomToolNamespace ?? options.LocalNamespace);
		Helper.AppendCodeUsings();
		Helper.AppendClassHeader(Options);
		Helper.AppendInnerClass(Options, Validator);
		GenerateCode(cancellationToken);
		Helper.AppendClassFooter(Options);

		return Helper.GetOutput(GeneratedFileName, Validator);
	}

	private void GenerateCode(CancellationToken cancellationToken)
	{
		var combo = new CultureInfoCombo(Options.GroupedFile.SubFiles);
		var definedLanguages = combo.GetDefinedLanguages();

		var fallback = ReadResxFile(Content!);
		var subfiles = definedLanguages.Select(lang =>
		{
			var subcontent = lang.FileWithHash.File.GetText(cancellationToken);
			return subcontent is null
				? null
				: ReadResxFile(subcontent)?
					.GroupBy(x => x.Key)
					.ToImmutableDictionary(x => x.Key, x => x.First().Value);
		}).ToList();

		if (fallback is null || subfiles.Any(x => x is null))
		{
			Helper.AppendFormat("//could not read {0} or one of its children", Options.GroupedFile.MainFile.File.Path);
			return;
		}

		fallback.ForEach(fbi =>
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (Helper.GenerateMember(fbi, Options, Validator) is not { valid: true }) return;

			Helper.Append(" => GetString_");
			Helper.AppendLanguages(definedLanguages);
			Helper.Append("(");
			Helper.Append(SymbolDisplay.FormatLiteral(fbi.Value, true));

			subfiles.ForEach(xml =>
			{
				Helper.Append(", ");
				if (!xml!.TryGetValue(fbi.Key, out var langValue))
					langValue = fbi.Value;
				Helper.Append(SymbolDisplay.FormatLiteral(langValue, true));
			});

			Helper.AppendLine(");");
		});
	}
}

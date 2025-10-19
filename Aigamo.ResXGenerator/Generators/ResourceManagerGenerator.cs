using Aigamo.ResXGenerator.Extensions;
using Aigamo.ResXGenerator.Models;
using Aigamo.ResXGenerator.Tools;
#nullable disable

namespace Aigamo.ResXGenerator.Generators;

public sealed class ResourceManagerGenerator : GeneratorBase<GenFileOptions>, IResXGenerator
{
	private StringBuilderGeneratorHelper Helper { get; set; }

	public override GeneratedOutput Generate(GenFileOptions options, CancellationToken cancellationToken = default)
	{
		Init(options);

		Helper = new StringBuilderGeneratorHelper(options);

		Content = Options.GroupedFile.MainFile.File.GetText(cancellationToken);
		if (Content is null)
		{
			GeneratedFileName = Options.GroupedFile.MainFile.File.Path;
			Helper.Append("//ERROR reading file:");
			return Helper.GetOutput(GeneratedFileName, Validator);
		}

		GeneratedFileName = $"{Options.LocalNamespace}.{Options.ClassName}.g.cs";

		Helper.AppendHeader(Options.CustomToolNamespace ?? Options.LocalNamespace);
		Helper.AppendResourceManagerUsings();
		Helper.AppendClassHeader(options);
		Helper.AppendInnerClass(options, Validator);
		GenerateResourceManager(cancellationToken);
		Helper.AppendClassFooter(options);

		return Helper.GetOutput(GeneratedFileName, Validator);
	}

	private void GenerateResourceManager(CancellationToken cancellationToken)
	{
		Helper.GenerateResourceManagerMembers(Options);

		var members = ReadResxFile(Content!);

		members?.ForEach(fbi =>
		{
			cancellationToken.ThrowIfCancellationRequested();
			CreateMember(fbi);
		});
	}

	private void CreateMember(FallBackItem fallbackItem)
	{
		if (Helper.GenerateMember(fallbackItem, Options, Validator) is not { valid: true } output) return;

		var (_, resourceAccessByName) = output;

		if (resourceAccessByName)
		{
			Helper.Append(" => ResourceManager.GetString(nameof(");
			Helper.Append(fallbackItem.Key);
			Helper.Append("), ");
		}
		else
		{
			Helper.Append(@" => ResourceManager.GetString(""");
			Helper.Append(fallbackItem.Key.Replace(@"""", @"\"""));
			Helper.Append(@""", ");
		}

		Helper.Append(Constants.CultureInfoVariable);
		Helper.Append(")");
		Helper.Append(Options.NullForgivingOperators ? "!" : string.Empty);
		Helper.AppendLine(";");
	}
}

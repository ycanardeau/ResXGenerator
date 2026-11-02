using System.Xml.Linq;
using Aigamo.ResXGenerator.Models;
using Aigamo.ResXGenerator.Tools;
using Microsoft.CodeAnalysis.Text;

namespace Aigamo.ResXGenerator.Generators;

public abstract class GeneratorBase<T> : IGenerator<T>
{
	protected SourceText Content { get; set; } = SourceText.From(string.Empty);
	public string GeneratedFileName { get; protected set; } = string.Empty;
	protected T Options { get; private set; } = default!;
	protected IntegrityValidator Validator { get; private set; } = new IntegrityValidator();

	protected void Init(T options)
	{
		Options = options;
		Validator = new IntegrityValidator();
	}

	public abstract GeneratedOutput Generate(T options, CancellationToken cancellationToken = default);

	protected static IEnumerable<FallBackItem> ReadResxFile(SourceText content)
	{
		using var reader = new StringReader(content.ToString());

		if (XDocument.Load(reader, LoadOptions.SetLineInfo).Root is { } element)
			return element
				.Descendants()
				.Where(static data => data.Name == "data")
				.Select(static data => new FallBackItem(data.Attribute("name")!.Value, data.Descendants("value").First().Value, data.Attribute("type")?.Value, data.Attribute("name")!));

		return [];
	}

	protected GeneratedOutput GetOutput(string sourceCode) => new(GeneratedFileName, sourceCode, Validator.ErrorsAndWarnings);
}

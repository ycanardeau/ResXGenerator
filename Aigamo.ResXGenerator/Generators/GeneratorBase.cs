using System.Xml.Linq;
using Aigamo.ResXGenerator.Models;
using Aigamo.ResXGenerator.Tools;
using Microsoft.CodeAnalysis.Text;
#nullable disable

namespace Aigamo.ResXGenerator.Generators;

public abstract class GeneratorBase<T> : IGenerator<T>
{
	protected SourceText Content { get; set; }
	public string GeneratedFileName { get; protected set; }
	protected T Options { get; private set; }
	protected IntegrityValidator Validator { get; private set; }

	protected void Init(T options)
	{
		Options = options;
		Validator = new IntegrityValidator();
		Content = SourceText.From(string.Empty);
		GeneratedFileName = string.Empty;
	}

	public abstract GeneratedOutput Generate(T options, CancellationToken cancellationToken = default);

	protected static IEnumerable<FallBackItem> ReadResxFile(SourceText content)
	{
		using var reader = new StringReader(content.ToString());

		if (XDocument.Load(reader, LoadOptions.SetLineInfo).Root is { } element)
			return element
				.Descendants()
				.Where(static data => data.Name == "data")
				.Select(static data => new FallBackItem(data.Attribute("name")!.Value, data.Descendants("value").First().Value, data.Attribute("name")!));

		return [];
	}

	protected GeneratedOutput GetOutput(string sourceCode) => new(GeneratedFileName, sourceCode, Validator.ErrorsAndWarnings);
}

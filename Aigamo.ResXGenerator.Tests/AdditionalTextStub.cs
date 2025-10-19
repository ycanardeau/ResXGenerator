using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Aigamo.ResXGenerator.Tests;

internal class AdditionalTextStub(string path, string? text = null) : AdditionalText
{
	private readonly SourceText? _text = text is null ? null : SourceText.From(text);

	public override string Path { get; } = path;

	public override SourceText? GetText(CancellationToken cancellationToken = new()) => _text;
}

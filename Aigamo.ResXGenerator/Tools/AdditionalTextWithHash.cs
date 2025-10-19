using Microsoft.CodeAnalysis;

namespace Aigamo.ResXGenerator.Tools;

public readonly record struct AdditionalTextWithHash(AdditionalText File, Guid Hash)
{
	public bool Equals(AdditionalTextWithHash other) => File.Path.Equals(other.File.Path) && Hash.Equals(other.Hash);

	public override int GetHashCode()
	{
		unchecked
		{
			return File.GetHashCode() * 397 ^ Hash.GetHashCode();
		}
	}

	public override string ToString() => $"{nameof(File)}: {File?.Path}, {nameof(Hash)}: {Hash}";
}

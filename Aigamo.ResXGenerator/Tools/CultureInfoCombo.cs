using System.Globalization;
using Aigamo.ResXGenerator.Models;
using Aigamo.ResXGenerator.Tools;

namespace Aigamo.ResXGenerator;

/// <summary>
/// Note: Equality takes into consideration Iso property only
/// </summary>
public readonly record struct CultureInfoCombo
{
	// order by length desc, so that da-DK comes before da, meaning that it HashSet<int> already doesn't contain da-DK when we process it
	public CultureInfoCombo(IReadOnlyList<AdditionalTextWithHash>? files)
	{
		CultureInfos = files?
			.Select(x => (Path.GetExtension(Path.GetFileNameWithoutExtension(x.File.Path)).TrimStart('.'), y: x))
			.OrderByDescending(x => x.Item1.Length)
			.ThenBy(y => y.Item1)
			.ToList() ?? [];
	}

	public IReadOnlyList<(string Iso, AdditionalTextWithHash File)> CultureInfos { get; }

	public IReadOnlyList<ComboItem> GetDefinedLanguages() => CultureInfos?
		.Select(x => (x.File, new CultureInfo(x.Iso)))
		.Select(x => new ComboItem(x.Item2.Name.Replace('-', '_'), x.Item2.LCID, x.File))
		.ToList() ?? [];

	public bool Equals(CultureInfoCombo other) =>
		(CultureInfos ?? []).Select(x => x.Iso)
		.SequenceEqual(other.CultureInfos?.Select(x => x.Iso) ?? []);

	public override int GetHashCode()
	{
		unchecked
		{
			if (CultureInfos == null) return 0;
			const int seedValue = 0x2D2816FE;
			const int primeNumber = 397;
			return CultureInfos.Aggregate(seedValue, (current, item) => (current * primeNumber) + (Equals(item.Iso, null) ? 0 : item.Iso.GetHashCode()));
		}
	}
}

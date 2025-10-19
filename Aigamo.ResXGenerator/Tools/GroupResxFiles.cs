using Aigamo.ResXGenerator.Extensions;

namespace Aigamo.ResXGenerator.Tools;

public static class GroupResxFiles
{
	public static IEnumerable<GroupedAdditionalFile> Group(IReadOnlyList<AdditionalTextWithHash> allFilesWithHash, CancellationToken cancellationToken = default)
	{
		var lookup = new Dictionary<string, AdditionalTextWithHash>();
		var res = new Dictionary<AdditionalTextWithHash, List<AdditionalTextWithHash>>();
		allFilesWithHash.ForEach(file =>
		{
			cancellationToken.ThrowIfCancellationRequested();

			var path = file.File.Path;
			var pathName = Path.GetDirectoryName(path);
			var baseName = Utilities.GetBaseName(path);
			if (Path.GetFileNameWithoutExtension(path) != baseName) return;

			var key = pathName + "\\" + baseName;
			//it should be impossible to exist already, but VS sometimes throws error about duplicate key added. Keep the original entry, not the new one
			if (!lookup.ContainsKey(key))
				lookup.Add(key, file);
			res.Add(file, []);
		});
		allFilesWithHash.ForEach(fileWithHash =>
		{
			cancellationToken.ThrowIfCancellationRequested();

			var path = fileWithHash.File.Path;
			var pathName = Path.GetDirectoryName(path);
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			var baseName = Utilities.GetBaseName(path);
			if (fileNameWithoutExtension == baseName) return;
			// this might happen if a .nn.resx file exists without a .resx file
			if (!lookup.TryGetValue(pathName + "\\" + baseName, out var additionalText)) return;
			res[additionalText].Add(fileWithHash);
		});
		// don't care at all HOW it is sorted, just that end result is the same
		return res.Select(file =>
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new GroupedAdditionalFile(file.Key, file.Value);
		});
	}

	public static IEnumerable<CultureInfoCombo> DetectChildCombos(IReadOnlyList<GroupedAdditionalFile> groupedAdditionalFiles)
	{
		return groupedAdditionalFiles
			.Select(x => new CultureInfoCombo(x.SubFiles))
			.Distinct().ToList();
	}
}

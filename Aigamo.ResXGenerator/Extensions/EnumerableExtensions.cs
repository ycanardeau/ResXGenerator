namespace Aigamo.ResXGenerator.Extensions;

public static class EnumerableExtensions
{
	public static void ForEach<T>(this IEnumerable<T> col, Action<T> action)
	{
		foreach (var i in col) action(i);
	}
}

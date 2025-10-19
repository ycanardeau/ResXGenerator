namespace Aigamo.ResXGenerator.Extensions;

public static class BoolExtensions
{
	public static T InterpolateCondition<T>(this bool condition, T valueIfTrue, T valueIfFalse) => condition ? valueIfTrue : valueIfFalse;
}

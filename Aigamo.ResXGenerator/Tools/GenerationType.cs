namespace Aigamo.ResXGenerator.Tools;

/// <summary>
/// Specifies the strategy used for generating or retrieving localized resources.
/// </summary>
/// <remarks>Use this enumeration to indicate how localization resources should be obtained or generated within an
/// application. The selected value determines whether resources are managed through a resource manager, generated as
/// code, accessed via a string localizer, or inherit the strategy from an outer context.</remarks>
public enum GenerationType
{
	/// <summary>
	/// When this option chosen the generator will use the classic ResourceManager to get resources string.
	/// See : <see href="https://learn.microsoft.com/en-us/dotnet/api/system.resources.resourcemanager?view=net-9.0">ResourceManager</see>.
	/// </summary>
	ResourceManager,
	/// <summary>
	/// When this option chosen the generator will generate code to get resources string. See README.md (Generate Code (per file or globally)) for more details
	/// </summary>
	CodeGeneration,
	/// <summary>
	/// When this option chosen the generator will generate interfaces and classes to use with
	/// <see href="https://docs.microsoft.com/en-us/dotnet/core/extensions/localization">[Microsoft.Extensions.Localization] `IStringLocalizer&lt;T&gt;`</see>.
	/// To see how to use it see README.md (Using IStringLocalizer)
	/// </summary>
	StringLocalizer,
	/// <summary>
	/// When this option chosen the generator will use the same generation type as the outer class if any. If no outer class exist it will fall back to 'ResourceManager'.
	/// </summary>
	SameAsOuter
}

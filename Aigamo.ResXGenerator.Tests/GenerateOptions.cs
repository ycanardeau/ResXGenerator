using Aigamo.ResXGenerator.Tools;

namespace Aigamo.ResXGenerator.Tests;

internal class GenerateOptions(IResXGenerator generator)
{
	public IResXGenerator Generator { get; } = generator;
	public bool PublicClass { get; set; } = true;
	public bool StaticClass { get; set; } = false;
	public bool Partial { get; set; } = false;
	public bool NullForgivingOperators { get; set; } = false;
	public bool StaticMembers { get; set; } = true;
	public string InnerClassName { get; set; } = "inner";
	public InnerClassVisibility InnerClassVisibility { get; set; } = InnerClassVisibility.SameAsOuter;
	public string InnerClassInstanceName { get; set; } = "";
}

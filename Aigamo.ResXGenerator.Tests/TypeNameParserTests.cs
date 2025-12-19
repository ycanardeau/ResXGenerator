using Aigamo.ResXGenerator.Tools;
using FluentAssertions;
using Xunit;

namespace Aigamo.ResXGenerator.Tests;

public class TypeNameParserTests
{
	private readonly TypeNameParser _parser = new();

	#region Parser Build Diagnostics

	[Fact]
	public void BuildParser_ShouldSucceed()
	{
		var parser = new TypeNameParser();
		var buildResult = parser.BuildParser();

		// Output any errors for debugging
		if (buildResult.IsError)
		{
			var errors = string.Join("\n", buildResult.Errors.Select(e => e.Message));
			buildResult.IsError.Should().BeFalse($"Parser build failed with errors:\n{errors}");
		}

		buildResult.IsError.Should().BeFalse();
		buildResult.Result.Should().NotBeNull();
	}

	#endregion

	#region Simple Type Names

	[Theory]
	[InlineData("String", null, "String")]
	[InlineData("Int32", null, "Int32")]
	[InlineData("MyClass", null, "MyClass")]
	[InlineData("_PrivateClass", null, "_PrivateClass")]
	[InlineData("Class123", null, "Class123")]
	public void Parse_SimpleTypeName_ShouldSucceed(string typeName, string? expectedNamespace, string expectedSimpleName)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be(expectedNamespace);
		parsed.SimpleName.Should().Be(expectedSimpleName);
		parsed.TypeNames.Should().HaveCount(1);
	}

	#endregion

	#region Namespaced Type Names

	[Theory]
	[InlineData("System.String", "System", "String")]
	[InlineData("System.Int32", "System", "Int32")]
	[InlineData("System.Collections.ArrayList", "System.Collections", "ArrayList")]
	[InlineData("System.Collections.Generic.List", "System.Collections.Generic", "List")]
	[InlineData("MyCompany.MyProduct.MyClass", "MyCompany.MyProduct", "MyClass")]
	public void Parse_NamespacedTypeName_ShouldSucceed(string typeName, string expectedNamespace, string expectedSimpleName)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be(expectedNamespace);
		parsed.SimpleName.Should().Be(expectedSimpleName);
	}

	#endregion

	#region Nested Types

	[Theory]
	[InlineData("OuterClass+InnerClass", null, new[] { "OuterClass", "InnerClass" })]
	[InlineData("Outer+Middle+Inner", null, new[] { "Outer", "Middle", "Inner" })]
	public void Parse_NestedTypeName_ShouldSucceed(string typeName, string? expectedNamespace, string[] expectedTypeNames)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be(expectedNamespace);
		parsed.TypeNames.Should().BeEquivalentTo(expectedTypeNames);
		parsed.SimpleName.Should().Be(expectedTypeNames[^1]);
	}

	[Theory]
	[InlineData("System.Environment+SpecialFolder", "System", new[] { "Environment", "SpecialFolder" })]
	[InlineData("MyNamespace.Outer+Inner", "MyNamespace", new[] { "Outer", "Inner" })]
	public void Parse_NamespacedNestedTypeName_ShouldSucceed(string typeName, string expectedNamespace, string[] expectedTypeNames)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be(expectedNamespace);
		parsed.TypeNames.Should().BeEquivalentTo(expectedTypeNames);
	}

	#endregion

	#region Generic Types

	[Fact]
	public void Parse_OpenGenericType_ShouldSucceed()
	{
		// List`1 - open generic with 1 type parameter
		// Note: Open generics without type arguments may not be supported in all grammar variations
		// This tests the basic generic arity recognition
		var typeName = "System.Collections.Generic.List`1";
		typeName.Should().Be(typeof(List<>).FullName);

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be("System.Collections.Generic");
		parsed.SimpleName.Should().Be("List");
		parsed.GenericArity.Should().Be(1);
		parsed.GenericArguments.Should().HaveCount(0);
		parsed.FullName.Should().Be(typeName);
	}

	[Fact]
	public void Parse_ClosedGenericType_SingleArg_ShouldSucceed()
	{
		var typeName = "System.Collections.Generic.List`1[[System.Int32]]";
		//typeName.Should().Be(typeof(List<int>).FullName); // This won't match because Type.FullName uses assembly-qualified names for generic args

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.GenericArity.Should().Be(1);
		parsed.GenericArguments.Should().HaveCount(1);
		parsed.GenericArguments[0].Namespace.Should().Be("System");
		parsed.GenericArguments[0].SimpleName.Should().Be("Int32");
		parsed.FullName.Should().Be(typeName);
	}

	[Fact]
	public void Parse_ClosedGenericType_MultipleArgs_ShouldSucceed()
	{
		var typeName = "System.Collections.Generic.Dictionary`2[[System.String],[System.Int32]]";
		//typeName.Should().Be(typeof(IDictionary<string, int>).FullName); // This won't match because Type.FullName uses assembly-qualified names for generic args

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be("System.Collections.Generic");
		parsed.SimpleName.Should().Be("Dictionary");
		parsed.GenericArity.Should().Be(2);
		parsed.GenericArguments.Should().HaveCount(2);
		parsed.GenericArguments[0].SimpleName.Should().Be("String");
		parsed.GenericArguments[1].SimpleName.Should().Be("Int32");
		parsed.FullName.Should().Be(typeName);
	}

	[Fact]
	public void Parse_NestedGenericType_ShouldSucceed()
	{
		// List of List of String
		var typeName = "System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[System.String]]]]";
		//typeName.Should().Be(typeof(List<List<string>>).FullName); // This won't match because Type.FullName uses assembly-qualified names for generic args

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.GenericArity.Should().Be(1);
		parsed.GenericArguments.Should().HaveCount(1);

		var innerList = parsed.GenericArguments[0];
		innerList.SimpleName.Should().Be("List");
		innerList.GenericArity.Should().Be(1);
		innerList.GenericArguments.Should().HaveCount(1);
		innerList.GenericArguments[0].SimpleName.Should().Be("String");
		// FullName doesn't include the generic argument brackets - it's just the type name with arity marker
		parsed.FullName.Should().Be(typeName);
		parsed.ToCSharp().Should().Be("System.Collections.Generic.List<System.Collections.Generic.List<System.String>>");
	}

	#endregion

	#region Array Types

	[Theory]
	[InlineData("System.Int32[]", "System", "Int32", 1)]
	[InlineData("System.String[]", "System", "String", 1)]
	[InlineData("MyClass[]", null, "MyClass", 1)]
	public void Parse_SingleDimensionArray_ShouldSucceed(string typeName, string? expectedNamespace, string expectedSimpleName, int expectedRank)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be(expectedNamespace);
		parsed.SimpleName.Should().Be(expectedSimpleName);
		parsed.ArrayRanks.Should().HaveCount(1);
		parsed.ArrayRanks[0].Rank.Should().Be(expectedRank);
		parsed.FullName.Should().Be(typeName);
	}

	[Theory]
	[InlineData("System.Int32[,]", 2)]
	[InlineData("System.Int32[,,]", 3)]
	[InlineData("System.Int32[,,,]", 4)]
	public void Parse_MultiDimensionArray_ShouldSucceed(string typeName, int expectedRank)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.ArrayRanks.Should().HaveCount(1);
		parsed.ArrayRanks[0].Rank.Should().Be(expectedRank);
		parsed.FullName.Should().Be(typeName);
	}

	[Fact]
	public void Parse_JaggedArray_ShouldSucceed()
	{
		var typeName = "System.Int32[][]";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.ArrayRanks.Should().HaveCount(2);
		parsed.ArrayRanks[0].Rank.Should().Be(1);
		parsed.ArrayRanks[1].Rank.Should().Be(1);
		parsed.FullName.Should().Be(typeName);
	}

	[Fact]
	public void Parse_ArrayWithUnknownLowerBound_ShouldSucceed()
	{
		var typeName = "System.Int32[*]";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.ArrayRanks.Should().HaveCount(1);
		parsed.FullName.Should().Be(typeName);
	}

	#endregion

	#region Pointer Types

	[Theory]
	[InlineData("System.Int32*", 1)]
	[InlineData("System.Int32**", 2)]
	[InlineData("System.Void*", 1)]
	public void Parse_PointerType_ShouldSucceed(string typeName, int expectedPointerDepth)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.PointerDepth.Should().Be(expectedPointerDepth);
		parsed.FullName.Should().Be(typeName);
	}

	#endregion

	#region Reference Types

	[Theory]
	[InlineData("System.Int32&")]
	[InlineData("MyClass&")]
	public void Parse_ReferenceType_ShouldSucceed(string typeName)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.IsReference.Should().BeTrue();
		parsed.FullName.Should().Be(typeName);
	}

	#endregion

	#region Assembly-Qualified Names

	[Fact]
	public void Parse_SimpleAssemblyQualifiedName_ShouldSucceed()
	{
		var typeName = "MyNamespace.MyClass, MyAssembly";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be("MyNamespace");
		parsed.SimpleName.Should().Be("MyClass");
		parsed.AssemblyName.Should().Be("MyAssembly");
	}

	[Fact]
	public void Parse_SimpleAssemblyQualifiedNameWithSpecialChars_ShouldSucceed()
	{
		var typeName = "MyNamespace.MyClass, My.Assembly-That_Has.Weird_Name";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be("MyNamespace");
		parsed.SimpleName.Should().Be("MyClass");
		parsed.AssemblyName.Should().Be("My.Assembly-That_Has.Weird_Name");
	}

	[Fact]
	public void Parse_FullyQualifiedAssemblyName_ShouldSucceed()
	{
		var typeName = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be("System");
		parsed.SimpleName.Should().Be("String");
		parsed.AssemblyName.Should().Be("mscorlib");
		parsed.AssemblyProperties.Should().NotBeNull();
		parsed.AssemblyProperties.Should().ContainKey("Version");
		parsed.AssemblyProperties!["Version"].Should().Be("4.0.0.0");
		parsed.AssemblyProperties.Should().ContainKey("Culture");
		parsed.AssemblyProperties["Culture"].Should().Be("neutral");
		parsed.AssemblyProperties.Should().ContainKey("PublicKeyToken");
		parsed.AssemblyProperties["PublicKeyToken"].Should().Be("b77a5c561934e089");
	}

	[Fact]
	public void Parse_AssemblyWithCulture_ShouldSucceed()
	{
		// Note: This uses a proper type name format with assembly and culture
		var typeName = "MyNamespace.MyClass, MyAssembly, Culture=en";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be("MyNamespace");
		parsed.SimpleName.Should().Be("MyClass");
		parsed.AssemblyName.Should().Be("MyAssembly");
		parsed.AssemblyProperties.Should().ContainKey("Culture");
		parsed.AssemblyProperties!["Culture"].Should().Be("en");
	}

	#endregion

	#region Combined Complex Types

	[Fact]
	public void Parse_GenericArrayType_ShouldSucceed()
	{
		var typeName = "System.Collections.Generic.List`1[[System.Int32]][]";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.SimpleName.Should().Be("List");
		parsed.GenericArity.Should().Be(1);
		parsed.ArrayRanks.Should().HaveCount(1);
		parsed.GenericArguments.Count.Should().Be(1);
		parsed.GenericArguments[0].FullName.Should().Be("System.Int32");
	}

	[Fact]
	public void Parse_ArrayOfPointers_ShouldSucceed()
	{
		var typeName = "System.Int32*[]";

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.PointerDepth.Should().Be(1);
		parsed.ArrayRanks.Should().HaveCount(1);
		parsed.FullName.Should().Be(typeName);
	}

	#endregion

	#region FullName Property

	[Theory]
	[InlineData("String", "String")]
	[InlineData("System.String", "System.String")]
	[InlineData("Outer+Inner", "Outer+Inner")]
	[InlineData("System.Outer+Inner", "System.Outer+Inner")]
	public void FullName_ShouldReturnCorrectValue(string typeName, string expectedFullName)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.FullName.Should().Be(expectedFullName);
	}

	[Fact]
	public void FullName_GenericType_ShouldIncludeArity()
	{
		var typeName = "System.Collections.Generic.List`1[[System.String]]";
		//typeName.Should().Be(typeof(List<string>).FullName); // This won't match because Type.FullName uses assembly-qualified names for generic args

		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue();
		parsed.Should().NotBeNull();
		parsed!.FullName.Should().Be(typeName);
	}

	#endregion

	#region Error Cases

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Parse_EmptyOrWhitespace_ShouldFail(string typeName)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeFalse();
	}

	[Theory]
	[InlineData("123InvalidStart")]
	[InlineData("Invalid-Name")]
	public void Parse_InvalidIdentifier_ShouldFail(string typeName)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeFalse();
	}

	#endregion

	#region Parser Reuse

	[Fact]
	public void Parse_MultipleCalls_ShouldReuseParser()
	{
		// First parse
		var result1 = _parser.TryParse("System.String", out var parsed1);
		result1.Should().BeTrue();
		parsed1.Should().NotBeNull();

		// Second parse with different type
		var result2 = _parser.TryParse("System.Int32", out var parsed2);
		result2.Should().BeTrue();
		parsed2.Should().NotBeNull();

		// Verify both results are correct
		parsed1!.SimpleName.Should().Be("String");
		parsed2!.SimpleName.Should().Be("Int32");
	}

	#endregion

	#region ResX File Ref Types

	[Theory]
	[InlineData("System.Resources.ResXFileRef, System.Windows.Forms", "System.Resources", "ResXFileRef", "System.Windows.Forms")]
	[InlineData("System.Byte[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System", "Byte", "mscorlib")]
	public void Parse_ResXFileRefType_ShouldSucceed(string typeName, string expectedNamespace, string expectedSimpleName, string expectedAssembly)
	{
		var result = _parser.TryParse(typeName, out var parsed);

		result.Should().BeTrue($"Failed to parse: {typeName}");
		parsed.Should().NotBeNull();
		parsed!.Namespace.Should().Be(expectedNamespace);
		parsed.SimpleName.Should().Be(expectedSimpleName);
		parsed.AssemblyName.Should().Be(expectedAssembly);
	}

	#endregion
}

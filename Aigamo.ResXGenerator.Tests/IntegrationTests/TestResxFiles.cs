using System.Globalization;
using FluentAssertions;
using Xunit;

namespace Aigamo.ResXGenerator.Tests.IntegrationTests;

public class TestResxFiles
{
	[Fact]
	public void TestNormalResourceGen()
	{
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("da");
		Test1.CreateDate.Should().Be("OldestDa");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
		Test1.CreateDate.Should().Be("Oldest");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("ch");
		Test1.CreateDate.Should().Be("Oldest");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
		Test1.CreateDate.Should().Be("OldestEnUs");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("da-DK");
		Test1.CreateDate.Should().Be("OldestDaDK");

		// Test embedded files are as expected
		Test1.TextFile.ReplaceLineEndings().Should().Be("This is a test.\n".ReplaceLineEndings());
		Test1.BinaryFile.Should().BeEquivalentTo(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
		Test1.TestIconAsBytes.Should().NotBeNull();
		Test1.TestImageAsBytes.Should().NotBeNull();
	}
	[Fact]
	public void TestCodeGenResourceGen()
	{
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("da");
		Test2.CreateDate.Should().Be("OldestDa");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
		Test2.CreateDate.Should().Be("Oldest");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("ch");
		Test2.CreateDate.Should().Be("Oldest");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
		Test2.CreateDate.Should().Be("OldestEnUs");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("da-DK");
		Test2.CreateDate.Should().Be("OldestDaDK");
	}

	[Fact]
	public void TestSkipFile_DoesNotGenerate() =>
		GetType().Assembly.GetTypes().Should()
			.NotContain(t => t.Name == "Test3");

	[Fact]
	public void TestLogicalNameMetadata()
	{
		// Simply loading a resource should prove that the LogicalName was interpreted correctly
		Test5.CreateDate.Should().Be("Oldest");
	}
}

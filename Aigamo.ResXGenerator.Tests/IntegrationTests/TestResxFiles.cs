using System.Globalization;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
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
		Test1.TextFile.Should().Be("This is a test.\r\n");
		Test1.BinaryFile.Should().BeEquivalentTo(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
		Test1.TestIconAsBytes.Should().NotBeNull();
		Test1.TestIconAsDrawingIcon.Should().NotBeNull().And.BeOfType<System.Drawing.Icon>().Which.Size.Should().Be(new Size(32, 32));
		Test1.TestImageAsBytes.Should().NotBeNull();
		Test1.TestImageAsDrawingBitmap.Should().NotBeNull().And.BeOfType<System.Drawing.Bitmap>().Which.Size.Should().Be(new Size(32, 32));
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
}

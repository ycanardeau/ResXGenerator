using System.Drawing;
using FluentAssertions;
using Xunit;

namespace Aigamo.ResXGenerator.Tests.Windows;

// These tests cover non-string resources whose generated members are typed as System.Drawing types
// (Icon, Bitmap). Those types are Windows-only, so this project targets net8.0-windows and is kept
// separate from the cross-platform Aigamo.ResXGenerator.Tests project.
public class DrawingResourceTests
{
	[Fact]
	public void LoadsDrawingResources()
	{
		DrawingResources.TestIconAsDrawingIcon.Should().NotBeNull().And.BeOfType<Icon>().Which.Size.Should().Be(new Size(32, 32));
		DrawingResources.TestImageAsDrawingBitmap.Should().NotBeNull().And.BeOfType<Bitmap>().Which.Size.Should().Be(new Size(32, 32));
	}
}

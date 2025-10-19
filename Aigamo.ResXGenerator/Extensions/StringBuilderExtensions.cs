using System.Text;

namespace Aigamo.ResXGenerator.Extensions;

/// <summary>
/// Provides extension methods for <see cref="StringBuilder"/> to append lines using LF ('\n')
/// regardless of the platform's default line ending (CRLF on Windows, LF on Unix/Mac).
/// </summary>
internal static class StringBuilderExtensions
{
	/// <summary>
	/// Appends a line feed character (<c>'\n'</c>) to the end of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <param name="builder">The <see cref="StringBuilder"/> to append to.</param>
	/// <remarks>
	/// This method always appends a LF character instead of the platform-dependent <see cref="Environment.NewLine"/>.
	/// Use this to ensure consistent line endings across Windows, Mac, and Linux.
	/// </remarks>
	public static void AppendLineLF(this StringBuilder builder)
	{
		builder.Append('\n');
	}

	/// <summary>
	/// Appends the specified string followed by a line feed character (<c>'\n'</c>)
	/// to the end of the <see cref="StringBuilder"/>.
	/// </summary>
	/// <param name="builder">The <see cref="StringBuilder"/> to append to.</param>
	/// <param name="value">The string to append before the line feed.</param>
	/// <remarks>
	/// This method always appends a LF character instead of the platform-dependent <see cref="Environment.NewLine"/>.
	/// Use this to ensure consistent line endings across Windows, Mac, and Linux.
	/// </remarks>
	public static void AppendLineLF(this StringBuilder builder, string value)
	{
		builder.Append(value);
		builder.AppendLineLF();
	}
}

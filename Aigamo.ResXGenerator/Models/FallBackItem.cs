using System.Xml;

namespace Aigamo.ResXGenerator.Models;

public record FallBackItem(string Key, string Value, IXmlLineInfo Line);

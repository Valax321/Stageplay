using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Radish.ContentBuilder.Utility;

public static class XmlExtensions
{
    extension(XElement e)
    {
        public bool TryGetAttribute(XName name, [NotNullWhen(true)] out XAttribute? attr)
        {
            attr = e.Attribute(name);
            return attr != null;
        }
    }
}
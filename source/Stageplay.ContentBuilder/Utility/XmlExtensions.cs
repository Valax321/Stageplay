using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Radish.ContentBuilder.Utility;

/// <summary>
/// XML parsing extensions for content tools.
/// </summary>
[PublicAPI]
public static class XmlExtensions
{
    extension(XElement e)
    {
        /// <summary>
        /// Checks if an attribute exists and returns it if it does.
        /// </summary>
        /// <param name="name">The name of the attribute to look for.</param>
        /// <param name="attr">The attribute. If not found, will be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the attribute was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetAttribute(XName name, [NotNullWhen(true)] out XAttribute? attr)
        {
            attr = e.Attribute(name);
            return attr != null;
        }
    }
}
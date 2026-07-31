using System.Xml;

namespace DotNetNuke.Modules.Foundation.Helpers
{
    public static class XmlHelper
    {
        public static string GetAttribute(XmlElement elt, string name)
        {
            if (elt?.Attributes == null) return null;
            var a = elt.Attributes[name];
            return a?.Value;
        }
    }
}
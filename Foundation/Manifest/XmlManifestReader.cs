using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public sealed class XmlManifestReader
    {
        private bool IsDashboard { set; get; } = false;
        private readonly XmlDocument _document =
            new XmlDocument();

        public XmlManifestReader(string path,bool isDashboard)
        {
            _document.Load(path);
            IsDashboard = isDashboard;
        }

        // New constructor to support reading from Stream
        public XmlManifestReader(Stream stream, bool isDashboard)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _document.Load(stream);
            IsDashboard = isDashboard;
        }

        public IEnumerable<ManifestNode> Select(string xpath)
        {
            var list = _document.SelectNodes(xpath);

            if (list == null)
                yield break;

            foreach (XmlElement element in list)
            {
                var attributes =
                    new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase);

                foreach (XmlAttribute attribute in element.Attributes)
                    attributes[attribute.Name] = attribute.Value;

                yield return new ManifestNode(
                    element.Name,
                    element.InnerText.Trim(),
                    attributes);
            }
        }

        public ManifestDocument Load()
        {
            var document = new ManifestDocument();

            ReadScripts(document);
            ReadStyleSheets(document);
            ReadTokens(document);

            return document;
        }


        private void ReadScripts(ManifestDocument document)
        {
            foreach (var script in Select("/manifest/scripts/script"))
            {
                document.Scripts.Add(new ManifestScript
                {
                    Path = script.Value,

                    Enabled = script.Get(ManifestProperties.Common.Enabled, true),
                    Priority = script.Get(ManifestProperties.Common.Priority, 0),
                    Compression = script.Get(ManifestProperties.Common.Compression, true),
                    Provider = ResolveProvider(script.Get(ManifestProperties.Common.Provider)),
                    Tokenization = script.Get(ManifestProperties.Common.Tokenization),
                    CultureAware = CheckCulture(script.Get(ManifestProperties.Common.UseForCulture)),
                    ExcludeDashboard = ExcludeDashboard(script.Get(ManifestProperties.Common.ExcludeDashboard)),

                    Async = script.Get(ManifestProperties.Script.Async, true),
                    Defer = script.Get(ManifestProperties.Script.Defer, false),


                    IsMobile = script.Get(ManifestProperties.Device.IsMobile, false),
                    IsTablet = script.Get(ManifestProperties.Device.IsTablet, false),
                    Browser = script.Get(ManifestProperties.Device.Browser, string.Empty),
                });
            }
        }
        private void ReadStyleSheets(ManifestDocument document)
        {
            foreach (var stylesheet in Select("/manifest/stylesheets/stylesheet"))
            {
                document.StyleSheets.Add(new ManifestStyleSheet
                {
                    Path = stylesheet.Value,


                    Enabled = stylesheet.Get(ManifestProperties.Common.Enabled, true),
                    Priority = stylesheet.Get(ManifestProperties.Common.Priority, 0),
                    Compression = stylesheet.Get(ManifestProperties.Common.Compression, true),
                    Provider = ResolveProvider(stylesheet.Get(ManifestProperties.Common.Provider), true),
                    Tokenization = stylesheet.Get(ManifestProperties.Common.Tokenization),
                    CultureAware = CheckCulture(stylesheet.Get(ManifestProperties.Common.UseForCulture)),
                    ExcludeDashboard = ExcludeDashboard(stylesheet.Get(ManifestProperties.Common.ExcludeDashboard)),


                    Media = stylesheet.Get(ManifestProperties.style.Media, string.Empty),


                    IsMobile = stylesheet.Get(ManifestProperties.Device.IsMobile, false),
                    IsTablet = stylesheet.Get(ManifestProperties.Device.IsTablet, false),
                    Browser = stylesheet.Get(ManifestProperties.Device.Browser, string.Empty),
                });
            }
        }
        private void ReadTokens(ManifestDocument document)
        {
            foreach (var token in Select("/manifest/tokens/token"))
            {
                document.Tokens.Add(new ManifestToken
                {
                    Name = token.Value,
                });
            }
        }



        private readonly Dictionary<string, string> Providers =
         new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
         {
             [DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName] =
                 DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName,

             [DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName] =
                 DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName,

             [DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName] =
                 DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName
         };
        private string ResolveProvider(string provider, bool isStyle = false)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(provider) &&
                Providers.TryGetValue(provider.Trim(), out result))
            {
                return result;
            }

            return isStyle
                ? DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName
                : DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName;
        }
        private bool ExcludeDashboard(bool val)
        {
            return !val
                || !IsDashboard;
        }
        private bool CheckCulture(string culture)
        {
            if (string.IsNullOrEmpty(culture))
                return true;

            return (culture.Equals(System.Globalization.CultureInfo.CurrentCulture.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}

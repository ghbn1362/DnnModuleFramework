using System;
using System.Xml;
using System.IO;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System.Web;

namespace DotNetNuke.Modules.Framework.Services
{
    /// <summary>
    /// Encapsulates path resolution, tokenization, provider selection and helpers used by ResourceService.
    /// </summary>
    public class EnvironmentService : IEnvironmentService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public EnvironmentService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        public string GetResolvedPath(
            string filepath
            , XmlElement scriptElt
            , bool isScript
            , string name
            , PortalSettings portalSettings
            , DotNetNuke.Entities.Modules.ModuleInfo moduleConfig)
        {
            // Similar logic to previous GetResolvedPath with tokenization handling.
            if (string.IsNullOrEmpty(filepath)) return null;
            if (filepath.IsUrl()) return filepath;

            string templatepath = TemplateDirectory(name, portalSettings);
            string result;

            if (filepath.StartsWith("[G]"))
                result = VirtualPathUtility.ToAbsolute(moduleConfig?.ModuleControl?.ControlSrc ?? string.Empty) + filepath.Replace("[G]", "");
            else
            {
                if (templatepath.ToLower().IndexOf("desktopmodules") > 0)
                    templatepath = templatepath.Substring(templatepath.ToLower().IndexOf("desktopmodules"));
                result = VirtualPathUtility.ToAbsolute("~/" + templatepath + filepath);
            }

            if ((scriptElt != null) && (Tokenization(scriptElt)))
            {
                string scheme = System.Web.HttpContext.Current.Request.Url.Scheme + "://";
                string language = System.Globalization.CultureInfo.CurrentCulture.ToString().ToLower();
                string alias = portalSettings.PortalAlias.HTTPAlias.ToLower().Replace(scheme, "").Replace("/" + language, "");
                string url = scheme + alias;
                result = result.Replace(".js", "").Replace(".css", "");
                return url + "/mid/" + moduleConfig.ModuleID + "/c/" + (isScript ? "js" : "css") + "/f/" + System.Web.HttpUtility.UrlEncode(result) + "/Assets.ashx?cdv=" + result.CDV();
            }

            return result;
        }

        public string ResolveTemplateManifestPath(string template, string skin, PortalSettings portalSettings)
        {
            try
            {
                string homeDir = portalSettings.HomeDirectoryMapPath;
                if (homeDir.ToLower().LastIndexOf("portals") > 0)
                    homeDir = homeDir.Substring(0, homeDir.ToLower().LastIndexOf("portals"));

                homeDir = $"{homeDir}{_definition.ModuleDirectory}Templates/";
                string templateFor = string.IsNullOrEmpty(template) ? "Dashboard" : template;

                homeDir += templateFor + "/";
                homeDir = homeDir.Replace("/", @"\").Replace(@"\\", @"\");

                var manifest = Path.Combine(homeDir, Constants.TemplateManifestName);
                return manifest;
            }
            catch
            {
                return null;
            }
        }

        public bool ShouldRegisterCompressed(XmlElement elt) => Compression(elt) && !Tokenization(elt);

        public string GetProvider(XmlElement elt, bool isStyle, int portalId)
        {
            // keep previous Provider behavior
            string styleProvider = DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName.ToLower().Trim();
            string scriptProvider = DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName.ToLower().Trim();

            if ((elt?.Attributes != null) && (elt.Attributes["Provider"] != null))
            {
                string provider = elt.Attributes["Provider"].Value.ToLower().Trim();
                if (provider == DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName;
                if (provider == DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName;
                if (provider == DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName;
            }

            if (isStyle)
            {
                if (styleProvider == DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName;
                if (styleProvider == DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName;
                if (styleProvider == DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName;
            }
            else
            {
                if (scriptProvider == DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName;
                if (scriptProvider == DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnBodyProvider.DefaultName;
                if (scriptProvider == DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName.ToLower().Trim())
                    return DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName;
            }

            return DotNetNuke.Web.Client.Providers.DnnFormBottomProvider.DefaultName;
        }

        public bool GetAsync(XmlElement elt) => Async(elt);
        public bool GetDefer(XmlElement elt) => Defer(elt);


        #region small helpers (extracts from old code)

        public string TemplateDirectory(string template, PortalSettings portalSettings)
        {
            try
            {
                if (portalSettings == null) return string.Empty;

                string HomeDirectory = portalSettings.HomeDirectoryMapPath ?? string.Empty;

                var lower = HomeDirectory.ToLower();
                var lastPortals = lower.LastIndexOf("portals");
                if (lastPortals > 0)
                    HomeDirectory = HomeDirectory.Substring(0, lastPortals);

                HomeDirectory = $"{HomeDirectory}{_definition.ModuleDirectory}Templates/";

                string TemplateFor = string.IsNullOrEmpty(template) ? "Dashboard" : template;

                if (string.IsNullOrEmpty(template))
                {
                    var q = System.Web.HttpContext.Current?.Request?.QueryString;
                    if (q != null)
                    {
                        if (q["ctl"] != null)
                            TemplateFor = q["ctl"].ToString();
                        if (q["sp"] != null)
                            TemplateFor = q["sp"].ToString();
                    }
                }

                HomeDirectory += TemplateFor + "/";
                HomeDirectory = HomeDirectory.Replace("/", @"\").Replace(@"\\", @"\");

                return HomeDirectory;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return string.Empty;
            }
        }

        private bool CheckCulture(XmlElement tag)
        {
            if ((tag?.Attributes != null) && (tag.Attributes["CheckCulture"] != null))
            {
                string culture = tag.Attributes["CheckCulture"].Value.Trim();
                return (culture.Equals(System.Globalization.CultureInfo.CurrentCulture.ToString(), StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        private bool Compression(XmlElement tag)
        {
            if ((tag?.Attributes != null) && (tag.Attributes["Compression"] != null))
            {
                string compression = tag.Attributes["Compression"].Value.Trim();
                return ((compression.ToLower() == "true") || (compression == "1"));
            }
            return true;
        }

        private bool Tokenization(XmlElement tag)
        {
            if ((tag?.Attributes != null) && (tag.Attributes["Tokenization"] != null))
            {
                string tokenization = tag.Attributes["Tokenization"].Value.Trim();
                return ((tokenization.ToLower() == "true") || (tokenization == "1"));
            }
            return false;
        }

        private bool Async(XmlElement tag)
        {
            if ((tag?.Attributes != null) && (tag.Attributes["Async"] != null))
            {
                string Async = tag.Attributes["Async"].Value.Trim();
                return ((Async.ToLower() == "true") || (Async == "1"));
            }
            return true;
        }

        private bool Defer(XmlElement tag)
        {
            if ((tag?.Attributes != null) && (tag.Attributes["Defer"] != null))
            {
                string defer = tag.Attributes["Defer"].Value.Trim();
                return ((defer.ToLower() == "true") || (defer == "1"));
            }
            return false;
        }

        private bool Naked(XmlElement tag, string ModuleName)
        {
            if ((tag?.Attributes != null) && (tag.Attributes["Naked"] != null))
            {
                string Tokenization = tag.Attributes["Naked"].Value.Trim();
                return (Tokenization.ToLower() == "false") || (Tokenization == "0") ||
                       (string.IsNullOrEmpty(ModuleName)) || (!ModuleName.Equals(_definition.ModuleName, StringComparison.OrdinalIgnoreCase));
            }
            return true;
        }

        #endregion
    }
}
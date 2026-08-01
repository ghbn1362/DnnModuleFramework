using System;
using System.Xml;
using System.IO;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System.Web;
using System.Collections.Generic;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public EnvironmentService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        public string GetResolvedPath(
            string filepath
            , bool tokenization
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

            if (tokenization)
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
    }
}
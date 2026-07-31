using System;
using System.IO;
using System.Xml;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Services.Exceptions;
using System.Globalization;

namespace DotNetNuke.Modules.Framework.Services
{
    /// <summary>
    /// Registers scripts/styles based on manifest. Uses EnvironmentService for path resolution and DeviceDetectionService for filtering.
    /// </summary>
    public class ResourceService : IResourceService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public ResourceService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        public void ImportFromManifest(string template, string skin, ref int cssPriority, ref int jsPriority, System.Web.UI.Page page, DotNetNuke.Entities.Modules.ModuleInfo moduleConfig, DotNetNuke.Entities.Portals.PortalSettings portalSettings, IEnvironmentService env)
        {
            try
            {
                var manifestPath = env.ResolveTemplateManifestPath(template, skin, portalSettings);
                if (string.IsNullOrEmpty(manifestPath) || !File.Exists(manifestPath)) return;

                var xml = new XmlDocument();
                xml.Load(manifestPath);

                foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element) continue;
                    var elt = (XmlElement)node;

                    if (elt.LocalName.Equals("scripts", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlElement scriptElt in elt.GetElementsByTagName("script"))
                        {
                            var inner = scriptElt.InnerText?.Trim();
                            if (string.IsNullOrEmpty(inner)) continue;

                            // Device/culture/version checks performed by environment service / device detection
                            var scriptPath = env.GetResolvedPath(inner, scriptElt, true, template, portalSettings, moduleConfig);
                            if (scriptPath == null) continue;

                            if (env.ShouldRegisterCompressed(scriptElt))
                                ClientResourceManager.RegisterScript(page, scriptPath, jsPriority++, env.GetProvider(scriptElt, false, portalSettings.PortalId));
                            else
                            {
                                string key = scriptPath.GetHashCode().ToString();
                                scriptPath = scriptPath + "?cdv=" + scriptPath.CDV();
                                string scriptStr = Common.GenerateScriptTag(scriptPath, env.GetAsync(scriptElt), env.GetDefer(scriptElt));
                                page.ClientScript.RegisterClientScriptBlock(page.GetType(), key, scriptStr);
                            }
                        }
                    }
                    else if (elt.LocalName.Equals("stylesheets", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlElement cssElt in elt.GetElementsByTagName("stylesheet"))
                        {
                            var inner = cssElt.InnerText?.Trim();
                            if (string.IsNullOrEmpty(inner)) continue;

                            var cssPath = env.GetResolvedPath(inner, cssElt, false, template, portalSettings, moduleConfig);
                            if (cssPath == null) continue;

                            if (env.ShouldRegisterCompressed(cssElt))
                                RegisterStyleSheet(page, cssPath, cssPriority++, env.GetProvider(cssElt, true, portalSettings.PortalId), CultureInfo.CurrentCulture);
                            else
                            {
                                string key = cssPath.GetHashCode().ToString();
                                cssPath = cssPath + "?cdv=" + cssPath.CDV();
                                string csslink = "<link href=\"" + cssPath + "\" type=\"text/css\" rel=\"stylesheet\" />";
                                page.ClientScript.RegisterClientScriptBlock(page.GetType(), key, csslink, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private void RegisterStyleSheet(System.Web.UI.Page page, string styleSheet, int priority, string provider, CultureInfo cultureInfo)
        {
            try
            {
                string cultureSkin = !styleSheet.EndsWith(cultureInfo.ToString().ToLower().ToSuffix() + ".css") ? styleSheet.Replace(".css", cultureInfo.ToString().ToLower().ToSuffix() + ".css") : styleSheet;

                if (!cultureSkin.IsUrl())
                {
                    string mapPath = _definition.DirectoryMapPath;
                    if (cultureSkin.ToLower().Contains("DesktopModules".ToLower()))
                        mapPath = mapPath.Substring(0, mapPath.ToLower().IndexOf("DesktopModules".ToLower()));

                    cultureSkin = cultureSkin.Substring(cultureSkin.ToLower().IndexOf("DesktopModules".ToLower()) > 0 ? cultureSkin.ToLower().IndexOf("DesktopModules".ToLower()) : 0);
                    if (File.Exists(mapPath + cultureSkin))
                    {
                        ClientResourceManager.RegisterStyleSheet(page, cultureSkin, priority, provider);
                        return;
                    }
                }

                if ((cultureInfo.TextInfo.IsRightToLeft))
                {
                    string rtlSkin = !styleSheet.EndsWith(".rtl.css") ? styleSheet.Replace(".css", ".rtl.css") : styleSheet;
                    if (!rtlSkin.IsUrl())
                    {
                        string mapPath = _definition.DirectoryMapPath;
                        if (rtlSkin.ToLower().Contains("DesktopModules".ToLower()))
                            mapPath = mapPath.Substring(0, mapPath.ToLower().IndexOf("DesktopModules".ToLower()));

                        rtlSkin = rtlSkin.Substring(rtlSkin.ToLower().IndexOf("DesktopModules".ToLower()) > 0 ? rtlSkin.ToLower().IndexOf("DesktopModules".ToLower()) : 0);
                        if (File.Exists(mapPath + rtlSkin))
                        {
                            ClientResourceManager.RegisterStyleSheet(page, rtlSkin, priority, provider);
                            return;
                        }
                    }
                }

                ClientResourceManager.RegisterStyleSheet(page, styleSheet, priority, provider);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }
    }
}
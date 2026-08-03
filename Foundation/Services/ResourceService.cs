using System;
using System.IO;
using System.Xml;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Services.Exceptions;
using System.Globalization;
using System.Collections.Generic;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class ResourceService : IResourceService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public ResourceService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        public void ImportFromManifest(
            string manifestPath
            , ref int cssPriority
            , ref int jsPriority
            , System.Web.UI.Page page
            , string modulePath
            , int moduleId
            , IEnvironmentService env)
        {
            try
            {
                if (string.IsNullOrEmpty(manifestPath) || !File.Exists(manifestPath)) return;

                Manifest.IManifestReader manifest = new Manifest.ManifestReader();
                Manifest.ManifestDocument manifestDocument = manifest.Load(manifestPath, _definition.UseDashboardSkin);

                if (manifestDocument?.Scripts?.Count > 0)
                    foreach (var script in manifestDocument.Scripts)
                    {
                        if (string.IsNullOrEmpty(script.Path)) continue;

                        var scriptPath = env.GetResolvedPath(
                            script.Path
                            , manifestPath
                            , modulePath
                            , moduleId
                            , script.Tokenization
                            , true);

                        if (scriptPath == null) continue;

                        if (script.Compression)
                            ClientResourceManager.RegisterScript(
                                page
                                , scriptPath
                                , jsPriority++
                                , script.Provider);
                        else
                        {
                            string key = scriptPath.GetHashCode().ToString();
                            scriptPath = scriptPath + "?cdv=" + scriptPath.CDV();

                            string scriptStr = Common.GenerateScriptTag(
                                scriptPath
                                , script.Async
                                , script.Defer);

                            page.ClientScript.RegisterClientScriptBlock(
                                page.GetType()
                                , key
                                , scriptStr);
                        }
                    }

                if (manifestDocument?.StyleSheets?.Count > 0)
                    foreach (var style in manifestDocument.StyleSheets)
                    {
                        if (string.IsNullOrEmpty(style.Path)) continue;

                        var cssPath = env.GetResolvedPath(
                            style.Path
                            , manifestPath
                            , modulePath
                            , moduleId
                            , style.Tokenization
                            , false);

                        if (cssPath == null) continue;

                        if (style.Compression)
                            RegisterStyleSheet(
                                page
                                , cssPath
                                , cssPriority++
                                , style.Provider
                                , CultureInfo.CurrentCulture);
                        else
                        {
                            string key = cssPath.GetHashCode().ToString();
                            cssPath = cssPath + "?cdv=" + cssPath.CDV();
                            string csslink = "<link href=\"" + cssPath + "\" type=\"text/css\" rel=\"stylesheet\" />";
                            page.ClientScript.RegisterClientScriptBlock(page.GetType(), key, csslink, false);
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
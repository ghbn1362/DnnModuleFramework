using System;
using System.IO;
using System.Text.RegularExpressions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public TemplateService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        private static readonly Regex TemplateTagRegex = new Regex(@"{{Template:(?<format>[^?]*?)(:(?<name>[^?]*?))?}}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public string LastRenderedTemplateFilePath { get; private set; } = string.Empty;

        public string LoadTemplatesInHtml(
            string html
            , string skin
            , PortalSettings portalSettings
            , PortalModuleBase moduleConfig)
        {
            if (string.IsNullOrEmpty(html)) return html;

            foreach (Match match in TemplateTagRegex.Matches(html))
            {
                var template = match.Groups["format"].Value.Trim();
                var templateskin = match.Groups["name"]?.Value.Trim();

                string templateHtml = string.Empty;
                try
                {
                    var usedSkin = string.IsNullOrEmpty(templateskin) ? skin : templateskin;
                    templateHtml = RenderTemplateInternal(
                        template
                        , usedSkin
                        , portalSettings
                        , moduleConfig);
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }

                html = html.Replace(match.Value, templateHtml);
            }

            return html;
        }

        private string RenderTemplateInternal(
            string template
            , string skin
            , PortalSettings portalSettings
            , PortalModuleBase moduleConfig)
        {
            try
            {
                string directory = Common.TemplateMapPath(_definition.ModuleDirectory, template, skin);
                string indexCshtml = Path.Combine(directory, "index.cshtml");
                string result = string.Empty;

                if (File.Exists(indexCshtml))
                {
                    LastRenderedTemplateFilePath = indexCshtml;

                    var templatePath = "~/" + indexCshtml.Substring(indexCshtml.IndexOf("DesktopModules")).Replace("\\", "/");
                    var razorEngine = new RazorEngine(templatePath, null, null); // requires reference
                    using (var writer = new StringWriter())
                    {
                        var model = CreateModel(portalSettings, moduleConfig);
                        razorEngine.Render<dynamic>(writer, model);
                        result = writer.ToString().Replace("[at]", "@");
                    }
                }
                else if (File.Exists(indexCshtml.Replace(".cshtml", ".html")))
                {
                    var htmlPath = indexCshtml.Replace(".cshtml", ".html");
                    LastRenderedTemplateFilePath = htmlPath;
                    result = File.ReadAllText(htmlPath);
                }
                else
                {
                    LastRenderedTemplateFilePath = string.Empty;
                }

                return result;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return string.Empty;
            }
        }

        private Core.Module.TemplateModel CreateModel(PortalSettings portalSettings, PortalModuleBase moduleConfig)
        {
            return new Core.Module.TemplateModel
            {
                IsAdmin = System.Web.HttpContext.Current?.User?.IsInRole(portalSettings.AdministratorRoleName) ?? false,
                IsSuperUser = portalSettings?.UserInfo?.IsSuperUser ?? false,
                IsEdit = DotNetNuke.Security.Permissions.TabPermissionController.CanAddContentToPage(portalSettings.ActiveTab),
                TabId = portalSettings.ActiveTab.TabID,
                ModuleId = moduleConfig?.ModuleId ?? -1,
                PortalId = portalSettings.PortalId,
                UserId = portalSettings.UserInfo?.UserID ?? -1,
                Page = "Dashboard",
                LocalResourceFile = moduleConfig?.LocalResourceFile,
                PortalSettings = portalSettings
            };
        }

        public string RenderTemplate(
            string templateName
            , string skin
            , PortalSettings portalSettings
            , PortalModuleBase moduleConfig)
        {
            return RenderTemplateInternal(templateName, skin, portalSettings, moduleConfig);
        }

        //protected string LoadTemplate(string html)
        //{
        //    if (string.IsNullOrEmpty(html)) return html;

        //    // load templates
        //    html = LoadTemplatesInHtml(html, Skin, PortalSettings, this) ?? html;

        //    // import resources referenced by template
        //    ResourceService?.ImportFromManifest(
        //        Common.TemplateManifestMapPath(Definition.ModuleDirectory, Template, Skin)
        //        , ref CssPriority
        //        , ref JsPriority
        //        , Page
        //        , ModuleConfiguration?.ModuleControl?.ControlSrc
        //        , ModuleConfiguration.ModuleID
        //        , EnvironmentService);

        //    // run localization (template-specific resource file handled inside TemplateService or LocalizationService)
        //    html = LocalizationService?.LocalizeHtml(html, Settings, TemplateService?.LastRenderedTemplateFilePath, LocalResourceFile) ?? html;

        //    // token replacements
        //    html = TokenService?.ReplaceAllTokens(html, Request, UserInfo, PortalSettings, Settings) ?? html;

        //    // DotNetNuke TokenReplace (keeps original behavior)
        //    var tokenReplace = new DotNetNuke.Services.Tokens.TokenReplace
        //    {
        //        User = UserInfo,
        //        PortalSettings = PortalSettings,
        //        ModuleId = ModuleId
        //    };
        //    html = tokenReplace.ReplaceEnvironmentTokens(html);

        //    return html;
        //}

    }
}
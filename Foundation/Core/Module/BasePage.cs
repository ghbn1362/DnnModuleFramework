using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.Foundation.Services;
using DotNetNuke.Modules.Foundation.Core;
using System.IO;

namespace DotNetNuke.Modules.Foundation
{
    public abstract class BasePage : Core.Module.ModuleBase
    {

        private string TemplateLocalResourceFile(string file)
        {
            try
            {
                if (string.IsNullOrEmpty(file))
                    return string.Empty;

                string filename = Path.GetFileName(file);
                if (string.IsNullOrEmpty(filename))
                    return string.Empty;

                string dir = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(dir))
                    return string.Empty;

                string appLocalResources = Path.Combine(dir, "App_LocalResources");

                string langSuffix = string.Empty;
                try
                {
                    langSuffix = Common.Language.ToSuffix();
                }
                catch
                {
                    langSuffix = "." + System.Globalization.CultureInfo.CurrentCulture.ToString().ToLower();
                }

                string candidate = Path.Combine(appLocalResources, filename + langSuffix + ".resx");
                if (!File.Exists(candidate))
                {
                    candidate = Path.Combine(appLocalResources, filename + ".resx");
                    if (!File.Exists(candidate))
                    {
                        Exceptions.LogException(new Exception($"TemplateLocalResourceFile: resource not found for template file '{file}' in '{appLocalResources}'"));
                        return string.Empty;
                    }
                }

                string normalized = candidate.Replace("\\", "/");
                int idx = normalized.IndexOf("DesktopModules", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                    return normalized.Substring(idx);

                return normalized;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return string.Empty;
            }
        }
        protected string Template
        {
            get
            {
                try
                {
                    string html = TemplateService.RenderTemplate(string.Empty, Skin, PortalSettings, this);

                    if (!string.IsNullOrEmpty(TemplateService.LastRenderedTemplateFilePath))
                    {
                        var tplResx = TemplateLocalResourceFile(TemplateService.LastRenderedTemplateFilePath);
                        html = LocalizationService?.LocalizeHtml(html, Settings, tplResx, LocalResourceFile) ?? html;
                    }
                    else
                    {
                        html = LocalizationService?.LocalizeHtml(html, Settings, string.Empty, LocalResourceFile) ?? html;
                    }

                    html = TokenService?.ReplaceAllTokens(html, Request, UserInfo, PortalSettings, Settings) ?? html;

                    var tokenReplace = new DotNetNuke.Services.Tokens.TokenReplace
                    {
                        User = UserInfo,
                        PortalSettings = PortalSettings,
                        ModuleId = ModuleId
                    };
                    html = tokenReplace.ReplaceEnvironmentTokens(html);

                    return html ?? string.Empty;
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return string.Empty;
                }
            }
        }
        protected string ServicesFramework(int moduleId)
        {
            int ActiveTabId = PortalSettings.ActiveTab.TabID;
            int tabId = Common.GetModuleById(moduleId).TabID;
            return ActiveTabId != tabId ? $"$.{Definition.ModuleName}ServicesFramework({moduleId},{tabId})" : $"$.ServicesFramework({moduleId})";
        }
        protected string ApiPath
        {
            get
            {
                string Scheme = HttpContext.Current.Request.Url.Scheme + "://";
                string ModulePath = ControlPath.Replace("desktopmodules", "DesktopModules");
                ModulePath = "/" + ModulePath.Substring(ModulePath.IndexOf("DesktopModules"));
                string Language = System.Globalization.CultureInfo.CurrentCulture.ToString().ToLower();
                string Alias = PortalSettings.PortalAlias.HTTPAlias.ToLower().Replace(Scheme, "").Replace("/" + Language, "");

                return Scheme + Alias + ModulePath;
            }
        }
        protected string TokenToday(string html) => TokenService?.TokenToday(html) ?? html;
    }
}
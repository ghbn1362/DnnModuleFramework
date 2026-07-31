using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface ITemplateService
    {
        string LastRenderedTemplateFilePath { get; }
        string LoadTemplatesInHtml(string html, string skin, PortalSettings portalSettings, PortalModuleBase moduleConfig);
        string RenderTemplate(string templateName, string skin, PortalSettings portalSettings, PortalModuleBase moduleConfig);
    }
}
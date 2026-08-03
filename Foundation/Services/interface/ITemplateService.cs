using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface ITemplateService
    {
        string LastRenderedTemplateFilePath { get; }
        string LoadTemplatesInHtml(string html, string skin, Core.Module.TemplateModel model);
        string RenderTemplate(string templateName, string skin, Core.Module.TemplateModel model);
    }
}
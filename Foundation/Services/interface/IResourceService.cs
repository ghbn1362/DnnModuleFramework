using System.Web.UI;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface IResourceService
    {
        void ImportFromManifest(string template, string skin, ref int cssPriority, ref int jsPriority, Page page, DotNetNuke.Entities.Modules.ModuleInfo moduleConfig, DotNetNuke.Entities.Portals.PortalSettings portalSettings, IEnvironmentService environmentService);
    }
}
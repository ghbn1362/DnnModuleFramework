using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Framework.Services
{
    public interface IDashboardService
    {
        void EnsureSkinAndDashboard(System.Web.UI.Page page, PortalModuleBase moduleConfig, PortalSettings portalSettings, System.Collections.Hashtable settings);
    }
}
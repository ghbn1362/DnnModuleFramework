using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface IDashboardService
    {
        void EnsureSkinAndDashboard(
            System.Web.UI.Page page
            , System.Collections.Hashtable settings);
    }
}
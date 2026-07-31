using System.Web;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Framework.Core.Module
{
    /// <summary>
    /// A thin wrapper around ambient context (HttpContext/PortalSettings/ModuleInfo).
    /// Use this in services to make testing easier (mock PageContext).
    /// </summary>
    public class PageContext
    {
        public HttpContextBase HttpContext { get; }
        public PortalSettings PortalSettings { get; }
        public DotNetNuke.Entities.Modules.ModuleInfo Module { get; }

        public PageContext(
            HttpContextBase httpContext
            , PortalSettings portalSettings
            , DotNetNuke.Entities.Modules.ModuleInfo module)
        {
            HttpContext = httpContext;
            PortalSettings = portalSettings;
            Module = module;
        }

        // convenience
        public string ControlPath => Module?.ModuleControl?.ControlSrc != null ? Module.ModuleControl.ControlSrc : string.Empty;
    }
}
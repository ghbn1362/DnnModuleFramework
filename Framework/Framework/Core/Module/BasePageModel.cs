using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Framework.Core
{
    /// <summary>
    /// View model used when rendering templates.
    /// Keep minimal; extend when templates require new properties.
    /// </summary>
    public class BasePageModel
    {
        public bool IsAdmin { get; set; }
        public bool IsSuperUser { get; set; }
        public bool IsEdit { get; set; }
        public int TabId { get; set; }
        public int ModuleId { get; set; }
        public int PortalId { get; set; }
        public int UserId { get; set; }
        public string Page { get; set; }
        public string LocalResourceFile { get; set; }
        public PortalSettings PortalSettings { get; set; }
    }
}
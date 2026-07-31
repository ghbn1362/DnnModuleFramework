using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Core.Module
{
    public class TemplateModel
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
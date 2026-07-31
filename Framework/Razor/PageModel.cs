using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Framework
{
    public class PageModel
    {
        public bool IsAdmin { set; get; }
        public bool IsSuperUser { set; get; }
        public bool IsEdit { set; get; }


        public int PortalId { set; get; }
        public int TabId { set; get; }
        public int ModuleId { set; get; }
        public int UserId { set; get; }


        public string Page { set; get; }
        public string LocalResourceFile { set; get; }
        public DotNetNuke.Entities.Portals.PortalSettings PortalSettings { set; get; }
    }
}
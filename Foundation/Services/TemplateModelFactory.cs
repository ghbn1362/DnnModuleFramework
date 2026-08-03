using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class TemplateModelFactory : ITemplateModelFactory
    {
        public Core.Module.TemplateModel Create(
            Core.Module.ModuleDefinition definition)
        {
            return new Core.Module.TemplateModel
            {
                IsAdmin = definition.IsAdmin,
                IsSuperUser = definition.IsSuperUser,
                IsEdit = definition.IsEdit,
                TabId = definition.TabId,
                ModuleId = definition.ModuleId,
                PortalId = definition.PortalId,
                UserId = definition.UserId,
                Page = "Dashboard",
                LocalResourceFile = definition.LocalResourceFile,
                PortalSettings = definition.PortalSettings
            };
        }
    }
}
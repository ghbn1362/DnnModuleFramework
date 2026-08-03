using System;
using System.Collections;
using System.IO;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public DashboardService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }

        public void EnsureSkinAndDashboard(
            System.Web.UI.Page page
            , Hashtable settings)
        {
            // previous behavior: try to ensure dashboard skin when IsDashboard
            try
            {
                if (_definition.UseDashboardSkin)
                {
                    if (page.Request.Params["ctl"] == null)
                    {
                        string SkinSrc = $"{_definition.ControlPath}{Constants.SkinDirectory}{Constants.SkinName}";
                        string ContainerSrc = $"{_definition.ControlPath}{Constants.SkinDirectory}{Constants.ContainerName}";

                        // try original approach with ControlPath if available
                        if (File.Exists(SkinSrc.MapPath()))
                        {
                            if (!_definition.PortalSettings.ActiveTab.SkinSrc.EndsWith(Constants.SkinName))
                            {
                                _definition.PortalSettings.ActiveTab.SkinSrc = SkinSrc;
                                _definition.PortalSettings.ActiveTab.ContainerSrc = ContainerSrc;
                                _definition.PortalSettings.ActiveTab.EndDate = DotNetNuke.Common.Utilities.Null.NullDate;
                                DotNetNuke.Entities.Tabs.TabController.Instance.UpdateTab(_definition.PortalSettings.ActiveTab);
                                page.Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(_definition.PortalSettings.ActiveTab.TabID));
                            }
                        }
                    }

                    if ((settings[Constants.SettingName_HideAdminBorder] == null 
                        || !bool.Parse(settings[Constants.SettingName_HideAdminBorder].ToString())) 
                        && page.Request.Params["ctl"] == null)
                    {
                        ModuleController.Instance.UpdateTabModuleSetting(_definition.TabModuleId, Constants.SettingName_HideAdminBorder, "true");
                        DotNetNuke.Common.Utilities.Config.Touch();
                        page.Response.Redirect(page.Request.Url.ToString());
                    }

                    if (_definition.PortalSettings.ActiveTab.Modules.Count > 1)
                    {
                        // remove other modules - keep original behavior
                        var modules = ModuleController.Instance.GetTabModules(_definition.PortalSettings.ActiveTab.TabID).Values;
                        foreach (var m in modules)
                        {
                            try
                            {
                                if (!m.ModuleDefinition.DefinitionName.Trim().Equals(_definition.DefinitionName.Trim(), StringComparison.OrdinalIgnoreCase))
                                    ModuleController.Instance.DeleteTabModule(_definition.PortalSettings.ActiveTab.TabID, m.ModuleID, true);
                            }
                            catch { /* ignore deletion errors like original */ }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

    }
}
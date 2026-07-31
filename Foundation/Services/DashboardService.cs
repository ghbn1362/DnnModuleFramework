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
            , PortalModuleBase moduleConfig
            , PortalSettings portalSettings
            , Hashtable settings)
        {
            // previous behavior: try to ensure dashboard skin when IsDashboard
            try
            {
                if (_definition.UseDashboardSkin)
                {
                    if (page.Request.Params["ctl"] == null)
                    {
                        string SkinName = "skin/EditSkin.ascx";
                        string ContainerName = "skin/EditContainer.ascx";
                        string SkinSrc = moduleConfig.ControlPath + "/" + SkinName;
                        string ContainerSrc = moduleConfig.ControlPath + "/" + ContainerName;

                        // try original approach with ControlPath if available
                        if (File.Exists((moduleConfig.ControlPath + "/" + SkinName).MapPath()))
                        {
                            if (!portalSettings.ActiveTab.SkinSrc.EndsWith(SkinName))
                            {
                                portalSettings.ActiveTab.SkinSrc = SkinSrc;
                                portalSettings.ActiveTab.ContainerSrc = ContainerSrc;
                                portalSettings.ActiveTab.EndDate = DotNetNuke.Common.Utilities.Null.NullDate;
                                DotNetNuke.Entities.Tabs.TabController.Instance.UpdateTab(portalSettings.ActiveTab);
                                page.Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(portalSettings.ActiveTab.TabID));
                            }
                        }
                    }

                    if (((settings["hideadminborder"] == null) || (!bool.Parse(settings["hideadminborder"].ToString()))) &&
                        (page.Request.Params["ctl"] == null))
                    {
                        ModuleController.Instance.UpdateTabModuleSetting(moduleConfig.TabModuleId, "hideadminborder", "true");
                        DotNetNuke.Common.Utilities.Config.Touch();
                        page.Response.Redirect(page.Request.Url.ToString());
                    }

                    if (portalSettings.ActiveTab.Modules.Count > 1)
                    {
                        // remove other modules - keep original behavior
                        var modules = ModuleController.Instance.GetTabModules(portalSettings.ActiveTab.TabID).Values;
                        foreach (var m in modules)
                        {
                            try
                            {
                                if (!m.ModuleDefinition.DefinitionName.Trim().Equals(_definition.DefinitionName.Trim(), StringComparison.OrdinalIgnoreCase))
                                    ModuleController.Instance.DeleteTabModule(portalSettings.ActiveTab.TabID, m.ModuleID, true);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Framework.Core.Skin
{
    public sealed class Menu
    {
        #region Public Properties

        public int ModuleId { get; }
        public int TabId { get; }
        public int UserId { get; }
        public int PortalId { get; }
        public string Page { get; }
        public string SubPage { get; }
        public string SkinPath { get; }

        #endregion

        #region Private Fields

        private DotNetNuke.Entities.Modules.ModuleInfo _moduleInfo;
        private DotNetNuke.Entities.Users.UserInfo _userInfo;
        private DotNetNuke.Entities.Portals.PortalSettings _portalSettings;
        private DotNetNuke.Entities.Tabs.TabInfo _tabInfo;

        #endregion

        #region Factory

        public static Menu Create(
            DotNetNuke.Entities.Portals.PortalSettings portalSettings,
            string moduleDefinitionName,
            string skinPath,
            object page,
            object subPage)
        {
            var moduleId = DotNetNuke.Entities.Modules.ModuleController.Instance
                .GetTabModules(portalSettings.ActiveTab.TabID)
                .Values
                .FirstOrDefault(m =>
                    string.Equals(
                        m.ModuleDefinition.DefinitionName,
                        moduleDefinitionName,
                        StringComparison.OrdinalIgnoreCase))
                ?.ModuleID ?? 0;

            return new Menu(
                moduleId,
                portalSettings.ActiveTab.TabID,
                portalSettings.PortalId,
                portalSettings.UserInfo.UserID,
                page,
                subPage,
                skinPath);
        }

        #endregion

        #region Constructor

        private Menu(
            int moduleId,
            int tabId,
            int portalId,
            int userId,
            object page,
            object subPage,
            string skinPath)
        {
            ModuleId = moduleId;
            TabId = tabId;
            PortalId = portalId;
            UserId = userId;

            Page = page?.ToString() ?? string.Empty;
            SubPage = subPage?.ToString() ?? string.Empty;

            SkinPath = skinPath ?? string.Empty;
        }

        #endregion

        #region Cached Objects

        private DotNetNuke.Entities.Modules.ModuleInfo ModuleInfo =>
            _moduleInfo ??
            (_moduleInfo = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(ModuleId, TabId, false));

        private Hashtable Settings =>
            ModuleInfo?.ModuleSettings ?? new Hashtable();

        private DotNetNuke.Entities.Users.UserInfo UserInfo =>
            _userInfo ??
            (_userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(PortalId, UserId));

        private DotNetNuke.Entities.Portals.PortalSettings PortalSettings =>
            _portalSettings ??
            (_portalSettings = new DotNetNuke.Entities.Portals.PortalSettings(PortalId));

        private DotNetNuke.Entities.Tabs.TabInfo TabInfo =>
            _tabInfo ??
            (_tabInfo = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(TabId, PortalId));

        #endregion

        #region Paths

        private string TemplatePath =>
            $"{SkinPath}{Constants.SkinMenuPath}".MapPath();

        #endregion

        #region ViewModel

        private MenuDTO Model
        {
            get
            {
                var currentUser = UserInfo;
                var portal = PortalSettings;

                return new MenuDTO
                {
                    IsAdmin =
                        currentUser?.IsInRole(portal.AdministratorRoleName) ?? false,

                    IsSuperUser =
                        currentUser?.IsSuperUser ?? false,

                    IsEdit =
                        portal.UserInfo.IsSuperUser ||
                        portal.UserInfo.IsInRole(portal.AdministratorRoleName) ||
                        DotNetNuke.Security.Permissions.TabPermissionController.CanAddContentToPage(portal.ActiveTab),

                    ModuleId = ModuleId,
                    PortalId = PortalId,
                    TabId = TabId,
                    UserId = UserId,

                    Page = Page,
                    SubPage = SubPage,

                    SkinPath = SkinPath,
                    LocalResourceFile = $"{SkinPath}{Constants.MenuLocalResourceFile}"
                };
            }
        }

        #endregion

        #region Render

        public string Render()
        {
            var engine = new RazorEngine(TemplatePath, null, null);

            using (var writer = new StringWriter())
            {
                engine.Render(writer, Model);
                return writer.ToString();
            }
        }

        #endregion
    }
}
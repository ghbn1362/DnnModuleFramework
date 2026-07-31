using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.Foundation.Services;
using DotNetNuke.Modules.Foundation.Core;
using System.IO;

namespace DotNetNuke.Modules.Foundation.Core.Module
{
    public abstract class ModuleBase : PortalModuleBase
    {
        protected abstract ModuleDefinition Definition { get; }

        protected string Skin { get; set; }
        protected int CssPriority = 50;
        protected int JsPriority = 50;

        protected readonly ITemplateService TemplateService;
        protected readonly ITokenService TokenService;
        protected readonly IResourceService ResourceService;
        protected readonly IEnvironmentService EnvironmentService;
        protected readonly IDeviceDetectionService DeviceDetectionService;
        protected readonly ILocalizationService LocalizationService;
        protected readonly IDashboardService DashboardService;

        public ModuleBase()
        {
            // resolve services via ServiceFactory (fallback). Replace with DI when available.
            TemplateService = ServiceFactory.Get<ITemplateService>(Definition);
            TokenService = ServiceFactory.Get<ITokenService>(Definition);
            ResourceService = ServiceFactory.Get<IResourceService>(Definition);
            EnvironmentService = ServiceFactory.Get<IEnvironmentService>(Definition);
            DeviceDetectionService = ServiceFactory.Get<IDeviceDetectionService>(Definition);
            LocalizationService = ServiceFactory.Get<ILocalizationService>(Definition);
            DashboardService = ServiceFactory.Get<IDashboardService>(Definition);
        }

        protected virtual void Page_Init(EventArgs e) { }
        protected virtual void Page_Load(object sender, EventArgs e) { }

        protected override void OnInit(EventArgs e)
        {
            CssPriority = JsPriority = 50;
            Skin = Settings[Constants.TemplateSettingsName] != null
                ? Settings[Constants.TemplateSettingsName].ToString()
                : Constants.TemplateDefaultName + "/";

            try
            {
                // enable DNN services support
                DotNetNuke.Framework.ServiceLocator<DotNetNuke.Framework.IServicesFramework, DotNetNuke.Framework.ServicesFramework>.Instance.RequestAjaxAntiForgerySupport();
                DotNetNuke.Framework.ServiceLocator<DotNetNuke.Framework.IServicesFramework, DotNetNuke.Framework.ServicesFramework>.Instance.RequestAjaxScriptSupport();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            // keep compatibility: check skin and dashboard init
            DashboardService?.EnsureSkinAndDashboard(Page, this, PortalSettings, Settings);

            // register essential client script
            try
            {
                ClientResourceManager.RegisterScript(this.Page, ResolveUrl(ControlPath + "js/utility/ServicesFramework.js"), 15, DotNetNuke.Web.Client.Providers.DnnPageHeaderProvider.DefaultName);
            }
            catch { /* swallow if not available; registration isn't critical here */ }

            base.OnInit(e);

            // import resources from manifest for current template
            try
            {
                ResourceService?.ImportFromManifest(string.Empty, Skin, ref CssPriority, ref JsPriority, Page, ModuleConfiguration, PortalSettings, EnvironmentService);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            Page_Init(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page_Load(this, e);
        }
    }
}
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.Foundation.Services.Contracts;

namespace DotNetNuke.Modules.Foundation.Services.Adapters
{
    public class DnnFacade : IDnnFacade
    {
        public int GetCurrentPortalId()
        {
            return PortalSettings.Current?.PortalId ?? -1;
        }

        public string GetHostSetting(string key)
        {
            return HostController.Instance.GetString(key);
        }

        public string GetModuleSetting(int moduleId, string key)
        {
            var mc = new ModuleController();
            var module = mc.GetModule(moduleId);
            if (module?.ModuleSettings == null) return null;
            return module.ModuleSettings.ContainsKey(key) ? module.ModuleSettings[key]?.ToString() : null;
        }
    }
}

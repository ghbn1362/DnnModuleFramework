using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface IEnvironmentService
    {
        string GetResolvedPath(string filepath, bool tokenization, bool isScript, string name, PortalSettings portalSettings, ModuleInfo moduleConfig);
        string ResolveTemplateManifestPath(string template, string skin, PortalSettings portalSettings);
    }
}
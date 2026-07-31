using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface IEnvironmentService
    {
        string GetResolvedPath(string filepath, XmlElement scriptElt, bool isScript, string name, PortalSettings portalSettings, ModuleInfo moduleConfig);
        string ResolveTemplateManifestPath(string template, string skin, PortalSettings portalSettings);
        bool ShouldRegisterCompressed(XmlElement elt);
        string GetProvider(XmlElement elt, bool isStyle, int portalId);
        bool GetAsync(XmlElement elt);
        bool GetDefer(XmlElement elt);
    }
}
using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface IEnvironmentService
    {
        string GetResolvedPath(
            string filepath
            , string manifestPath
            , string modulePath
            , int moduleId
            , bool tokenization
            , bool isScript);

        string TemplateManifestMapPath(string template);
    }
}
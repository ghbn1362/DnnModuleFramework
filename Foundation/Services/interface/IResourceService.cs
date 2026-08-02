using System.Web.UI;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface IResourceService
    {
        void ImportFromManifest(
            string manifestPath
            , ref int cssPriority
            , ref int jsPriority
            , System.Web.UI.Page page
            , string modulePath
            , int moduleId
            , IEnvironmentService env);
    }
}
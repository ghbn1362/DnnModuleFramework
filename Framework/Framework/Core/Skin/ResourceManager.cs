using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client.Providers;
using System.Web.UI;

namespace DotNetNuke.Modules.Framework.Core.Skin
{
    internal static class ResourceManager
    {
        public static void Register(Page page, SkinDefinition definition)
        {
            int cssPriority = 42;

            foreach (string css in definition.StyleSheets)
            {
                ClientResourceManager.RegisterStyleSheet(
                    page,
                    definition.AssetsPath + css,
                    cssPriority++,
                    DnnPageHeaderProvider.DefaultName);
            }

            int jsPriority = 100;

            foreach (string js in definition.Scripts)
            {
                ClientResourceManager.RegisterScript(
                    page,
                    definition.AssetsPath + js,
                    jsPriority++,
                    DnnPageHeaderProvider.DefaultName);
            }
        }
    }
}
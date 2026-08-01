using System;
using System.Web;
using System.Web.UI;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client.Providers;

namespace DotNetNuke.Modules.Foundation.Core.Skin
{
    /// <summary>
    /// Responsible for registering skin resources into DNN.
    /// </summary>
    internal static class ResourceManager
    {
        public static void Register(
            Page page,
            SkinDefinition definition)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (definition == null)
                throw new ArgumentNullException(nameof(definition));


            foreach (var resource in definition.Resources.GetOrdered())
            {
                if (!resource.Enabled)
                    continue;


                string path = ResolvePath(
                    definition.AssetsPath,
                    resource);


                switch (resource.Type)
                {
                    case ResourceType.StyleSheet:

                        RegisterStyleSheet(
                            page,
                            path,
                            resource);

                        break;


                    case ResourceType.Script:

                        RegisterScript(
                            page,
                            path,
                            resource);

                        break;
                }
            }
        }



        private static void RegisterStyleSheet(
            Page page,
            string path,
            ResourceItem resource)
        {
            ClientResourceManager.RegisterStyleSheet(
                page,
                path,
                resource.Priority,
                GetProvider(resource.Location));
        }



        private static void RegisterScript(
            Page page,
            string path,
            ResourceItem resource)
        {
            ClientResourceManager.RegisterScript(
                page,
                path,
                resource.Priority,
                GetProvider(resource.Location));
        }



        private static string ResolvePath(
            string assetsPath,
            ResourceItem resource)
        {
            var path = VirtualPathUtility.Combine(
                assetsPath,
                resource.Path);


            if (!string.IsNullOrWhiteSpace(resource.Version))
            {
                path += "?v=" + resource.Version;
            }


            return path;
        }



        private static string GetProvider(
            ResourceLocation location)
        {
            switch (location)
            {
                case ResourceLocation.Footer:

                    return DnnFormBottomProvider.DefaultName;


                default:

                    return DnnPageHeaderProvider.DefaultName;
            }
        }
    }
}
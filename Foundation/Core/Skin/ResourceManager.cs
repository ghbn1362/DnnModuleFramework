using System;
using System.Web;
using System.Web.UI;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client.Providers;
using DotNetNuke.Modules.Foundation.Manifest;
using DotNetNuke.Modules.Foundation.Services;

namespace DotNetNuke.Modules.Foundation.Core.Skin
{
    /// <summary>
    /// Responsible for registering skin resources into DNN.
    /// </summary>
    internal static class ResourceManager
    {
        public static void Register(
            Page page,
            Core.Module.ModuleDefinition definition)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (definition == null)
                throw new ArgumentNullException(nameof(definition));




            //Manifest.XmlManifestReader manifest =
            //    new Manifest.XmlManifestReader(definition.ManifestPath, true);
            //Manifest.ManifestDocument manifestDocument = manifest.Load();

            //if (manifestDocument?.Scripts?.Count > 0)
            //    RegisterStyles(page, definition, manifestDocument);

            //if (manifestDocument?.StyleSheets?.Count > 0)
            //    RegisterScripts(page, definition, manifestDocument);
        }



        //private static void RegisterStyles(
        //    Page page,
        //    SkinDefinition definition,
        //    ManifestDocument manifest)
        //{
        //    foreach (var style in manifest.StyleSheets)
        //    {
        //        ClientResourceManager.RegisterStyleSheet(
        //            page,
        //            ResolvePath(
        //                style.Path,
        //                style.Version),
        //            style.Priority,
        //            style.Provider);
        //    }
        //}


        //private static void RegisterScripts(
        //    Page page,
        //    SkinDefinition definition,
        //    ManifestDocument manifest)
        //{
        //    foreach (var script in manifest.Scripts)
        //    {
        //        ClientResourceManager.RegisterScript(
        //            page,
        //            ResolvePath(
        //                script.Path,
        //                script.Version),
        //            script.Priority,
        //            script.Provider);
        //    }
        //}

        //private static string ResolvePath(
        //    string path,
        //    string version)
        //{
        //    if (!string.IsNullOrWhiteSpace(version))
        //    {
        //        path += "?v=" + version;
        //    }

        //    return path;
        //}
    }
}
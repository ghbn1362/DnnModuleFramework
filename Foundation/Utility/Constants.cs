using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation
{
    public static class Constants
    {
        internal const string DefaultCompanyName = "Bazrafshan";
        internal const string DefaultTemplatesFolderName = "Templates";
        internal const string SharedResources = "App_LocalResources/SharedResources.resx";

        internal const string TemplateManifestName = "template.config";
        internal const string TemplateDefaultName = "default";
        internal const string TemplateSettingsName = "Template";

        internal const string SkinName = "EditSkin.ascx";
        internal const string ContainerName = "EditContainer.ascx";
        internal const string DefaultMenuFileName = "default.cshtml";

        internal const string SkinDirectory = "Skin/";
        internal const string MenuDirectory = "Menu/";
        internal const string MenuLocalResourceFile = "App_LocalResources/" + SkinName;

        internal const string SkinMenuPath = MenuDirectory + DefaultMenuFileName;
        internal const string ModuleSkinMenuPath = SkinDirectory + SkinMenuPath;
    }
}

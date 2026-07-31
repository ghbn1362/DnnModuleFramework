using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Framework
{
    public static class Constants
    {
        internal const string CompanyName = "Bazrafshan";

        internal const string TemplateManifestName = "template.config";
        internal const string TemplateDefaultName = "default";
        internal const string TemplateSettingsName = "Template";

        internal const string SkinDirectory = "Skin/";
        internal const string MenuDirectory = "Menu/";
        internal const string DefaultMenuFileName = "default.cshtml";
        internal const string MenuLocalResourceFile = "app_localresources/EditSkin.ascx";

        internal const string SkinMenuPath = MenuDirectory + DefaultMenuFileName;
        internal const string ModuleSkinMenuPath = SkinDirectory + SkinMenuPath;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Framework.Core.Skin
{
    public sealed class DefaultSkinDefinition : SkinDefinition
    {
        public DefaultSkinDefinition(Core.Module.ModuleDefinition definition)
        : base(definition)
        {
        }

        public override string AssetsPath
            => $"~{Definition.ModuleDirectory}Skin/Assets/";

        public override IEnumerable<string> StyleSheets
        {
            get
            {
                yield return "css/bootstrap.min.css";
                yield return "css/bootstrapcolors.css";
                yield return "css/font-awesome.min.css";
                yield return "css/style.css";
                yield return "css/switcher.css";
                yield return "css/theme.css";
                yield return "css/dnn.css";
                yield return "css/shared.css";
            }
        }

        public override IEnumerable<string> Scripts
        {
            get
            {
                yield return "js/popper.min.js";
                yield return "js/bootstrap.min.js";
                yield return "js/perfect-scrollbar.min.js";
                yield return "js/sidemenu.js";
                yield return "js/themeColors.js";
                yield return "js/sticky.js";
                yield return "js/custom.js";
                yield return "js/switcher.js";
            }
        }
    }
}
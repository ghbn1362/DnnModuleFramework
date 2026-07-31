using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Core.Skin
{
    public abstract class SkinDefinition
    {
        protected SkinDefinition(Core.Module.ModuleDefinition definition)
        {
            Definition = definition ?? null;
        }

        protected Core.Module.ModuleDefinition Definition { get; }


        /// <summary>
        /// مسیر ریشه Assets
        /// </summary>
        public abstract string AssetsPath { get; }

        /// <summary>
        /// فایل های Css
        /// </summary>
        public virtual IEnumerable<string> StyleSheets
        {
            get { yield break; }
        }

        /// <summary>
        /// فایل های Js
        /// </summary>
        public virtual IEnumerable<string> Scripts
        {
            get { yield break; }
        }

        /// <summary>
        /// آیا CSS پرسونا بار ثبت شود؟
        /// </summary>
        public virtual bool RegisterPersonaBarCss
        {
            get { return true; }
        }
    }
}
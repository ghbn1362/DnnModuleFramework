using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Core.Skin
{
    /// <summary>
    /// Specifies where a client resource should be rendered.
    /// </summary>
    public enum ResourceLocation
    {
        /// <summary>
        /// Render in the page header.
        /// </summary>
        Header,

        /// <summary>
        /// Render before the closing form tag.
        /// </summary>
        Footer
    }
}
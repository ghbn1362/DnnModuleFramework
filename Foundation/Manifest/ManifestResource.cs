using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public abstract class ManifestResource
    {
        public string Path { get; internal set; }


        public bool Enabled { get; internal set; }
        public int Priority { get; internal set; }


        public bool Compression { get; internal set; }
        public bool CultureAware { get; internal set; }
        public bool Tokenization { get; internal set; }
        public bool ExcludeDashboard { get; internal set; }


        public string Provider { get; internal set; }


        public bool IsMobile { get; internal set; }
        public bool IsTablet { get; internal set; }
        public string Browser { get; internal set; }


        public string Version { get; internal set; }
    }
}
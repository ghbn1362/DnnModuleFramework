using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public sealed class ManifestScript : ManifestResource
    {
        public bool Async { get; internal set; }

        public bool Defer { get; internal set; }
    }
}
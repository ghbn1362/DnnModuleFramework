using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public sealed class ManifestDocument
    {
        public IList<ManifestScript> Scripts { get; }

        public IList<ManifestStyleSheet> StyleSheets { get; }

        public IList<ManifestToken> Tokens { get; }

        public ManifestDocument()
        {
            Scripts = new List<ManifestScript>();
            StyleSheets = new List<ManifestStyleSheet>();
            Tokens = new List<ManifestToken>();
        }
    }
}
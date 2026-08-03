using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public class ManifestReader : IManifestReader
    {
        public ManifestDocument Load(string manifestPath, bool excludeDashboard)
        {
            Manifest.XmlManifestReader manifest =
                new Manifest.XmlManifestReader(manifestPath, excludeDashboard);

            Manifest.ManifestDocument manifestDocument = manifest.Load();

            return manifestDocument;
        }
    }
}
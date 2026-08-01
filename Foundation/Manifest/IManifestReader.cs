using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public interface IManifestReader
    {
        ManifestDocument Load(string fileName);
    }
}
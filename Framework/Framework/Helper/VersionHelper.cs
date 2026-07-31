using System;

namespace DotNetNuke.Modules.Framework.Helpers
{
    public static class VersionHelper
    {
        public static bool CheckVersion(Version version, string versions)
        {
            if (version == null) return true;
            Version minversion = null;
            Version maxversion = null;

            if (versions.Contains(","))
            {
                var parts = versions.Split(',');
                if (!string.IsNullOrEmpty(parts[0]) && parts[0].IsVersion()) minversion = new Version(parts[0]);
                if (!string.IsNullOrEmpty(parts[1]) && parts[1].IsVersion()) maxversion = new Version(parts[1]);
            }
            else if (!string.IsNullOrEmpty(versions) && versions.IsVersion())
            {
                minversion = new Version(versions);
            }

            if (minversion != null && maxversion != null)
                return version.CompareTo(minversion) >= 0 && version.CompareTo(maxversion) <= 0;
            if (minversion != null)
                return version.CompareTo(minversion) >= 0;
            if (maxversion != null)
                return version.CompareTo(maxversion) < 0;
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public static class ManifestProperties
    {
        public static class Common
        {
            public static readonly ManifestProperty<bool> Enabled =
                new ManifestProperty<bool>("Enabled");

            public static readonly ManifestProperty<int> Priority =
                new ManifestProperty<int>("Priority");

            public static readonly ManifestProperty<bool> Compression =
                new ManifestProperty<bool>("Compression");

            public static readonly ManifestProperty<bool> Tokenization =
                new ManifestProperty<bool>("Tokenization");

            public static readonly ManifestProperty<bool> ExcludeDashboard =
                new ManifestProperty<bool>("ExcludeDashboard");

            public static readonly ManifestProperty<string> Provider =
                new ManifestProperty<string>("Provider");

            public static readonly ManifestProperty<string> UseForCulture =
                new ManifestProperty<string>("UseForCulture");
        }

        public static class Script
        {
            public static readonly ManifestProperty<bool> Async =
                new ManifestProperty<bool>("Async");

            public static readonly ManifestProperty<bool> Defer =
                new ManifestProperty<bool>("Defer");
        }

        public static class style
        {
            public static readonly ManifestProperty<string> Media =
                new ManifestProperty<string>("Media");
        }

        public static class Device
        {
            public static readonly ManifestProperty<bool> IsMobile =
                new ManifestProperty<bool>("IsMobile");

            public static readonly ManifestProperty<bool> IsTablet =
                new ManifestProperty<bool>("IsTablet");

            public static readonly ManifestProperty<string> Browser =
                new ManifestProperty<string>("Browser");
        }
    }
}
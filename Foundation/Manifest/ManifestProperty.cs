using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public sealed class ManifestProperty<T>
    {
        internal ManifestProperty(
            string name,
            T defaultValue = default(T),
            bool required = false)
        {
            Name = name;
            DefaultValue = defaultValue;
            Required = required;
        }

        public string Name { get; }

        public T DefaultValue { get; }

        public bool Required { get; }
    }
}
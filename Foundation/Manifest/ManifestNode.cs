using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public sealed class ManifestNode
    {
        private readonly Dictionary<string, string> _attributes;

        internal ManifestNode(
            string name,
            string value,
            Dictionary<string, string> attributes)
        {
            Name = name;
            Value = value;
            _attributes = attributes;
        }

        public string Name { get; }

        public string Value { get; }

        public bool Contains<T>(ManifestProperty<T> property)
        {
            return _attributes.ContainsKey(property.Name);
        }

        public T Get<T>(
            ManifestProperty<T> property,
            T defaultValue = default(T))
        {
            string value = "";
            if (!_attributes.TryGetValue(property.Name, out value))
                return defaultValue;

            return ManifestValueConverter.Convert<T>(value);
        }
    }
}
using System;
using System.Collections.Generic;

namespace DotNetNuke.Modules.Foundation.Core.Skin
{
    /// <summary>
    /// Represents a client resource definition.
    /// This class is immutable and describes how a resource should be registered.
    /// </summary>
    public sealed class ResourceItem
    {
        internal ResourceItem(
            ResourceType type,
            string path,
            int priority,
            ResourceLocation location)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "Resource path cannot be empty.",
                    nameof(path));

            Type = type;
            Path = path;
            Priority = priority;
            Location = location;

            Attributes = new Dictionary<string, string>();
        }


        /// <summary>
        /// Resource type.
        /// </summary>
        public ResourceType Type { get; }


        /// <summary>
        /// Relative resource path.
        /// </summary>
        public string Path { get; }


        /// <summary>
        /// Registration priority.
        /// Lower values are registered first.
        /// </summary>
        public int Priority { get; }


        /// <summary>
        /// Resource rendering location.
        /// </summary>
        public ResourceLocation Location { get; }


        /// <summary>
        /// Optional version for cache busting.
        /// Example: ?v=1.0.0
        /// </summary>
        public string Version { get; internal set; }


        /// <summary>
        /// Indicates whether resource should be registered.
        /// </summary>
        public bool Enabled { get; internal set; } = true;


        /// <summary>
        /// Optional resource dependencies.
        /// Example: site.js depends on jquery.js
        /// </summary>
        public IReadOnlyList<string> Dependencies { get; internal set; }
            = new List<string>();


        /// <summary>
        /// Additional provider specific attributes.
        /// </summary>
        public IDictionary<string, string> Attributes { get; }


        public override string ToString()
        {
            return $"{Type}: {Path}";
        }
    }
}
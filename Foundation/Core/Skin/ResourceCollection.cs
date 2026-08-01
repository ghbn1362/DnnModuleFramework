using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetNuke.Modules.Foundation.Core.Skin
{
    /// <summary>
    /// Provides a collection of client resources
    /// registered by a skin definition.
    /// </summary>
    public sealed class ResourceCollection
    {
        private readonly List<ResourceItem> _items;

        internal ResourceCollection()
        {
            _items = new List<ResourceItem>();
        }


        /// <summary>
        /// Gets all registered resources.
        /// </summary>
        public IReadOnlyCollection<ResourceItem> Items =>
            _items.AsReadOnly();


        /// <summary>
        /// Adds a CSS resource.
        /// </summary>
        public ResourceItem Css(
            string path,
            int priority = 50,
            ResourceLocation location = ResourceLocation.Header)
        {
            return Add(
                ResourceType.StyleSheet,
                path,
                priority,
                location);
        }


        /// <summary>
        /// Adds a JavaScript resource.
        /// </summary>
        public ResourceItem Js(
            string path,
            int priority = 100,
            ResourceLocation location = ResourceLocation.Footer)
        {
            return Add(
                ResourceType.Script,
                path,
                priority,
                location);
        }


        /// <summary>
        /// Adds a resource.
        /// </summary>
        public ResourceItem Add(
            ResourceType type,
            string path,
            int priority,
            ResourceLocation location)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "Resource path cannot be empty.",
                    nameof(path));


            var resource = new ResourceItem(
                type,
                path,
                priority,
                location);


            _items.Add(resource);


            return resource;
        }


        /// <summary>
        /// Removes all resources.
        /// </summary>
        internal void Clear()
        {
            _items.Clear();
        }


        /// <summary>
        /// Gets resources ordered by priority.
        /// </summary>
        internal IEnumerable<ResourceItem> GetOrdered()
        {
            return _items
                .Where(x => x.Enabled)
                .OrderBy(x => x.Priority);
        }
    }
}
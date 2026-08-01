using System;

namespace DotNetNuke.Modules.Foundation.Core.Skin
{
    /// <summary>
    /// Base definition for a DNN Skin.
    /// Defines resources and skin configuration.
    /// </summary>
    public abstract class SkinDefinition
    {
        private readonly ResourceCollection _resources;


        protected SkinDefinition(Module.ModuleDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;

            _resources = new ResourceCollection();

            Configure();
        }


        protected Module.ModuleDefinition Definition { get; }


        internal ResourceCollection Resources => _resources;


        protected abstract void Configure();


        public abstract string AssetsPath { get; }


        protected void Css(params string[] files)
        {
            foreach (var file in files)
            {
                _resources.Css(file);
            }
        }


        protected void Js(params string[] files)
        {
            foreach (var file in files)
            {
                _resources.Js(file);
            }
        }

        /// <summary>
        /// Determines whether PersonaBar CSS should be registered.
        /// </summary>
        public virtual bool RegisterPersonaBarCss =>
            true;
    }
}
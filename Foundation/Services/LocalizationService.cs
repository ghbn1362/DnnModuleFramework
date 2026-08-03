using System.Collections;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public LocalizationService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        public string LocalizeHtml(
            string html
            , Hashtable moduleSettings
            , string templateLocalResourceFile
            , string localResourceFile)
        {
            if (string.IsNullOrEmpty(html)) return html;

            // wrap existing Common.Localization calls to keep behavior
            if (!string.IsNullOrEmpty(templateLocalResourceFile))
                html = Common.Localization(moduleSettings, html, templateLocalResourceFile);

            if (!string.IsNullOrEmpty(localResourceFile))
                html = Common.Localization(moduleSettings, html, localResourceFile);

            html = Common.Localization(moduleSettings, html, Common.SharedResourceFile(_definition.ModuleDirectory));

            return html;
        }
    }
}
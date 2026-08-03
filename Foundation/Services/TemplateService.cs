using System;
using System.IO;
using System.Text.RegularExpressions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public TemplateService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        private static readonly Regex TemplateTagRegex = new Regex(@"{{Template:(?<format>[^?]*?)(:(?<name>[^?]*?))?}}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public string LastRenderedTemplateFilePath { get; private set; } = string.Empty;

        public string LoadTemplatesInHtml(
            string html
            , string skin
            , Core.Module.TemplateModel model)
        {
            if (string.IsNullOrEmpty(html)) return html;

            foreach (Match match in TemplateTagRegex.Matches(html))
            {
                var template = match.Groups["format"].Value.Trim();
                var templateskin = match.Groups["name"]?.Value.Trim();

                string templateHtml = string.Empty;
                try
                {
                    var usedSkin = string.IsNullOrEmpty(templateskin) ? skin : templateskin;
                    templateHtml = RenderTemplateInternal(
                        template
                        , usedSkin
                        , model);
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }

                html = html.Replace(match.Value, templateHtml);
            }

            return html;
        }

        private string RenderTemplateInternal(
            string template
            , string skin
            , Core.Module.TemplateModel model)
        {
            try
            {
                string directory = Common.TemplateMapPath(_definition.ModuleDirectory, template, skin);
                string indexCshtml = Path.Combine(directory, "index.cshtml");
                string result = string.Empty;

                if (File.Exists(indexCshtml))
                {
                    LastRenderedTemplateFilePath = indexCshtml;

                    var templatePath = "~/" + indexCshtml.Substring(indexCshtml.IndexOf("DesktopModules")).Replace("\\", "/");
                    var razorEngine = new RazorEngine(templatePath, null, null); // requires reference
                    using (var writer = new StringWriter())
                    {
                        razorEngine.Render<dynamic>(writer, model);
                        result = writer.ToString().Replace("[at]", "@");
                    }
                }
                else if (File.Exists(indexCshtml.Replace(".cshtml", ".html")))
                {
                    var htmlPath = indexCshtml.Replace(".cshtml", ".html");
                    LastRenderedTemplateFilePath = htmlPath;
                    result = File.ReadAllText(htmlPath);
                }
                else
                {
                    LastRenderedTemplateFilePath = string.Empty;
                }

                return result;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return string.Empty;
            }
        }


        public string RenderTemplate(
            string templateName
            , string skin
            , Core.Module.TemplateModel model)
        {
            return RenderTemplateInternal(templateName, skin, model);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Core.Module
{
    public sealed class ModuleDefinition
    {
        public string CompanyName { get; } = Constants.DefaultCompanyName;

        public string ModuleName { get; }

        public string DefinitionName { get; }

        public string FriendlyName { get; }

        public bool UseDashboardSkin { get; }

        public string TemplatesFolderName { get; } = Constants.DefaultTemplatesFolderName;

        public ModuleDefinition(
            string moduleName,
            string definitionName,
            string friendlyName)
        {
            ModuleName = moduleName;
            DefinitionName = definitionName;
            FriendlyName = friendlyName;
        }

        public string ModuleDirectory =>
            $"/DesktopModules/{CompanyName}/{ModuleName}/";

        public string SharedResourceFile =>
            $"~{ModuleDirectory}{Constants.SharedResources}";

        public string ControlPath =>
            "~/" + ModuleDirectory;

        public string ModuleVirtualPath =>
            VirtualPathUtility.ToAbsolute($"~{ModuleDirectory}");

        public string DirectoryMapPath =>
            ModuleDirectory.MapPath();

        public string TemplateDirectoryMapPath =>
            Path.Combine(DirectoryMapPath, TemplatesFolderName);

        public string MenuTemplatePath =>
            $"~{ModuleDirectory}{Constants.ModuleSkinMenuPath}";
    }
}
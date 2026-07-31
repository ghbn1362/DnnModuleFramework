using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Core.Module
{
    public sealed class ModuleDefinition
    {
        public  string CompanyName { get; } = Constants.DefaultCompanyName;
        public string TemplatesFolderName { get; } = Constants.DefaultTemplatesFolderName;
        public string ModuleName { get; }
        public string DefinitionName { get; }
        public string FriendlyName { get; }
        public bool UseDashboardSkin { get; }


        public ModuleDefinition(
            string moduleName,
            string definitionName,
            string friendlyName)
        {
            ModuleName = moduleName;
            DefinitionName = definitionName;
            FriendlyName = friendlyName;
        }
        public ModuleDefinition(
            string moduleName,
            string definitionName,
            string friendlyName,
            bool useDashboardSkin,
            string companyName = Constants.DefaultCompanyName,
            string templatesFolderName = Constants.DefaultTemplatesFolderName)
        {
            ModuleName = moduleName;
            DefinitionName = definitionName;
            FriendlyName = friendlyName;
            UseDashboardSkin = useDashboardSkin;
            CompanyName = companyName;
            TemplatesFolderName = templatesFolderName;
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
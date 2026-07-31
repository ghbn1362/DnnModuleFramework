using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Framework
{
    public sealed class ModuleDefinition
    {
        public string ModuleName { get; }

        public string DefinitionName { get; }

        public string FriendlyName { get; }

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
            $"/DesktopModules/{Constants.CompanyName}/{ModuleName}/";

        public string SharedResourceFile =>
            $"~/DesktopModules/{ModuleName}/App_LocalResources/SharedResources.resx";

        public string ControlPath =>
            "~/" + ModuleDirectory;

        public string ModuleVirtualPath =>
            VirtualPathUtility.ToAbsolute("~" + ModuleDirectory);

        public string DirectoryMapPath =>
            ModuleDirectory.MapPath();

        public string TemplateDirectoryMapPath =>
            Path.Combine(DirectoryMapPath, "Templates");

        public string MenuTemplatePath => 
            "~" + ModuleDirectory + Constants.ModuleSkinMenuPath;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Services
{

    public interface ITemplateModelFactory
    {
        Core.Module.TemplateModel Create(
            Core.Module.ModuleDefinition definition);
    }
}
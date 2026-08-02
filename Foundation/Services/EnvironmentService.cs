using System;
using System.Xml;
using System.IO;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System.Web;
using System.Collections.Generic;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public EnvironmentService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        public string GetResolvedPath(
            string manifestAssetPath
            , string manifestPath
            , string modulePath
            , int moduleId
            , bool tokenization
            , bool isScript)
        {
            if (string.IsNullOrEmpty(manifestAssetPath)) return null;
            if (manifestAssetPath.IsUrl()) return manifestAssetPath;

            string result;

            if (manifestAssetPath.StartsWith("[G]"))
                result = $"{modulePath.DirectoryPath()}{manifestAssetPath.Replace("[G]", "")}".ResolveUrl();
            else
            {
                result = $"{manifestPath.DirectoryPath()}{manifestAssetPath}".ResolveUrl();
            }

            if (tokenization && moduleId > 0)
            {
                var request = HttpContext.Current.Request;
                string baseUrl = request.Url.GetLeftPart(UriPartial.Authority);

                if (!string.IsNullOrEmpty(request.ApplicationPath) &&
                    request.ApplicationPath != "/")
                {
                    baseUrl += request.ApplicationPath;
                }

                return $"{baseUrl}/mid/{moduleId}/c/{(isScript ? "js" : "css")}/f/{HttpUtility.UrlEncode(result.Replace(".js", "").Replace(".css", ""))}/Assets.ashx?cdv={result.CDV()}";
            }

            return result;
        }
    }
}
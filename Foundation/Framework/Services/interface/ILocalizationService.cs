using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Services
{

    public interface ILocalizationService
    {
        string LocalizeHtml(string html, Hashtable moduleSettings, string templateLocalResourceFile, string localResourceFile);
    }
}
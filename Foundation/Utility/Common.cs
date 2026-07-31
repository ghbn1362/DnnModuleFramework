using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.Foundation
{
    public sealed class Common
    {

        public static string CultureCode
        {
            get
            {
                string cultureCode = string.Empty;
                string PortalCultureCode = string.Empty;
                string CookiesCultureCode = string.Empty;

                try
                {
                    cultureCode = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

                    PortalSettings PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    if (PortalSettings != null)
                    {
                        if ((PortalSettings.ActiveTab != null) &&
                            (!string.IsNullOrEmpty(PortalSettings.ActiveTab.CultureCode)) &&
                            (PortalSettings.ActiveTab.CultureCode.ToLower().Trim() != "en-US".ToLower().Trim()))
                            PortalCultureCode = PortalSettings.ActiveTab.CultureCode;
                        else if ((PortalSettings.PortalAlias != null) &&
                            (!string.IsNullOrEmpty(PortalSettings.PortalAlias.CultureCode)) &&
                            (PortalSettings.PortalAlias.CultureCode.ToLower().Trim() != "en-US".ToLower().Trim()))
                            PortalCultureCode = PortalSettings.PortalAlias.CultureCode;
                        else if (PortalSettings.DefaultLanguage.ToLower().Trim() != "en-US".ToLower().Trim())
                            PortalCultureCode = PortalSettings.DefaultLanguage;
                    }

                    if ((HttpContext.Current != null) &&
                        (HttpContext.Current.Request != null) &&
                        (HttpContext.Current.Request.Cookies != null) &&
                        (HttpContext.Current.Request.Cookies["language"] != null))
                    {
                        CookiesCultureCode = System.Web.HttpContext.Current.Request.Cookies["language"].Value;
                    }
                }
                catch (Exception ex) { }

                if (!string.IsNullOrEmpty(PortalCultureCode))
                    return PortalCultureCode;
                else if (!string.IsNullOrEmpty(CookiesCultureCode))
                    return CookiesCultureCode;
                else
                    return cultureCode;
            }
        }

        public static string Language
        {
            get
            {
                string language = CultureCode;

                if ((language.ToLower() == "en-us".ToLower()) ||
                    (language.ToLower().Contains("en-us".ToLower())))
                    language = string.Empty;

                return language;
            }
        }

        public static string SharedResourceFile(string moduleDirectory)
        {
            return $"{moduleDirectory}App_LocalResources/SharedResources{Language.ToSuffix()}.resx";
        }

        public static string LocalResourceFile(string moduleDirectory, string ascxName)
        {
            if (!ascxName.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
            {
                ascxName += ".ascx";
            }

            return moduleDirectory +
                   "App_LocalResources/" +
                   ascxName +
                   Language.ToSuffix() +
                   ".resx";
        }

        public static String GetString(string key, string LocalResourceFile, ref bool finded)
        {
            try
            {
                finded = false;
                string result = DotNetNuke.Services.Localization.Localization.GetString(key, LocalResourceFile);
                if (string.IsNullOrEmpty(result.Trim()))
                {
                    return key;
                }
                else
                {
                    finded = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return key;
        }

        public static void SetText(Control obj, Hashtable Settings, string LocalResourceFile)
        {
            foreach (Control pctrl in obj.Controls)
            {
                try
                {
                    foreach (Control ctrl in obj.Controls.OfType<DotNetNuke.UI.UserControls.LabelControl>())
                    {
                        try
                        {
                            DotNetNuke.UI.UserControls.LabelControl objctrl = (DotNetNuke.UI.UserControls.LabelControl)ctrl;
                            string key = string.IsNullOrEmpty(objctrl.ResourceKey) ? objctrl.ID : objctrl.ResourceKey;
                            bool finded = false;
                            objctrl.Text = Common.GetString(key, LocalResourceFile, ref finded);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                    }
                    foreach (Control ctrl in pctrl.Controls.OfType<DotNetNuke.UI.UserControls.LabelControl>())
                    {
                        try
                        {
                            DotNetNuke.UI.UserControls.LabelControl objctrl = (DotNetNuke.UI.UserControls.LabelControl)ctrl;
                            string key = string.IsNullOrEmpty(objctrl.ResourceKey) ? objctrl.ID : objctrl.ResourceKey;
                            bool finded = false;
                            objctrl.Text = Common.GetString(key, LocalResourceFile, ref finded);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                    }
                    foreach (LinkButton ctrl in obj.Controls.OfType<LinkButton>())
                    {
                        try
                        {
                            string key = ctrl.ID;
                            bool finded = false;
                            ctrl.Text = Common.GetString(key, LocalResourceFile, ref finded);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                    }
                    foreach (LinkButton ctrl in pctrl.Controls.OfType<LinkButton>())
                    {
                        try
                        {
                            string key = ctrl.ID;
                            bool finded = false;
                            ctrl.Text = Common.GetString(key, LocalResourceFile, ref finded);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                    }

                    if (pctrl.Controls.Count > 0)
                        SetText(pctrl, Settings, LocalResourceFile);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
        }

        public static string Localization(Hashtable Settings, string Template, string resx)
        {
            if (string.IsNullOrEmpty(resx))
                resx = "SharedResources.resx";
            else if (!resx.ToLower().EndsWith(".ascx") && !resx.ToLower().EndsWith(".resx"))
                resx = resx + ".ascx";

            Template = Template.Replace("{{", @"⌡⌡");
            Template = Regex.Replace(Template, @"{(\w+)}", (m) =>
            {
                var key = m.Groups[1].Value;
                bool finded = false;
                string replacement = GetString(key, resx, ref finded);
                if ((key != replacement || finded) && !string.IsNullOrEmpty(replacement))
                    return replacement;
                else
                    return m.Value;
            }, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            Template = Template.Replace(@"⌡⌡", "{{");
            return Template;
        }

        public static List<DotNetNuke.Entities.Modules.ModuleInfo> ModulesFilder(int PortalId, string name)
        {
            ArrayList Modules = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(PortalId);

            if (Modules == null)
                return new List<DotNetNuke.Entities.Modules.ModuleInfo>();

            List<DotNetNuke.Entities.Modules.ModuleInfo> result = Modules.Cast<DotNetNuke.Entities.Modules.ModuleInfo>().Where(M => (M.DesktopModule.ModuleName == name) && (!M.IsDeleted)).ToList();
            if (result != null)
                return result;
            else
                return new List<DotNetNuke.Entities.Modules.ModuleInfo>();
        }

        public static DotNetNuke.Entities.Modules.ModuleInfo GetModuleById(int moduleId)
        {
            ArrayList Modules = DotNetNuke.Entities.Modules.ModuleController.Instance.GetAllTabsModulesByModuleID(moduleId);
            if ((Modules != null) && (Modules.Count > 0))
            {
                DotNetNuke.Entities.Modules.ModuleInfo moduleInfo = (DotNetNuke.Entities.Modules.ModuleInfo)Modules[0];
                int tabId = DotNetNuke.Entities.Modules.ModuleController.Instance.GetMasterTabId(moduleInfo);
                moduleInfo = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId, tabId, true);
                return moduleInfo;
            }
            else
                return new DotNetNuke.Entities.Modules.ModuleInfo();
        }

        private static int FindMessageTab(PortalSettings PortalSettings)
        {
            var profileTab = TabController.Instance.GetTab(PortalSettings.UserTabId, PortalSettings.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (TabInfo tab in childTabs)
                {
                    foreach (KeyValuePair<int, DotNetNuke.Entities.Modules.ModuleInfo> kvp in DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center" && !module.IsDeleted)
                        {
                            return tab.TabID;
                        }
                    }
                }
            }
            return PortalSettings.UserTabId;
        }
        public static int GetMessageTab(PortalSettings PortalSettings)
        {
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", PortalSettings.PortalId, Common.CultureCode);
            var messageTabId = DotNetNuke.Common.Utilities.DataCache.GetCache<int>(cacheKey);
            if (messageTabId > 0)
                return messageTabId;
            messageTabId = FindMessageTab(PortalSettings);
            DotNetNuke.Common.Utilities.DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));
            return messageTabId;
        }

        public static string Localization(
            string moduleDirectory
            , string Template
            , string resx)
        {
            Template = Template.Replace("{{", @"⌡⌡");
            Template = Regex.Replace(Template, @"{(\w+)}", (m) =>
            {
                var key = m.Groups[1].Value;
                string result = m.Groups[0].Value;
                string CultureCode = Language.ToLower().ToSuffix();
                bool finded = false;
                string replacement = DotNetNuke.Services.Localization.Localization.GetString(key, resx);


                if (!string.IsNullOrEmpty(replacement))
                {
                    result = replacement;
                    finded = true;
                }
                else if ((!string.IsNullOrEmpty(CultureCode)) && (resx.ToLower().Contains(CultureCode)))
                {
                    string CultureResx = resx.ToLower().Replace(CultureCode, string.Empty);
                    replacement = DotNetNuke.Services.Localization.Localization.GetString(key, CultureResx);
                    if (!string.IsNullOrEmpty(replacement))
                    {
                        result = replacement;
                        finded = true;
                    }
                }

                if (!finded)
                {
                    replacement = DotNetNuke.Services.Localization.Localization.GetString(key, SharedResourceFile(moduleDirectory));
                    if (!string.IsNullOrEmpty(replacement))
                        result = replacement;
                    else if ((!string.IsNullOrEmpty(CultureCode)) && (SharedResourceFile(moduleDirectory).ToLower().Contains(CultureCode)))
                    {
                        string CultureResx = SharedResourceFile(moduleDirectory).ToLower().Replace(CultureCode, string.Empty);
                        replacement = DotNetNuke.Services.Localization.Localization.GetString(key, CultureResx);
                        if (!string.IsNullOrEmpty(replacement))
                            result = replacement;
                    }
                }

                return result;
            }, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            Template = Template.Replace(@"⌡⌡", "{{");
            return Template;
        }

        public static Hashtable GetModuleSettings(int moduleId)
        {
            try
            {
                DotNetNuke.Entities.Modules.ModuleInfo moduleInfo = GetModuleById(moduleId);
                if (moduleInfo != null)
                {
                    return moduleInfo.ModuleSettings;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return null;
        }
        public static Hashtable GetModuleSettings(int moduleId, ref int PortalId)
        {
            try
            {
                DotNetNuke.Entities.Modules.ModuleInfo moduleInfo = GetModuleById(moduleId);
                if (moduleInfo != null)
                {
                    PortalId = moduleInfo.PortalID;
                    return moduleInfo.ModuleSettings;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return null;
        }

        public static string GenerateScriptTag(string url, bool async = true, bool defer = false)
        {
            if (url.EndsWith("tinymce.min.js"))
                async = defer = false;

            return string.Format("<script {1} {2} src='{0}'></script>", url, (async && !defer ? "async='true'" : string.Empty), (defer ? "defer='true'" : string.Empty));
        }



        public static string CurrencySymbol(CultureInfo CurrentCulture, Hashtable Settings)
        {
            string CurrencySymbol = CurrentCulture.NumberFormat.CurrencySymbol;
            if (CurrencySymbol == string.Empty)
            {
                CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
                CultureInfo currentCulture = cinfo.Where(c => c.Name == CultureInfo.CurrentCulture.Name).FirstOrDefault();
                if (currentCulture != null)
                {
                    CurrencySymbol = currentCulture.NumberFormat.CurrencySymbol;
                }
            }

            if ((Settings != null) &&
                (Settings["CurrencySymbol"] != null) &&
                (!string.IsNullOrEmpty(Settings["CurrencySymbol"].ToString())))
            {
                CurrencySymbol = Settings["CurrencySymbol"].ToString();
            }

            return CurrencySymbol;
        }
        public static string CurrencyName(System.Globalization.RegionInfo Region, Hashtable Settings)
        {
            string CurrencyName = Region.CurrencyNativeName;
            if (CurrencyName == string.Empty)
            {
                CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
                CultureInfo currentCulture = cinfo.Where(c => c.Name == CultureInfo.CurrentCulture.Name).FirstOrDefault();
                System.Globalization.RegionInfo region = new System.Globalization.RegionInfo(currentCulture.Name);

                if (region != null)
                {
                    CurrencyName = region.CurrencyNativeName;
                }
            }
            if ((Settings != null) && (Settings["CurrencyName"] != null) && (!string.IsNullOrEmpty(Settings["CurrencyName"].ToString())))
            {
                CurrencyName = Settings["CurrencyName"].ToString();
            }

            return CurrencyName;
        }
        public static string CurrencyFractionalUnit(Hashtable Settings)
        {
            string CurrencyFractionalUnit = string.Empty;
            if ((Settings != null) && (Settings["CurrencyFractionalUnit"] != null) && (!string.IsNullOrEmpty(Settings["CurrencyFractionalUnit"].ToString())))
            {
                CurrencyFractionalUnit = Settings["CurrencyFractionalUnit"].ToString();
            }

            return CurrencyFractionalUnit;
        }


        public static int MinPasswordLength { get { return DotNetNuke.Security.Membership.MembershipProviderConfig.MinPasswordLength; } }
        public static int MinNonAlphanumericCharacters { get { return DotNetNuke.Security.Membership.MembershipProviderConfig.MinNonAlphanumericCharacters; } }
        public static string CreatePassword(Hashtable Settings)
        {
            int length = Common.MinPasswordLength;
            int NonAlphanumeric = Common.MinNonAlphanumericCharacters;

            try
            {
                if (NonAlphanumeric < 1)
                {
                    string PasswordCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                    StringBuilder res = new StringBuilder();
                    Random rnd = new Random();
                    while (0 < length--)
                    {
                        res.Append(PasswordCharacters[rnd.Next(PasswordCharacters.Length)]);
                    }
                    return res.ToString();
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return System.Web.Security.Membership.GeneratePassword(length, NonAlphanumeric);
        }

        public static string TrateFriendlyURL(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return string.Empty;
            }
            string str = title;
            char[] arr = title.ToCharArray();
            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-')));
            str = new string(arr);
            str = str.Replace(' ', '-');

            if (str.Length > 150)
            {
                str = str.Substring(0, 150);
            }
            while ((str.Length > 1) && str.EndsWith("-"))
            {
                str = str.Substring(0, str.Length - 1);
            }

            str = str.Replace(" ", "");
            str = Regex.Replace(str, "-+", "-");

            return str;
        }

        public static System.Reflection.Assembly LoadAssembly(string AssemblyFileName)
        {
            string path = DotNetNuke.Common.Globals.ApplicationMapPath + "\\bin\\" + AssemblyFileName;
            System.Reflection.Assembly assembly = null;
            if (System.IO.File.Exists(path))
                assembly = AppDomain.CurrentDomain.Load(System.IO.File.ReadAllBytes(path));
            return assembly;
        }
        private static Type CreateType(string TypeName, string CacheKey, bool UseCache)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                CacheKey = TypeName;
            }

            Type type = null;

            if (UseCache)
            {
                type = (Type)DotNetNuke.Common.Utilities.DataCache.GetCache(CacheKey);
            }

            if (type == null)
            {
                try
                {
                    type = System.Web.Compilation.BuildManager.GetType(TypeName, true, true);
                    if (UseCache)
                    {
                        DotNetNuke.Common.Utilities.DataCache.SetCache(CacheKey, type);
                    }
                }
                catch (Exception exc)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(exc);
                }
            }

            return type;
        }
        public static object GetInstance(string ObjectAssemblyName, string ObjectNamespace)
        {
            object result = null;

            if (string.IsNullOrEmpty(ObjectAssemblyName) && string.IsNullOrEmpty(ObjectNamespace))
                return result;

            if (!string.IsNullOrEmpty(ObjectAssemblyName))
            {
                string path = DotNetNuke.Common.Globals.ApplicationMapPath + "\\bin\\" + ObjectAssemblyName;
                if (!System.IO.File.Exists(path))
                    return result;

                try
                {
                    if (ObjectAssemblyName.ToLower().EndsWith(".dll"))
                        ObjectAssemblyName = ObjectAssemblyName.Substring(0, (ObjectAssemblyName.Length - ".dll".Length));

                    string TypeName = ObjectNamespace + ", " + ObjectAssemblyName;
                    Type t = CreateType(TypeName, TypeName, true);
                    if (t != null)
                        result = Activator.CreateInstance(t);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            if (result != null)
                return result;
            else
                return GetInstance(ObjectNamespace);
        }
        public static object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }


        public static string TokenReplace(string Template, object info)
        {
            string result = Template;
            foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in System.ComponentModel.TypeDescriptor.GetProperties(info))
            {
                string newValue = (propertyDescriptor.GetValue(info) ?? string.Empty).ToString();

                result = result.Replace("[" + propertyDescriptor.Name + "]", newValue);
                result = result.Replace("{{" + propertyDescriptor.Name + "}}", newValue);
                result = result.Replace(propertyDescriptor.Name, newValue);
            }

            return result;
        }

    }
}
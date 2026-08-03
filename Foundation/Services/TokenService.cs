using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class TokenService : ITokenService
    {
        private readonly Core.Module.ModuleDefinition _definition;
        public TokenService(Core.Module.ModuleDefinition definition)
        {
            _definition = definition;
        }


        private static readonly Regex TodayRegex = new Regex(@"{{Today:(?<format>[^?]*?)(:(?<culture>[^?]*?))?}}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex QueryStringBlockRegex = new Regex(@"\{\{\#QueryString\s(?<param>[^?]*?)(\s(?<value>[^?]*?))?\}\}(?<content>((.|\n)*?))\{\{\/QueryString\}\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex IsUserRegex = new Regex(@"\{\{\#IsUser\}\}(?<content>((.|\n)*?))\{\{\/IsUser\}\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex IsEditorRegex = new Regex(@"\{\{\#IsEditor\}\}(?<content>((.|\n)*?))\{\{\/IsEditor\}\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex SettingsRegex = new Regex(@"\{\{Settings:(?<key>[^?]*?)\}\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex RequestRegex = new Regex(@"\{\{Request:(?<key>[^?]*?)\}\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex InRoleBlockRegex = new Regex(@"\{\{\#InRole\s(?<condition>[^?]*?)\}\}(?<content>((.|\n)*?))\{\{\/InRole\}\}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public string TokenToday(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            foreach (Match match in TodayRegex.Matches(html))
            {
                var format = match.Groups["format"].Value.Trim();
                var cultureGroup = match.Groups["culture"];
                CultureInfo culture = new CultureInfo(Common.Language);
                if (cultureGroup != null && !string.IsNullOrEmpty(cultureGroup.Value))
                    culture = new CultureInfo(cultureGroup.Value.Trim());

                var replacement = !string.IsNullOrEmpty(format)
                    ? DateTime.Now.ToString(format, culture)
                    : DateTime.Now.ToString("dddd d MMMM yyyy hh:mm:ss", culture);

                html = html.Replace(match.Value, replacement);
            }

            return html;
        }

        public string ReplaceAllTokens(
            string html
            , HttpRequest request
            , Hashtable settings)
        {
            if (string.IsNullOrEmpty(html)) return html;

            html = TokenToday(html);
            html = TokenIsUser(html);
            html = TokenIsEditor(html);
            html = TokenIsInRole(html);
            html = TokenSettings(html, settings);
            html = TokenRequest(html, request);
            html = TokenQueryString(html, request);

            return html;
        }

        private string TokenIsUser(string template)
        {
            foreach (Match match in IsUserRegex.Matches(template))
            {
                var content = match.Groups["content"].Value;
                var cond = Conditions.Instance(content);
                var incase = cond.InCase;
                var otherwise = cond.OtherWise;
                bool isUser = _definition.PortalSettings?.UserInfo.UserID > 0;
                template = template.Replace(match.Value, isUser ? (!string.IsNullOrEmpty(incase) ? incase : content) : (!string.IsNullOrEmpty(otherwise) ? otherwise : string.Empty));
            }
            return template;
        }

        private string TokenIsEditor(
            string template)
        {
            foreach (Match match in IsEditorRegex.Matches(template))
            {
                var content = match.Groups["content"].Value;
                var cond = Conditions.Instance(content);
                var incase = cond.InCase;
                var otherwise = cond.OtherWise;

                bool isEditor = (_definition.PortalSettings?.UserInfo?.UserID > 0) &&
                                ((_definition.PortalSettings.UserInfo.IsSuperUser) ||
                                 (_definition.PortalSettings.UserInfo.IsInRole(_definition.PortalSettings.AdministratorRoleName)) ||
                                 (DotNetNuke.Security.Permissions.TabPermissionController.CanAddContentToPage(_definition.PortalSettings.ActiveTab)));

                template = template.Replace(match.Value, isEditor ? (!string.IsNullOrEmpty(incase) ? incase : content) : (!string.IsNullOrEmpty(otherwise) ? otherwise : string.Empty));
            }
            return template;
        }

        private string TokenIsInRole(string template)
        {
            while (template.ToLower().Contains("{#inrole"))
            {
                var matches = InRoleBlockRegex.Matches(template);
                if (matches.Count == 0) break;

                foreach (Match match in matches)
                {
                    var condition = match.Groups["condition"].Value;
                    var content = match.Groups["content"].Value;
                    bool correct = EvaluateRoleCondition(condition);
                    var cond = Conditions.Instance(content);
                    var incase = cond.InCase;
                    var otherwise = cond.OtherWise;

                    template = template.Replace(match.Value, correct ? (!string.IsNullOrEmpty(incase) ? incase : content) : (!string.IsNullOrEmpty(otherwise) ? otherwise : string.Empty));
                }
            }
            return template;
        }

        private bool EvaluateRoleCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return false;
            if (_definition.PortalSettings?.UserInfo == null) return false;

            if (condition.Contains("&&"))
            {
                bool correct = true;
                var parts = condition.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var term = part.Trim();
                    if (term.StartsWith("!"))
                        correct = correct && (!IsInRole(term.Substring(1).Trim()));
                    else
                        correct = correct && IsInRole(term);
                }
                return correct;
            }
            else if (condition.Contains("||"))
            {
                bool correct = false;
                var parts = condition.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var term = part.Trim();
                    if (term.StartsWith("!"))
                        correct = correct || (!IsInRole(term.Substring(1).Trim()));
                    else
                        correct = correct || IsInRole(term);
                }
                return correct;
            }
            else
            {
                return IsInRole(condition.Trim());
            }
        }

        private bool IsInRole(string role)
        {
            if (string.IsNullOrEmpty(role) || _definition.PortalSettings ?.UserInfo?.UserID < 1) return false;
            if (_definition.PortalSettings.UserInfo.IsInRole(_definition.PortalSettings.AdministratorRoleName)) return true;
            return _definition.PortalSettings.UserInfo.IsInRole(role.Trim());
        }

        private string TokenSettings(string template, Hashtable settings)
        {
            foreach (Match match in SettingsRegex.Matches(template))
            {
                var key = match.Groups["key"].Value;
                var value = string.Empty;
                if (!string.IsNullOrEmpty(key) && settings != null && settings[key] != null)
                    value = settings[key].ToString();
                template = template.Replace(match.Value, value);
            }
            return template;
        }

        private string TokenRequest(string template, HttpRequest request)
        {
            foreach (Match match in RequestRegex.Matches(template))
            {
                var key = match.Groups["key"].Value;
                var value = string.Empty;
                if (!string.IsNullOrEmpty(key) && request.Params[key] != null)
                    value = request.Params[key].ToString();
                template = template.Replace(match.Value, value);
            }
            return template;
        }

        private string TokenQueryString(string template, HttpRequest request)
        {
            foreach (Match match in QueryStringBlockRegex.Matches(template))
            {
                var param = match.Groups["param"].Value;
                var value = match.Groups["value"]?.Value ?? string.Empty;
                var content = match.Groups["content"]?.Value ?? string.Empty;

                if (string.IsNullOrEmpty(content)) continue;
                var cond = Conditions.Instance(content);
                var incase = cond.InCase;
                var otherwise = cond.OtherWise;

                bool matched =
                    (!string.IsNullOrEmpty(param) && request.Params[param] != null && request.Params[param].Trim().Equals(value.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(param) && (request.Params[param] == null || string.IsNullOrEmpty(request.Params[param])) && string.IsNullOrEmpty(value));

                template = template.Replace(match.Value, matched ? (!string.IsNullOrEmpty(incase) ? incase : content) : (!string.IsNullOrEmpty(otherwise) ? otherwise : string.Empty));
            }
            return template;
        }
    }
}
using System.Text.RegularExpressions;

namespace DotNetNuke.Modules.Framework.Helpers
{
    public static class RegexHelper
    {
        public static Regex Compiled(string pattern, RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)
        {
            return new Regex(pattern, options);
        }
    }
}
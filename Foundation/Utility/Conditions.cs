using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace DotNetNuke.Modules.Foundation
{
    public class Conditions
    {
        private string incase = "";
        private string otherwise = "";

        private string Statment(string[] source, ref int index, string result)
        {
            if ((Regex.Matches(result, "{{#").Count < 1) ||
                (Regex.Matches(result, "{{#").Count == Regex.Matches(result, "{{/").Count))
            {
                return result;
            }
            else
            {
                index += 1;
                if (source.Length > index)
                    return Statment(source, ref index, result + " {{else}} " + source[index]);
                else
                    return result;
            }
        }


        public string InCase { get { return incase; } }
        public string OtherWise { get { return otherwise; } }

        public static Conditions Instance(string html) { return new Conditions(html); }

        public Conditions(string html)
        {
            if (html.StartsWith("{{else}}"))
                html = " " + html;

            string[] array = html.Replace("{{else}}", "ö").Split('ö');
            int index = 0;

            if (array != null && array.Length > 0)
                incase = Statment(array, ref index, array[0]);

            if (array != null && array.Length > index + 1)
            {
                for (int i = index + 1; i < array.Length; i++)
                {
                    otherwise += !string.IsNullOrEmpty(otherwise) ? " {{else}} " + array[i] : array[i];
                }
            }
        }
    }
}
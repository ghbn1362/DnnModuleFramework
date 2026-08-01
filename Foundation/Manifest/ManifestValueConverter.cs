using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Foundation.Manifest
{
    public static class ManifestValueConverter
    {
        public static T Convert<T>(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return default(T);

            Type type = typeof(T);

            if (type == typeof(string))
                return (T)(object)value;

            if (type == typeof(bool))
            {
                string v = value.Trim().ToLowerInvariant();

                bool result =
                    v == "1" ||
                    v == "true" ||
                    v == "yes" ||
                    v == "on";

                return (T)(object)result;
            }

            if (type.IsEnum)
                return (T)Enum.Parse(type, value, true);

            return (T)System.Convert.ChangeType(value, type);
        }
    }
}
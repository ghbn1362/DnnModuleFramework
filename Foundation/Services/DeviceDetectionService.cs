using DotNetNuke.Services.ClientCapability;
using System;
using System.Collections.Generic;
using System.Xml;

namespace DotNetNuke.Modules.Foundation.Services
{
    public class DeviceDetectionService : IDeviceDetectionService
    {
        private static readonly Dictionary<string, Func<IClientCapability, XmlAttribute, bool>> Matchers =
            new Dictionary<string, Func<IClientCapability, XmlAttribute, bool>>(StringComparer.OrdinalIgnoreCase)
            {
                ["IsMobile"] = (c, a) => c.IsMobile == ParseBool(a.Value),
                ["IsTablet"] = (c, a) => c.IsTablet == ParseBool(a.Value),
                ["IsTouchScreen"] = (c, a) => c.IsTouchScreen == ParseBool(a.Value),

                ["BrowserName"] = (c, a) => c.BrowserName.Equals(a.Value, StringComparison.OrdinalIgnoreCase),

                ["ScreenResolutionWidthInPixels"] =
                (c, a) => c.ScreenResolutionWidthInPixels == int.Parse(a.Value),

                ["ScreenResolutionHeightInPixels"] =
                (c, a) => c.ScreenResolutionHeightInPixels == int.Parse(a.Value),
            };

        public bool MatchesDevice(XmlElement element)
        {
            if (element?.Attributes == null)
                return true;

            var capability = ClientCapabilityProvider.CurrentClientCapability;

            foreach (XmlAttribute attribute in element.Attributes)
            {
                Func<IClientCapability, XmlAttribute, bool> matcher;
                if (Matchers.TryGetValue(attribute.Name, out matcher) &&
                    !matcher(capability, attribute))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ParseBool(string value)
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   value == "1";
        }
    }
}
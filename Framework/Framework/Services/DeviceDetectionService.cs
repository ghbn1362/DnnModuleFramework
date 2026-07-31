using System;
using System.Xml;

namespace DotNetNuke.Modules.Framework.Services
{
    public class DeviceDetectionService : IDeviceDetectionService
    {
        public bool MatchesDevice(XmlElement elt)
        {
            // Basic port of previous DeviceDetection logic, keep small and testable.
            var device = DotNetNuke.Services.ClientCapability.ClientCapabilityProvider.CurrentClientCapability;
            if (elt?.Attributes == null) return true;

            if (elt.Attributes["Mobile"] != null)
            {
                var val = elt.Attributes["Mobile"].Value.Trim();
                bool IsMobile = (val.ToLower() == "true" || val == "1");
                return (device.IsMobile && IsMobile) || (!device.IsMobile && !IsMobile);
            }

            // add other attributes as needed...
            return true;
        }
    }
}
using System.Xml;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface IDeviceDetectionService
    {
        bool MatchesDevice(XmlElement elt);
    }
}
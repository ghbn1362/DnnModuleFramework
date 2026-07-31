using System.Xml;

namespace DotNetNuke.Modules.Framework.Services
{
    public interface IDeviceDetectionService
    {
        bool MatchesDevice(XmlElement elt);
    }
}
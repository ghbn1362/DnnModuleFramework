using System.Web;
using System.Collections;

namespace DotNetNuke.Modules.Foundation.Services
{
    public interface ITokenService
    {
        string TokenToday(string html);
        string ReplaceAllTokens(string html, HttpRequest request, DotNetNuke.Entities.Users.UserInfo user, DotNetNuke.Entities.Portals.PortalSettings portalSettings, Hashtable settings);
    }
}
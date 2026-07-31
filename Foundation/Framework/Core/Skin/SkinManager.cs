using DotNetNuke.Common;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client.Providers;
using System;
using DotNetNuke.Modules.Foundation.Core.Skin;

namespace DotNetNuke.Modules.Foundation
{
    public abstract class SkinManager : Skin
    {
        protected abstract SkinDefinition Definition { get; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RegisterPersonaBar();

            ResourceManager.Register(Page, Definition);
        }

        protected virtual void RegisterPersonaBar()
        {
            if (!Definition.RegisterPersonaBarCss)
                return;

            if (PortalSettings.UserInfo != null &&
                PortalSettings.UserInfo.IsSuperUser)
                return;

            ClientResourceManager.RegisterStyleSheet(
                Page,
                ResolveUrl(SkinPath + "css/personaBar.css"),
                int.MaxValue,
                DnnFormBottomProvider.DefaultName);
        }

        protected virtual void CheckSecurity()
        {
            try
            {
                if (PortalSettings.UserInfo != null &&
                    PortalSettings.UserInfo.UserID > 0)
                    return;

                Response.Redirect(
                    Globals.LoginURL(
                        Server.UrlEncode(Request.Url.PathAndQuery),
                        false));
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);

                Response.Redirect(
                    Globals.LoginURL(
                        Server.UrlEncode(Request.Url.PathAndQuery),
                        false));
            }
        }
    }
}
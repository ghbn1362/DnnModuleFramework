using System;
using DotNetNuke.Common;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Client.Providers;

namespace DotNetNuke.Modules.Foundation
{
    /// <summary>
    /// Base class for Foundation based DNN skins.
    /// </summary>
    public abstract class SkinManager : Skin
    {
        /// <summary>
        /// Gets current skin definition.
        /// </summary>
        protected abstract Core.Skin.SkinDefinition Definition { get; }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            Initialize();
        }

        /// <summary>
        /// Initializes skin pipeline.
        /// </summary>
        protected virtual void Initialize()
        {
            OnBeforeInitialize();


            RegisterResources();


            RegisterPersonaBar();


            OnAfterInitialize();
        }

        /// <summary>
        /// Called before initialization starts.
        /// </summary>
        protected virtual void OnBeforeInitialize()
        {

        }

        /// <summary>
        /// Called after initialization completed.
        /// </summary>
        protected virtual void OnAfterInitialize()
        {

        }

        /// <summary>
        /// Registers CSS and JavaScript resources.
        /// </summary>
        protected virtual void RegisterResources()
        {
            Core.Skin.ResourceManager.Register(
                Page,
                Definition);
        }

        /// <summary>
        /// Registers PersonaBar compatibility resources.
        /// </summary>
        protected virtual void RegisterPersonaBar()
        {
            if (!Definition.RegisterPersonaBarCss)
                return;


            if (PortalSettings.UserInfo != null &&
                PortalSettings.UserInfo.IsSuperUser)
                return;



            ClientResourceManager.RegisterStyleSheet(
                Page,
                ResolveUrl(
                    SkinPath +
                    "css/personaBar.css"),
                int.MaxValue,
                DnnFormBottomProvider.DefaultName);
        }

        /// <summary>
        /// Redirects anonymous users to login page.
        /// </summary>
        protected virtual void CheckSecurity()
        {
            try
            {
                if (PortalSettings.UserInfo != null &&
                    PortalSettings.UserInfo.UserID > 0)
                {
                    return;
                }


                RedirectToLogin();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);

                RedirectToLogin();
            }
        }

        private void RedirectToLogin()
        {
            Response.Redirect(
                Globals.LoginURL(
                    Server.UrlEncode(
                        Request.Url.PathAndQuery),
                    false));
        }
    }
}
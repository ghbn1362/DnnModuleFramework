using System.Web.WebPages;

namespace DotNetNuke.Modules.Framework
{
    public abstract class DotNetNukeWebPage : WebPageBase
    {
        private dynamic _model;

        protected override void ConfigurePage(WebPageBase parentPage)
        {
            base.ConfigurePage(parentPage);

            //Child pages need to get their context from the Parent
            Context = parentPage.Context;
        }

        public dynamic Model
        {
            get { return _model ?? (_model = PageContext.Model); }
            set { _model = value; }
        }
    }

    public abstract class DotNetNukeWebPage<TModel> :DotNetNukeWebPage where TModel : class
    {
        private TModel _model;

        public new TModel Model
        {
            get { return _model ?? (_model = PageContext.Model as TModel); }
            set { _model = value; }
        }
    }
}
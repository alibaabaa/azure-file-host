using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using AzureStorageFileHost.App_Start;

namespace AzureStorageFileHost
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            WebApiRoutes.Register(GlobalConfiguration.Configuration);
        }
    }
}

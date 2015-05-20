using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace AzureStorageFileHost.App_Start
{
    public class WebApiRoutes
    {
        public static void Register(HttpConfiguration configuration)
        {
            configuration.Routes.MapHttpRoute(
                name: "ApiRoute",
                routeTemplate: "{action}",
                defaults: new { controller = "Home" });
        }
    }
}
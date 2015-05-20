using System.Web.Mvc;

namespace AzureStorageFileHost.Controllers
{
    public class HomeController : Controller
    {
        public string Index()
        {
            ControllerContext.HttpContext.Response.StatusCode = 202;
            return "ACCEPTED";
        }
    }
}
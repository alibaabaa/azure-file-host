using System.IO;
using System.Net;
using System.Web;
using System.Web.Http;

namespace AzureStorageFileHost.Controllers
{
    public class HomeController : ApiController
    {
        [HttpGet, Route("")]
        public string Upload()
        {
            HttpContext.Current.Response.StatusCode = 202;
            return "ACCEPTED";
        }
    }
}
using System;
using System.Web;
using System.Web.Mvc;

namespace AzureStorageFileHost.Controllers
{
    public class UploadController : Controller
    {
        [HttpPost]
        public string Receive(HttpPostedFileBase file, Guid apiKey, string config)
        {
            ControllerContext.HttpContext.Response.StatusCode = 202;
            return "ACCEPTED";
        }
    }
}
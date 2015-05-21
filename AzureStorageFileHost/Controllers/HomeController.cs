using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AzureStorageFileHost.ConfigurationStorage;

namespace AzureStorageFileHost.Controllers
{
    public class UploadController : Controller
    {
        private readonly IUploadConfigurationStore store = new UploadConfigurationStore();

        [HttpPost]
        public async Task<string> Receive(HttpPostedFileBase file, Guid apiKey, string config)
        {
            var json = await store.GetConfiguration(apiKey).ConfigureAwait(false);
            ControllerContext.HttpContext.Response.StatusCode = 202;
            return "ACCEPTED";
        }
    }
}
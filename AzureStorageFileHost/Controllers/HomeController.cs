using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AzureStorageFileHost.ConfigurationStorage;
using Newtonsoft.Json.Linq;

namespace AzureStorageFileHost.Controllers
{
    public class UploadController : Controller
    {
        private readonly IUploadConfigurationStore store = new UploadConfigurationStore();

        [HttpPost]
        public async Task<JsonResult> Receive(HttpPostedFileBase file, Guid apiKey, string config)
        {
            if (file == null)
            {
                ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new {message = "no file provided"});
            }
            JObject json;
            try
            {
                json = await store.GetConfiguration(apiKey).ConfigureAwait(false);
            }
            catch (Exception)
            {
                ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { message = "invalid api key" });
            }
            var selectedConfigSet = json["contentActionSets"].FirstOrDefault(set => (string)set["name"] == config);
            if (selectedConfigSet == null)
            {
                ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(new { messsage = "content set not found" });
            }
            var probablyAnImage = ProbablyResizableImage(file.InputStream, file.ContentType);

            ControllerContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
            return Json(new { status = "ACCEPTED" });
        }

        private static bool ProbablyResizableImage(Stream stream, string contentType)
        {
            stream.Position = 0;
            using (var reader = new BinaryReader(stream))
            {
                switch (contentType)
                {
                    case "image/png":
                        return
                            reader.ReadUInt64() == 0xa1a0a0d474e5089;
                    case "image/jpeg":
                        return reader.ReadUInt32() == 0xdbffd8ff;
                    default:
                        return false;
                }
            }
        }
    }
}
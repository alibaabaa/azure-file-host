using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;

namespace AzureStorageFileHost
{
    public class ActionSetStreamProcessor
    {
        private readonly Stream inputStream = new MemoryStream();
        private readonly string filename;
        private readonly string contentType;
        private readonly JObject processConfig;

        public ActionSetStreamProcessor(
            Stream inputStream,
            string filename,
            string contentType,
            JObject processConfig)
        {
            inputStream.Position = 0;
            inputStream.CopyTo(this.inputStream);
            this.inputStream.Position = 0;
            this.filename = filename;
            this.contentType = contentType;
            this.processConfig = processConfig;
        }

        public async Task<IEnumerable<string>> ProcessStreamForPublicBlobUrl(string actionSetName)
        {
            var blobContainer = await BlobAccess.GetBlobContainer(
                (string)processConfig["storage"]["accountName"],
                (string)processConfig["storage"]["accessKey"],
                (string)processConfig["storage"]["container"]).ConfigureAwait(false);

            var actionSets = processConfig["contentActionSets"].First(set => (string)set["name"] == actionSetName);
            if (actionSets["imageActions"] != null &&
                StreamAnalyser.ProbablyResizableImage(inputStream, contentType))
            {
                var imageConfig = (JArray)actionSets["imageActions"];
                foreach (var config in imageConfig)
                {
                    await StreamImageConfigResultToContainer(config, blobContainer).ConfigureAwait(false);
                }
            }
            else
            {
                var fileConfig = (JArray)actionSets["fileActions"];
                foreach (var config in fileConfig)
                {
                    await StreamFileConfigResultToContainer(config, blobContainer).ConfigureAwait(false);
                }
            }
            return await Task.FromResult(Enumerable.Empty<string>());
        }

        private async Task StreamImageConfigResultToContainer(JToken config, CloudBlobContainer blobContainer)
        {

        }

        private async Task StreamFileConfigResultToContainer(JToken config, CloudBlobContainer blobContainer)
        {
            var newFilename = config["rename"] == null
                ? filename
                : ApplyRenamePattern(filename, (string)config["rename"]);
            if (config["replaceExisting"] != null && !(bool)config["replaceExisting"])
            {
                newFilename = await NextAvailableFilename(blobContainer, newFilename).ConfigureAwait(false);
            }
            await BlobAccess.StreamToContainer(blobContainer, inputStream, newFilename, contentType).ConfigureAwait(false);
        }

        private static async Task<string> NextAvailableFilename(CloudBlobContainer blobContainer, string filename)
        {
            var fileIncrement = 1;
            var finalFilename = filename;
            while (await BlobAccess.BlobExistsInContainer(blobContainer, finalFilename).ConfigureAwait(false))
            {
                finalFilename =
                    filename.Substring(0, filename.LastIndexOf('.')) +
                    "_" + fileIncrement++ +
                    filename.Substring(filename.LastIndexOf('.'));
            }
            return finalFilename;
        }

        private static string ApplyRenamePattern(string filename, string renamePattern)
        {
            renamePattern = renamePattern.Replace("{now:MM}", DateTime.Now.ToString("MM"));
            renamePattern = renamePattern.Replace("{now:dd}", DateTime.Now.ToString("dd"));
            renamePattern = renamePattern.Replace("{file:name}", filename.Substring(0, filename.LastIndexOf('.')));
            renamePattern = renamePattern.Replace("{file:ext}", filename.Substring(filename.LastIndexOf('.') + 1));
            return renamePattern;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Imaging;
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

            var writtenBlobs = new List<string>();
            var actionSets = processConfig["contentActionSets"].First(set => (string)set["name"] == actionSetName);
            if (actionSets["imageActions"] != null &&
                StreamAnalyser.ProbablyResizableImage(inputStream, contentType))
            {
                var imageConfig = (JArray)actionSets["imageActions"];
                foreach (var config in imageConfig)
                {
                    if (config["resize"] != null)
                    {
                        writtenBlobs.Add(await StreamImageConfigResultToContainer(config, blobContainer).ConfigureAwait(false));
                    }
                    else
                    {
                        writtenBlobs.Add(await StreamFileConfigResultToContainer(config, blobContainer).ConfigureAwait(false));
                    }
                }
            }
            else
            {
                var fileConfig = (JArray)actionSets["fileActions"];
                foreach (var config in fileConfig)
                {
                    writtenBlobs.Add(await StreamFileConfigResultToContainer(config, blobContainer).ConfigureAwait(false));
                }
            }
            return writtenBlobs.Where(name => name != null).Select(name => processConfig["publicUrl"] + name);
        }

        private async Task<string> StreamImageConfigResultToContainer(JToken config, CloudBlobContainer blobContainer)
        {
            var upscaleAllowed = config["resize"]["upscale"] != null && (bool)config["resize"]["upscale"];
            if (!upscaleAllowed && ResizeWouldUpscaleImage(config))
            {
                return null;
            }
            var newSize = (string)config["resize"]["constraint"] == "width"
                ? new Size((int)config["resize"]["size"], Int32.MaxValue)
                : new Size(Int32.MaxValue, (int)config["resize"]["size"]);
            using (var blobStream = new MemoryStream())
            {
                using (var imageFactory = new ImageFactory())
                {
                    imageFactory
                        .Load(inputStream)
                        .Resize(new ResizeLayer(newSize, ResizeMode.Max) { Upscale = upscaleAllowed })
                        .Save(blobStream);
                }
                var dimensions = StreamAnalyser.DimensionsFromImageStream(blobStream);
                var newFilename = config["rename"] == null
                    ? filename
                    : ApplyRenamePattern(filename, (string)config["rename"], dimensions.Item1, dimensions.Item2);
                if (config["replaceExisting"] != null && !(bool)config["replaceExisting"])
                {
                    newFilename = await NextAvailableFilename(blobContainer, newFilename).ConfigureAwait(false);
                }
                await BlobAccess.StreamToContainer(blobContainer, blobStream, newFilename, contentType).ConfigureAwait(false);
                return newFilename;
            }
        }

        private bool ResizeWouldUpscaleImage(JToken config)
        {
            var originalDimensions = StreamAnalyser.DimensionsFromImageStream(inputStream);
            return
                ((string)config["resize"]["constraint"] == "width" &&
                 originalDimensions.Item1 < (int)config["resize"]["size"])
                ||
                ((string)config["resize"]["constraint"] == "height" &&
                 originalDimensions.Item2 < (int)config["resize"]["size"]);
        }

        private async Task<string> StreamFileConfigResultToContainer(JToken config, CloudBlobContainer blobContainer)
        {
            var newFilename = config["rename"] == null
                ? filename
                : ApplyRenamePattern(filename, (string)config["rename"]);
            if (config["replaceExisting"] != null && !(bool)config["replaceExisting"])
            {
                newFilename = await NextAvailableFilename(blobContainer, newFilename).ConfigureAwait(false);
            }
            await BlobAccess.StreamToContainer(blobContainer, inputStream, newFilename, contentType).ConfigureAwait(false);
            return newFilename;
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

        private static string ApplyRenamePattern(string filename, string renamePattern, int? width = null, int? height = null)
        {
            renamePattern = renamePattern.Replace("{now:MM}", DateTime.Now.ToString("MM"));
            renamePattern = renamePattern.Replace("{now:dd}", DateTime.Now.ToString("dd"));
            renamePattern = renamePattern.Replace("{file:name}", filename.Substring(0, filename.LastIndexOf('.')));
            renamePattern = renamePattern.Replace("{file:ext}", filename.Substring(filename.LastIndexOf('.') + 1));
            if (width.HasValue)
            {
                renamePattern = renamePattern.Replace("{file:w}", width.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (height.HasValue)
            {
                renamePattern = renamePattern.Replace("{file:h}", height.Value.ToString(CultureInfo.InvariantCulture));
            }
            return renamePattern;
        }
    }
}
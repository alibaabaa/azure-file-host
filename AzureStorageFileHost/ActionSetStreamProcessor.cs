using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureStorageFileHost
{
    public class ActionSetStreamProcessor
    {
        private readonly Stream inputStream = new MemoryStream();
        private readonly string contentType;
        private readonly JObject processConfig;

        public ActionSetStreamProcessor(Stream inputStream, string contentType, JObject processConfig)
        {
            inputStream.Position = 0;
            inputStream.CopyTo(this.inputStream);
            this.inputStream.Position = 0;
            this.contentType = contentType;
            this.processConfig = processConfig;
        }

        public async Task<IEnumerable<string>> ProcessStreamForPublicBlobUrl(string actionSetName)
        {
            if (processConfig["contentActionSets"].First(set => (string)set["name"] == actionSetName)["imageActions"] != null &&
                StreamAnalyser.ProbablyResizableImage(inputStream, contentType))
            {
                //do some image processing
                int i = 1;
            }
            else
            {
                int j = 2;
                //process as file
            }
            return await Task.FromResult(Enumerable.Empty<string>());
        }
    }
}
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureStorageFileHost.ConfigurationStorage
{
    public interface IUploadConfigurationStore
    {
        Task<JObject> GetConfiguration(Guid apiKey);
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;

namespace AzureStorageFileHost.ConfigurationStorage
{
    public class UploadConfigurationStore
    {
        private const string TableName = "uploadconfig";
        public async Task<JObject> GetConfiguration(Guid apiKey)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConfigurationStore"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);

            var retrieveConfigResult = await table.ExecuteAsync(
                TableOperation.Retrieve<ConfigurationEntity>("JsonConfig", apiKey.ToString("N"))).ConfigureAwait(false);
            if ((retrieveConfigResult.Result as ConfigurationEntity) == null)
            {
                throw new ArgumentOutOfRangeException("apiKey", "Provided API value does not match an upload configuration");
            }
            return JObject.Parse(((ConfigurationEntity)retrieveConfigResult.Result).Config);
        }

        public async Task<bool> AtLeastOneConfigExists()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("ConfigurationStore"));
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(TableName);
                await table.CreateIfNotExistsAsync().ConfigureAwait(false);
                var configQuery = new TableQuery<ConfigurationEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "JsonConfig"));
                var resultSet = table.ExecuteQuery(configQuery);
                return resultSet.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
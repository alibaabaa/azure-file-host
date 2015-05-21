using Microsoft.WindowsAzure.Storage.Table;

namespace AzureStorageFileHost.ConfigurationStorage
{
    public class ConfigurationEntity : TableEntity
    {
        public string Config { get; set; }
    }
}
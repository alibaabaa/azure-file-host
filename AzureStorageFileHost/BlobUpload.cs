using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageFileHost
{
    public static class BlobUpload
    {
        public async static Task<CloudBlobContainer> GetBlobContainer(
            string storageAccountName,
            string storageAccountAccessKey,
            string containerName)
        {
            var connectionString = string.Format(
                "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};",
                storageAccountName,
                storageAccountAccessKey);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var containerClient = storageAccount.CreateCloudBlobClient();
            var container = containerClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            return container;
        }

        public static async Task StreamToContainer(CloudBlobContainer container, Stream inputStream, string name)
        {
            var blob = container.GetBlockBlobReference(name);
            inputStream.Position = 0;
            await blob.UploadFromStreamAsync(inputStream).ConfigureAwait(false);
        }
    }
}
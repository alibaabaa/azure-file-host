using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageFileHost
{
    public static class BlobAccess
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
            if (await container.CreateIfNotExistsAsync().ConfigureAwait(false))
            {
                await container.SetPermissionsAsync(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }).ConfigureAwait(false);
            }
            return container;
        }

        public static async Task StreamToContainer(CloudBlobContainer container, Stream inputStream, string name, string contentType)
        {
            var blob = container.GetBlockBlobReference(name);
            blob.Properties.ContentType = contentType;
            inputStream.Position = 0;
            await blob.UploadFromStreamAsync(inputStream).ConfigureAwait(false);
        }

        public static async Task<bool> BlobExistsInContainer(CloudBlobContainer container, string name)
        {
            return await container.GetBlockBlobReference(name).ExistsAsync().ConfigureAwait(false);
        }
    }
}
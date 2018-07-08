using AIT.PullRequestStatus.DataAccess.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.DataAccess.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly CloudBlobContainer _container;

        public ConfigurationRepository(CloudStorageAccount cloudStorageAccount)
        {
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference("configuration");
            _container.CreateIfNotExistsAsync();
        }

        public async Task<AccountConfiguration> GetAsync(string collectionId)
        {
            var blockBlob = _container.GetBlockBlobReference(collectionId);
            if (!(await blockBlob.ExistsAsync()))
            {
                return new AccountConfiguration();
            }

            try
            {
                return JsonConvert.DeserializeObject<AccountConfiguration>(await blockBlob.DownloadTextAsync());
            }
            catch
            {
                return new AccountConfiguration();
            }
        }

        public async Task UpdateAsync(AccountConfiguration configuration)
        {
            var blockBlob = _container.GetBlockBlobReference(configuration.CollectionId);
            await blockBlob.UploadTextAsync(JsonConvert.SerializeObject(configuration));
        }
    }
}
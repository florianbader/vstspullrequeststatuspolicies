using AIT.PullRequestStatus.DataAccess.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.DataAccess.Repositories
{
    public class ConfigurationRepositoryLocal : IConfigurationRepository
    {
        private const string DatabaseFileName = "configuration.json";

        private readonly Dictionary<string, AccountConfiguration> _configuration;

        public ConfigurationRepositoryLocal()
        {
            var database = File.Exists(DatabaseFileName) ? File.ReadAllText(DatabaseFileName) : "{}";
            _configuration = JsonConvert.DeserializeObject<Dictionary<string, AccountConfiguration>>(database);
        }

        public Task<AccountConfiguration> GetAsync(string collectionId) =>
            Task.FromResult(_configuration.ContainsKey(collectionId)
                ? _configuration[collectionId]
                : new AccountConfiguration());

        public Task UpdateAsync(AccountConfiguration configuration)
        {
            _configuration[configuration.CollectionId] = configuration;

            File.WriteAllText(DatabaseFileName, JsonConvert.SerializeObject(_configuration));

            return Task.FromResult(0);
        }
    }
}
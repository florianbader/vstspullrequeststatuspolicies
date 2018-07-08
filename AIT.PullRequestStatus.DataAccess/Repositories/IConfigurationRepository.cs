using AIT.PullRequestStatus.DataAccess.Entities;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.DataAccess.Repositories
{
    public interface IConfigurationRepository
    {
        Task<AccountConfiguration> GetAsync(string collectionId);

        Task UpdateAsync(AccountConfiguration configuration);
    }
}
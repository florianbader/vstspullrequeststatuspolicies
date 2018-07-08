using AIT.PullRequestStatus.DataAccess.Entities;
using AIT.PullRequestStatus.Domain.Entities;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.Services
{
    public interface IPullRequestService
    {
        Task<bool> ActivateCollectionAsync(AccountConfiguration configuration);

        Task ActivateStatusPolicyAsync(string collectionId, string projectId, string repositoryId, string statusPolicyName);

        Task DeactivateStatusPolicyAsync(string collectionId, string projectId, string repositoryId, string statusPolicyName);

        Task EvaluateAsync(PullRequestUpdate pullRequestUpdate);

        Task<CollectionStatus> GetCollectionStatusAsync(string collectionId);

        Task<ProjectStatus> GetProjectStatusAsync(string collectionId, string projectId);

        Task<RepositoryStatus> GetRepositoryStatusAsync(string collectionId, string projectId, string repositoryId);

        Task ReactivateProjectAsync(string collectionId, string projectId);
    }
}
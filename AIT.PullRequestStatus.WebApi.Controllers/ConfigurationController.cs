using AIT.PullRequestStatus.DataAccess.Entities;
using AIT.PullRequestStatus.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.WebApi.Controllers
{
    [Route("api/v1/configuration")]
    public class ConfigurationController : Controller
    {
        private readonly IPullRequestService _pullRequestService;

        public ConfigurationController(IPullRequestService pullRequestService)
            => _pullRequestService = pullRequestService;

        [Route("{collectionId}")]
        [HttpPost]
        public Task ActivateCollectionAsync(string collectionId)
            => ActivateCollectionAsync(collectionId, Request);

        [NonAction]
        public async Task ActivateCollectionAsync(string collectionId, HttpRequest request)
        {
            using (var reader = new StreamReader(request.Body))
            {
                var json = await reader.ReadToEndAsync();
                var configuration = JsonConvert.DeserializeObject<AccountConfiguration>(json);
                configuration.CollectionId = collectionId;

                await _pullRequestService.ActivateCollectionAsync(configuration);
            }
        }

        [Route("{collectionId}/{projectId}/{repositoryId}/{statusPolicyName}")]
        [HttpPost]
        public Task ActivateStatusPolicyAsync(string collectionId, string projectId, string repositoryId, string statusPolicyName)
            => _pullRequestService.ActivateStatusPolicyAsync(collectionId, projectId, repositoryId, statusPolicyName);

        [Route("{collectionId}/{projectId}/{repositoryId}/{statusPolicyName}")]
        [HttpDelete]
        public Task DeactivateAsync(string collectionId, string projectId, string repositoryId, string statusPolicyName)
            => _pullRequestService.DeactivateStatusPolicyAsync(collectionId, projectId, repositoryId, statusPolicyName);

        [Route("{collectionId}")]
        public async Task<IActionResult> GetCollectionStatusAsync(string collectionId)
            => Ok(await _pullRequestService.GetCollectionStatusAsync(collectionId));

        [Route("{collectionId}/{projectId}")]
        public async Task<IActionResult> GetProjectStatusAsync(string collectionId, string projectId)
            => Ok(await _pullRequestService.GetProjectStatusAsync(collectionId, projectId));

        [Route("{collectionId}/{projectId}/{repositoryId}")]
        public async Task<IActionResult> GetRepositoryStatusAsync(string collectionId, string projectId, string repositoryId)
            => Ok(await _pullRequestService.GetRepositoryStatusAsync(collectionId, projectId, repositoryId));

        [Route("{collectionId}/{projectId}")]
        [HttpPost]
        public Task ReactivateProjectAsync(string collectionId, string projectId)
            => _pullRequestService.ReactivateProjectAsync(collectionId, projectId);
    }
}
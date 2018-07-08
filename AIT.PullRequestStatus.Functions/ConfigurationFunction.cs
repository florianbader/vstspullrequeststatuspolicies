using AIT.PullRequestStatus.DataAccess.Repositories;
using AIT.PullRequestStatus.Domain.Services;
using AIT.PullRequestStatus.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Functions
{
    public static class ConfigurationFunction
    {
        private static readonly IPullRequestService _pullRequestService = new PullRequestService(
            new Uri($"https://{GetEnvironmentVariable("WEBSITE_HOSTNAME")}/api/v1/"),
            new VssConnectionFactory(),
            new StatusPoliciesService(),
            new ConfigurationRepository(
                new CloudStorageAccount(
                    new StorageCredentials(
                        GetEnvironmentVariable("TableStorageAccount"),
                        GetEnvironmentVariable("TableStorageKey")), true)));

        [FunctionName("ConfigurationActivateCollection")]
        public static Task ActivateCollectionAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "configuration/{collectionId}")] HttpRequest req, string collectionId)
            => new ConfigurationController(_pullRequestService).ActivateCollectionAsync(collectionId, req);

        [FunctionName("ConfigurationActivateProjectStatusPolicy")]
        public static Task ActiveStatusPolicyAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "configuration/{collectionId}/{projectId}/{repositoryId}/{statusPolicyName}")] HttpRequest req, string collectionId,
                string projectId, string repositoryId, string statusPolicyName)
            => new ConfigurationController(_pullRequestService).ActivateStatusPolicyAsync(collectionId, projectId, repositoryId, statusPolicyName);

        [FunctionName("ConfigurationDeactivateProjectStatusPolicy")]
        public static Task DeactiveStatusPolicyAsync([HttpTrigger(AuthorizationLevel.Anonymous, "delete",
                Route = "configuration/{collectionId}/{projectId}/{repositoryId}/{statusPolicyName}")] HttpRequest req, string collectionId,
                string projectId, string repositoryId, string statusPolicyName)
            => new ConfigurationController(_pullRequestService).DeactivateAsync(collectionId, projectId, repositoryId, statusPolicyName);

        [FunctionName("ConfigurationGetCollectionStatus")]
        public static Task<IActionResult> GetCollectionStatusAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "configuration/{collectionId}")] HttpRequest req, string collectionId)
            => new ConfigurationController(_pullRequestService).GetCollectionStatusAsync(collectionId);

        [FunctionName("ConfigurationGetProjectStatus")]
        public static Task<IActionResult> GetProjectStatusAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "configuration/{collectionId}/{projectId}")] HttpRequest req, string collectionId, string projectId)
            => new ConfigurationController(_pullRequestService).GetProjectStatusAsync(collectionId, projectId);

        [FunctionName("ConfigurationGetRepositoryStatus")]
        public static Task<IActionResult> GetRepositoryStatusAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "configuration/{collectionId}/{projectId}/{repositoryId}")] HttpRequest req, string collectionId, string projectId, string repositoryId)
            => new ConfigurationController(_pullRequestService).GetRepositoryStatusAsync(collectionId, projectId, repositoryId);

        [FunctionName("ConfigurationReactivateProject")]
        public static Task ReactivateProjectAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "configuration/{collectionId}/{projectId}")] HttpRequest req, string collectionId, string projectId)
            => new ConfigurationController(_pullRequestService).ReactivateProjectAsync(collectionId, projectId);

        private static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }
}
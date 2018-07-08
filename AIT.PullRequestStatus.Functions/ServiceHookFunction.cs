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
    public static class ServiceHookFunction
    {
        private static IPullRequestService _pullRequestService = new PullRequestService(
            new Uri($"https://{GetEnvironmentVariable("WEBSITE_HOSTNAME")}/api/v1/"),
            new VssConnectionFactory(),
            new StatusPoliciesService(),
            new ConfigurationRepository(
                new CloudStorageAccount(
                    new StorageCredentials(
                        GetEnvironmentVariable("TableStorageAccount"),
                        GetEnvironmentVariable("TableStorageKey")), true)));

        [FunctionName("PullRequestServiceHook")]
        public static Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "servicehook")] HttpRequest req)
            => new ServiceHookController(_pullRequestService).PostAsync(req);

        private static string GetEnvironmentVariable(string name) =>
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }
}
using AIT.PullRequestStatus.Domain.Entities;
using AIT.PullRequestStatus.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.WebApi.Controllers
{
    [Route("api/v1/servicehook")]
    public class ServiceHookController : Controller
    {
        private readonly IPullRequestService _pullRequestService;

        public ServiceHookController(IPullRequestService pullRequestService)
            => _pullRequestService = pullRequestService;

        public Task<IActionResult> PostAsync() => PostAsync(Request);

        [NonAction]
        public async Task<IActionResult> PostAsync(HttpRequest request)
        {
            try
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var json = await reader.ReadToEndAsync();
                    var data = JObject.Parse(json);

                    if (!data["eventType"].Value<string>().StartsWith("git.pullrequest"))
                    {
                        return BadRequest("Bad event type");
                    }

                    var pullRequestUpdate = new PullRequestUpdate();
                    pullRequestUpdate.Id = data["resource"]["pullRequestId"].Value<int>();
                    pullRequestUpdate.CollectionId = data["resourceContainers"]["collection"]["id"].Value<string>();
                    pullRequestUpdate.ProjectId = data["resourceContainers"]["project"]["id"].Value<string>();
                    pullRequestUpdate.RepositoryId = data["resource"]["repository"]["id"].Value<string>();
                    pullRequestUpdate.Status = data["resource"]["status"].Value<string>();
                    pullRequestUpdate.Token = request.Query["token"];

                    await _pullRequestService.EvaluateAsync(pullRequestUpdate);
                }
            }
            catch
            {
                // always blame it on the client
                return BadRequest();
            }

            return Ok();
        }
    }
}
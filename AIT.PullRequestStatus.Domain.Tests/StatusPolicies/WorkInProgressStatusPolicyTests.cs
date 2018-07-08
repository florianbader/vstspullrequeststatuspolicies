using AIT.PullRequestStatus.Domain.StatusPolicies;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Threading.Tasks;
using Xunit;

namespace AIT.PullRequestStatus.Domain.Tests.StatusPolicies
{
    public class WorkInProgressStatusPolicyTests : StatusPolicyTestsBase<WorkInProgressStatusPolicy>
    {
        [Fact]
        public async Task EvaluateAsync_WorkInProgressInTitle_ShouldSetStatusToFailed()
        {
            var pullRequest = new GitPullRequest();
            pullRequest.Title = "[WIP] New super awesome feature pull request #123";

            await EvaluateAsync(pullRequest, GitStatusState.Failed);
        }

        [Fact]
        public async Task EvaluateAsync_WorkInProgressNotInTitle_ShouldSetStatusToSuceeded()
        {
            var pullRequest = new GitPullRequest();
            pullRequest.Title = "Wipe all data";

            await EvaluateAsync(pullRequest, GitStatusState.Succeeded);
        }
    }
}
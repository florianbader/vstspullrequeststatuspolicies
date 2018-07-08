using AIT.PullRequestStatus.Domain.StatusPolicies;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Threading.Tasks;
using Xunit;

namespace AIT.PullRequestStatus.Domain.Tests.StatusPolicies
{
    public class DescriptionTaskListDoneStatusPolicyTests : StatusPolicyTestsBase<DescriptionTaskListDoneStatusPolicy>
    {
        [Fact]
        public async Task EvaluateAsync_FinishedTaskListInDescription_ShouldSetStatusToSuceeded()
        {
            var pullRequest = new GitPullRequest();
            pullRequest.Description = "[x] Task 1 [x] Task 2";

            await EvaluateAsync(pullRequest, GitStatusState.Succeeded);
        }

        [Fact]
        public async Task EvaluateAsync_NoTaskListInDescription_ShouldSetStatusToSuceeded()
        {
            var pullRequest = new GitPullRequest();
            pullRequest.Description = "Task 1 Task 2";

            await EvaluateAsync(pullRequest, GitStatusState.Succeeded);
        }

        [Fact]
        public async Task EvaluateAsync_UnfinishedTaskListInDescription_ShouldSetStatusToFailed()
        {
            var pullRequest = new GitPullRequest();
            pullRequest.Description = "[ ] Task 1 [x] Task 2";

            await EvaluateAsync(pullRequest, GitStatusState.Failed);
        }
    }
}
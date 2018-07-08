using AIT.PullRequestStatus.Domain.Services;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.StatusPolicies
{
    [StatusPolicyDescription("Description task list", "Pull requests are only successful if all tasks in the description task list are done or if there is no task list in the description.")]
    public class DescriptionTaskListDoneStatusPolicy : StatusPolicy
    {
        public DescriptionTaskListDoneStatusPolicy(IVssServiceFactory serviceFactory) : base(serviceFactory)
        {
        }

        protected override Task EvaluateInternalAsync(GitPullRequest pullRequest) => (pullRequest.Description?.Contains("[ ]") ?? false)
                ? UpdateStatusAsync(GitStatusState.Failed, "Task list unfinished")
                : UpdateStatusAsync(GitStatusState.Succeeded, "Task list done");
    }
}
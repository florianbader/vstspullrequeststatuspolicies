using AIT.PullRequestStatus.Domain.Services;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.StatusPolicies
{
    using PullRequestStatus = Microsoft.TeamFoundation.SourceControl.WebApi.PullRequestStatus;

    [StatusPolicyDescription("Work in progress", "Pull requests are only successful if there is no work in progress prefix in the pull request title.")]
    public class WorkInProgressStatusPolicy : StatusPolicy
    {
        private static readonly string[] _workInProgressPrefixes = new[]
        {
            "wip -",
            "[wip]",
            "(wip)",
            "wip:"
        };

        public WorkInProgressStatusPolicy(IVssServiceFactory vssConnectionFactory)
                                    : base(vssConnectionFactory)
        {
        }

        protected override async Task EvaluateInternalAsync(GitPullRequest pullRequest)
        {
            if (pullRequest.Status != PullRequestStatus.Active)
            {
                return;
            }

            if (_workInProgressPrefixes.Any(p => pullRequest.Title.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
            {
                await UpdateStatusAsync(GitStatusState.Failed, "Work in progress");
            }
            else
            {
                await UpdateStatusAsync(GitStatusState.Succeeded, "Work done");
            }
        }
    }
}
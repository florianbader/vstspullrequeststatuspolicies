using AIT.PullRequestStatus.Domain.Services;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.StatusPolicies
{
    [StatusPolicyDescription("Out-of-date with base branch", "Pull requests are only successful if they have the latest changes from the base branch.")]
    public class OutOfDateWithBaseBranchStatusPolicy : StatusPolicy
    {
        public OutOfDateWithBaseBranchStatusPolicy(IVssServiceFactory serviceFactory) : base(serviceFactory)
        {
        }

        protected override async Task EvaluateInternalAsync(GitPullRequest pullRequest)
        {
            var client = await ServiceFactory.GetClientAsync<GitHttpClient>();

            var sourceBranchName = pullRequest.SourceRefName.Replace("refs/heads/", string.Empty);

            var sourceBranch = await client.GetBranchAsync(pullRequest.Repository.Id, sourceBranchName);
            if ((sourceBranch?.BehindCount ?? 0) > 0)
            {
                await UpdateStatusAsync(GitStatusState.Failed, "Out-of-date with base branch");
            }
            else
            {
                await UpdateStatusAsync(GitStatusState.Succeeded, "Up-to-date with base branch");
            }
        }
    }
}
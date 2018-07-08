using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.StatusPolicies
{
    public class EmptyStatusPolicy : StatusPolicy
    {
        public EmptyStatusPolicy(Services.IVssServiceFactory serviceFactory) : base(serviceFactory)
        {
        }

        protected override Task EvaluateInternalAsync(GitPullRequest pullRequest) => Task.FromResult(0);
    }
}
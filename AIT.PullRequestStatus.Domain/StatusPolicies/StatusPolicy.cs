using AIT.PullRequestStatus.Domain.Services;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.StatusPolicies
{
    public abstract class StatusPolicy
    {
        protected GitPullRequest PullRequest { get; private set; }

        protected IVssServiceFactory ServiceFactory { get; }

        protected virtual string TargetUrl { get; }

        protected StatusPolicy(IVssServiceFactory serviceFactory)
        {
            ServiceFactory = serviceFactory;
        }

        public virtual async Task EvaluateAsync(GitPullRequest pullRequest)
        {
            PullRequest = pullRequest;

            try
            {
                await EvaluateInternalAsync(pullRequest);
            }
            catch
            {
                await UpdateStatusAsync(GitStatusState.Error, GetName() + " run into an error");
            }
        }

        protected abstract Task EvaluateInternalAsync(GitPullRequest pullRequest);

        protected async Task UpdateStatusAsync(GitStatusState state, string description)
        {
            var client = await ServiceFactory.GetClientAsync<GitHttpClient>();
            var status = new GitPullRequestStatus
            {
                State = state,
                Description = description,
                TargetUrl = TargetUrl,
                Context = new GitStatusContext
                {
                    Genre = "ait-policy",
                    Name = GetName()
                }
            };

            await client.CreatePullRequestStatusAsync(status, PullRequest.Repository.Id, PullRequest.PullRequestId);
        }

        private string GetName()
        {
            return GetType().Name.Replace("StatusPolicy", string.Empty).ToKebabCase();
        }
    }
}
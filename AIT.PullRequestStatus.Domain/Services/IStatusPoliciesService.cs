using AIT.PullRequestStatus.Domain.StatusPolicies;
using System.Collections.Generic;

namespace AIT.PullRequestStatus.Domain.Services
{
    public interface IStatusPoliciesService
    {
        IEnumerable<string> GetPolicies();

        StatusPolicy GetPolicy(IVssServiceFactory vssConnectionFactory, string statusPolicyName);
    }
}
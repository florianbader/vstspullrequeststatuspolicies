using AIT.PullRequestStatus.Domain.StatusPolicies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIT.PullRequestStatus.Domain.Services
{
    public class StatusPoliciesService : IStatusPoliciesService
    {
        private IEnumerable<string> _policies;

        public IEnumerable<string> GetPolicies()
        {
            if (_policies != null)
            {
                return _policies;
            }

            try
            {
                return _policies = typeof(PullRequestService).Assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && typeof(StatusPolicy).IsAssignableFrom(t) && t.Name != "EmptyStatusPolicy")
                        .Select(t => t.Name.Replace("StatusPolicy", string.Empty))
                        .ToList();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        public StatusPolicy GetPolicy(IVssServiceFactory vssConnectionFactory, string statusPolicyName)
        {
            try
            {
                statusPolicyName += "StatusPolicy";

                var type = typeof(PullRequestService).Assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(StatusPolicy).IsAssignableFrom(t))
                    .FirstOrDefault(t => t.Name == statusPolicyName);

                if (type == null)
                {
                    return new EmptyStatusPolicy(vssConnectionFactory);
                }

                return Activator.CreateInstance(type, vssConnectionFactory) as StatusPolicy
                    ?? new EmptyStatusPolicy(vssConnectionFactory);
            }
            catch
            {
                return new EmptyStatusPolicy(vssConnectionFactory);
            }
        }
    }
}
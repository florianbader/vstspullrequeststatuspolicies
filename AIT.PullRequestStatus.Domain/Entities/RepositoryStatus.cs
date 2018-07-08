using System.Collections.Generic;

namespace AIT.PullRequestStatus.Domain.Entities
{
    public class RepositoryStatus
    {
        public bool IsActivated { get; set; }

        public IEnumerable<RepositoryStatusPolicy> StatusPolicies { get; set; }
            = new List<RepositoryStatusPolicy>();
    }
}
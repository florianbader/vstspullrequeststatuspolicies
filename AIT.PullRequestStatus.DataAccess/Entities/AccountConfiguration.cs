using System.Collections.Generic;
using System.Linq;

namespace AIT.PullRequestStatus.DataAccess.Entities
{
    public class AccountConfiguration
    {
        public string BaseUrl { get; set; }

        public string CollectionId { get; set; }

        public string PersonalAccessToken { get; set; }

        public Dictionary<string, string[]> ProjectServiceHooks { get; }
            = new Dictionary<string, string[]>();

        public Dictionary<string, Dictionary<string, bool>> ProjectStatusPolicies { get; }
            = new Dictionary<string, Dictionary<string, bool>>();

        public Dictionary<string, string> ProjectToken { get; }
                            = new Dictionary<string, string>();

        public void Activate(string projectId, string token, IEnumerable<string> serviceHookIds)
        {
            if (IsActivated(projectId))
            {
                return;
            }

            ProjectServiceHooks[projectId] = serviceHookIds.ToArray();
            ProjectToken[projectId] = token;
        }

        public void Activate(string projectId, string repositoryId, string statusPolicyName)
        {
            var key = projectId + repositoryId;
            if (!ProjectStatusPolicies.ContainsKey(key))
            {
                ProjectStatusPolicies.Add(key, new Dictionary<string, bool>());
            }

            ProjectStatusPolicies[key][statusPolicyName] = true;
        }

        public void Deactivate(string projectId)
        {
            if (!IsActivated(projectId))
            {
                return;
            }

            ProjectServiceHooks.Remove(projectId);
        }

        public void Deactivate(string projectId, string repositoryId, string statusPolicyName)
        {
            var key = projectId + repositoryId;
            if (!ProjectStatusPolicies.ContainsKey(key))
            {
                return;
            }

            ProjectStatusPolicies[key][statusPolicyName] = false;
        }

        public IEnumerable<string> GetProjects() => ProjectServiceHooks.Keys;

        public IEnumerable<string> GetServiceHookIds(string projectId) => ProjectServiceHooks.ContainsKey(projectId)
            ? ProjectServiceHooks[projectId]
            : Enumerable.Empty<string>();

        public string GetToken(string projectId) => ProjectToken.ContainsKey(projectId)
            ? ProjectToken[projectId]
            : null;

        public bool HasActiveRepositories(string projectId) => ProjectStatusPolicies.Keys
                        .Where(k => k.StartsWith(projectId))
                .Any(k => ProjectStatusPolicies[k].Values.Any(t => t));

        public bool IsActivated(string projectId) => ProjectServiceHooks.ContainsKey(projectId);

        public bool IsActivated() => !string.IsNullOrEmpty(PersonalAccessToken);

        public bool IsStatusPolicyEnabled(string projectId, string repositoryId, string statusPolicyName)
        {
            var key = projectId + repositoryId;
            if (!ProjectStatusPolicies.ContainsKey(key))
            {
                return false;
            }

            var statusPolicies = ProjectStatusPolicies[key];
            return statusPolicies.ContainsKey(statusPolicyName) && statusPolicies[statusPolicyName];
        }

        public bool IsValidToken(string projectId, string token) => GetToken(projectId) == token;
    }
}
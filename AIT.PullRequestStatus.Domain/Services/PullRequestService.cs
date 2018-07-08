using AIT.PullRequestStatus.DataAccess.Entities;
using AIT.PullRequestStatus.DataAccess.Repositories;
using AIT.PullRequestStatus.Domain.Entities;
using AIT.PullRequestStatus.Domain.StatusPolicies;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Security.Client;
using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.Services
{
    public class PullRequestService : IPullRequestService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IVssConnectionFactory _connectionFactory;
        private readonly Uri _serviceHookUrl;
        private readonly IEnumerable<string> _statusPolicies;
        private readonly IStatusPoliciesService _statusPoliciesService;

        public PullRequestService(
            Uri serviceHookUrl,
            IVssConnectionFactory connectionFactory,
            IStatusPoliciesService statusPoliciesService,
            IConfigurationRepository configurationRepository)
        {
            _serviceHookUrl = serviceHookUrl;
            _connectionFactory = connectionFactory;
            _statusPoliciesService = statusPoliciesService;
            _configurationRepository = configurationRepository;

            _statusPolicies = statusPoliciesService.GetPolicies();
        }

        public async Task<bool> ActivateCollectionAsync(AccountConfiguration configuration)
        {
            var currentConfiguration = await _configurationRepository.GetAsync(configuration.CollectionId);
            if (currentConfiguration != null)
            {
                currentConfiguration.PersonalAccessToken = configuration.PersonalAccessToken;
                currentConfiguration.BaseUrl = configuration.BaseUrl;
                currentConfiguration.CollectionId = configuration.CollectionId;
                configuration = currentConfiguration;
            }

            var baseUrl = new Uri(configuration.BaseUrl);
            if (!baseUrl.Host.EndsWith("visualstudio.com"))
            {
                throw new ArgumentException("Invalid base url");
            }

            if (string.IsNullOrEmpty(configuration.PersonalAccessToken) || await HasPermissionAsync(configuration))
            {
                await _configurationRepository.UpdateAsync(configuration);
                return true;
            }

            return false;
        }

        public async Task ActivateStatusPolicyAsync(string collectionId, string projectId, string repositoryId, string statusPolicyName)
        {
            var configuration = await _configurationRepository.GetAsync(collectionId);

            if (!configuration.IsActivated())
            {
                return;
            }

            configuration.Activate(projectId, repositoryId, statusPolicyName);

            await ActivateProjectAsync(configuration, projectId);

            await _configurationRepository.UpdateAsync(configuration);
        }

        public async Task DeactivateStatusPolicyAsync(string collectionId, string projectId, string repositoryId, string statusPolicyName)
        {
            var configuration = await _configurationRepository.GetAsync(collectionId);

            if (!configuration.IsActivated())
            {
                return;
            }

            configuration.Deactivate(projectId, repositoryId, statusPolicyName);

            await DeactivateProjectAsync(configuration, projectId);

            await _configurationRepository.UpdateAsync(configuration);
        }

        public async Task EvaluateAsync(PullRequestUpdate pullRequestUpdate)
        {
            var configuration = await _configurationRepository.GetAsync(pullRequestUpdate.CollectionId);

            if (!configuration.IsActivated())
            {
                return;
            }

            if (!configuration.IsValidToken(pullRequestUpdate.ProjectId, pullRequestUpdate.Token))
            {
                throw new ArgumentException("Invalid token");
            }

            var serviceFactory = _connectionFactory.CreateFactory(new Uri(configuration.BaseUrl),
                new VssBasicCredential(string.Empty, configuration.PersonalAccessToken));

            var client = await serviceFactory.GetClientAsync<GitHttpClient>();
            var pullRequest = await client.GetPullRequestAsync(pullRequestUpdate.RepositoryId,
                pullRequestUpdate.Id);

            Parallel.ForEach(_statusPolicies, async statusPolicyName =>
            {
                try
                {
                    var isStatusPolicyEnabled = configuration.IsStatusPolicyEnabled(pullRequestUpdate.ProjectId,
                        pullRequestUpdate.RepositoryId, statusPolicyName);

                    if (isStatusPolicyEnabled)
                    {
                        var statusPolicy = _statusPoliciesService.GetPolicy(serviceFactory, statusPolicyName);
                        await statusPolicy.EvaluateAsync(pullRequest);
                    }
                }
                catch
                {
                    // TODO: log?
                }
            });
        }

        public async Task<CollectionStatus> GetCollectionStatusAsync(string collectionId)
        {
            var configuration = await _configurationRepository.GetAsync(collectionId);
            return new CollectionStatus { IsActivated = configuration?.IsActivated() ?? false };
        }

        public async Task<ProjectStatus> GetProjectStatusAsync(string collectionId, string projectId)
        {
            var configuration = await _configurationRepository.GetAsync(collectionId);

            if (!configuration.IsActivated())
            {
                return new ProjectStatus();
            }

            var serviceFactory = _connectionFactory.CreateFactory(new Uri(configuration.BaseUrl),
                new VssBasicCredential(string.Empty, configuration.PersonalAccessToken));

            var client = await serviceFactory.GetClientAsync<ServiceHooksPublisherHttpClient>();

            var serviceHookIds = configuration.GetServiceHookIds(projectId);

            var projectStatus = new ProjectStatus();

            if (!serviceHookIds.Any())
            {
                projectStatus.HasBrokenServiceHooks = true;
            }

            foreach (var serviceHookId in serviceHookIds)
            {
                try
                {
                    var subscription = await client.GetSubscriptionAsync(new Guid(serviceHookId));

                    if (subscription == null
                        || subscription.Status == SubscriptionStatus.OnProbation
                        || subscription.Status == SubscriptionStatus.DisabledBySystem
                        || subscription.Status == SubscriptionStatus.DisabledByInactiveIdentity)
                    {
                        projectStatus.HasBrokenServiceHooks = true;
                    }

                    if (subscription?.ConsumerInputs?["url"] != GetSubscriptionUrl(configuration.GetToken(projectId)))
                    {
                        projectStatus.HasBrokenServiceHooks = true;
                    }
                }
                catch
                {
                    projectStatus.HasBrokenServiceHooks = true;
                }
            }

            return projectStatus;
        }

        public async Task<RepositoryStatus> GetRepositoryStatusAsync(string collectionId, string projectId, string repositoryId)
        {
            var configuration = await _configurationRepository.GetAsync(collectionId);

            if (!configuration.IsActivated())
            {
                return new RepositoryStatus { IsActivated = false };
            }

            var projectStatusPolicies = new List<RepositoryStatusPolicy>();
            foreach (var statusPolicy in _statusPolicies)
            {
                var statusPolicyDescription = GetStatusPolicyDescription(statusPolicy);

                projectStatusPolicies.Add(new RepositoryStatusPolicy
                {
                    Id = statusPolicy,
                    Name = statusPolicyDescription.Name,
                    Description = statusPolicyDescription.Description,
                    IsActivated = configuration?.IsStatusPolicyEnabled(projectId, repositoryId, statusPolicy) ?? false
                });
            }

            return new RepositoryStatus
            {
                IsActivated = configuration?.IsActivated(projectId) ?? false,
                StatusPolicies = projectStatusPolicies
            };
        }

        public async Task ReactivateProjectAsync(string collectionId, string projectId)
        {
            var configuration = await _configurationRepository.GetAsync(collectionId);

            if (!configuration.IsActivated())
            {
                return;
            }

            await DeactivateProjectAsync(configuration, projectId, force: true);

            await ActivateProjectAsync(configuration, projectId, force: true);
        }

        private async Task ActivateProjectAsync(AccountConfiguration configuration, string projectId, bool force = false)
        {
            if (configuration.IsActivated(projectId) && !force)
            {
                return;
            }

            var serviceFactory = _connectionFactory.CreateFactory(new Uri(configuration.BaseUrl),
                new VssBasicCredential(string.Empty, configuration.PersonalAccessToken));

            var client = await serviceFactory.GetClientAsync<ServiceHooksPublisherHttpClient>();

            var token = Guid.NewGuid().ToString();

            var pullRequestCreatedSubscription =
                await client.CreateSubscriptionAsync(CreateSubscription(projectId, token, "git.pullrequest.created"));

            var pullRequestUpdatedSubscription =
                await client.CreateSubscriptionAsync(CreateSubscription(projectId, token, "git.pullrequest.updated"));

            configuration.Activate(projectId, token, new[]
            {
                pullRequestCreatedSubscription.Id.ToString(),
                pullRequestUpdatedSubscription.Id.ToString()
            });

            await _configurationRepository.UpdateAsync(configuration);
        }

        private Subscription CreateSubscription(string projectId, string token, string eventType)
        {
            return new Subscription
            {
                ConsumerId = "webHooks",
                ConsumerActionId = "httpRequest",
                ConsumerInputs = new Dictionary<string, string>
                {
                    { "url", GetSubscriptionUrl(token) }
                },
                EventType = eventType,
                PublisherId = "tfs",
                PublisherInputs = new Dictionary<string, string>
                {
                    { "projectId", projectId }
                },
            };
        }

        private async Task DeactivateProjectAsync(AccountConfiguration configuration, string projectId, bool force = false)
        {
            if (!configuration.IsActivated(projectId) && !force)
            {
                return;
            }

            if (configuration.HasActiveRepositories(projectId) && !force)
            {
                return;
            }

            var serviceFactory = _connectionFactory.CreateFactory(new Uri(configuration.BaseUrl),
                new VssBasicCredential(string.Empty, configuration.PersonalAccessToken));

            var client = await serviceFactory.GetClientAsync<ServiceHooksPublisherHttpClient>();

            var serviceHookIds = configuration.GetServiceHookIds(projectId);
            configuration.Deactivate(projectId);

            foreach (var serviceHookId in serviceHookIds)
            {
                try
                {
                    await client.DeleteSubscriptionAsync(new Guid(serviceHookId));
                }
                catch
                {
                    // TODO: log?
                }
            }

            await _configurationRepository.UpdateAsync(configuration);
        }

        private StatusPolicyDescriptionAttribute GetStatusPolicyDescription(string statusPolicy) =>
            Type.GetType(typeof(StatusPolicy).Namespace + "." + statusPolicy + "StatusPolicy")
                .GetAttribute<StatusPolicyDescriptionAttribute>();

        private string GetSubscriptionUrl(string token) => _serviceHookUrl.AbsoluteUri + "/servicehook?token=" + token;

        private async Task<bool> HasPermissionAsync(AccountConfiguration configuration)
        {
            const string ServiceHookSecurityNamespace = "cb594ebe-87dd-4fc9-ac2c-6a10a4c92046";
            const int ServiceHookEditPermission = 2;

            try
            {
                var serviceFactory = _connectionFactory.CreateFactory(new Uri(configuration.BaseUrl),
                    new VssBasicCredential(string.Empty, configuration.PersonalAccessToken));

                var client = await serviceFactory.GetClientAsync<SecurityHttpClient>();
                return await client.HasPermissionAsync(new Guid(ServiceHookSecurityNamespace), string.Empty, ServiceHookEditPermission, true);
            }
            catch
            {
                return false;
            }
        }
    }
}
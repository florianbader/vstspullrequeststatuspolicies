using AIT.PullRequestStatus.DataAccess.Entities;
using AIT.PullRequestStatus.DataAccess.Repositories;
using AIT.PullRequestStatus.Domain.Entities;
using AIT.PullRequestStatus.Domain.Services;
using AIT.PullRequestStatus.Domain.StatusPolicies;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AIT.PullRequestStatus.Domain.Tests.Services
{
    public class PullRequestServiceTests
    {
        [Fact(Skip = "Service hook client cannot be mocked in the preview version.")]
        public async Task ActivateAsync_NotActivated_ShouldCreateServiceHooksAndConfiguration()
        {
            var collectionId = Guid.NewGuid().ToString();
            var projectId = Guid.NewGuid().ToString();
            var repositoryId = Guid.NewGuid().ToString();
            var statusPolicyName = "test-status-policy";

            var configuration = new AccountConfiguration
            {
                BaseUrl = "https://fabrikam.visualstudio.com/DefaultCollection",
                CollectionId = collectionId
            };

            var serviceHookId1 = Guid.NewGuid();
            var serviceHookId2 = Guid.NewGuid();

            var serviceHookClientMock = new Mock<ServiceHooksPublisherHttpClient>(new object[]
            {
                new Uri("https://fabrikam.visualstudio.com/DefaultCollection"),
                (VssCredentials) new VssBasicCredential()
            });

            serviceHookClientMock
                .SetupSequence(m => m.CreateSubscriptionAsync(It.IsAny<Subscription>(), It.IsAny<object>()))
                .ReturnsAsync(new Subscription { Id = serviceHookId1 })
                .ReturnsAsync(new Subscription { Id = serviceHookId2 });

            var configurationRepositoryMock = new Mock<IConfigurationRepository>();

            configurationRepositoryMock
                .Setup(m => m.GetAsync(collectionId))
                .ReturnsAsync(configuration);

            var connectionFactoryMock = new Mock<IVssConnectionFactory>();

            connectionFactoryMock
                .Setup(m => m.CreateFactory(It.IsAny<Uri>(), It.IsAny<VssCredentials>()))
                .Returns(connectionFactoryMock.Object);

            connectionFactoryMock
                .Setup(m => m.GetClientAsync<ServiceHooksPublisherHttpClient>())
                .ReturnsAsync(serviceHookClientMock.Object);

            var statusPoliciesMock = new Mock<IStatusPoliciesService>();

            var pullRequestService = new PullRequestService(
                new Uri("https://aitpullrequests.azurewebsites.net/"),
                connectionFactoryMock.Object,
                statusPoliciesMock.Object,
                configurationRepositoryMock.Object);

            await pullRequestService.ActivateStatusPolicyAsync(collectionId, projectId, repositoryId, statusPolicyName);

            configurationRepositoryMock
                .Verify(m => m.GetAsync(collectionId), Times.Once);

            serviceHookClientMock
                .Verify(m => m.CreateSubscriptionAsync(It.IsAny<Subscription>(), It.IsAny<object>()), Times.Exactly(2));

            configurationRepositoryMock
                .Verify(m => m.UpdateAsync(
                    It.Is<AccountConfiguration>(c =>
                        c.GetServiceHookIds(projectId).First() == serviceHookId1.ToString()
                        && c.GetServiceHookIds(projectId).Last() == serviceHookId2.ToString())), Times.Once);
        }

        [Fact]
        public async Task EvaluateAsync_WithDisabledPolicy_ShouldNotEvaluatePolicy()
        {
            var pullRequestUpdate = new PullRequestUpdate
            {
                Id = 5,
                CollectionId = Guid.NewGuid().ToString(),
                ProjectId = Guid.NewGuid().ToString(),
                RepositoryId = Guid.NewGuid().ToString()
            };

            var accountConfiguration = new AccountConfiguration
            {
                CollectionId = pullRequestUpdate.CollectionId,
                BaseUrl = "https://fabrikam.visualstudio.com/DefaultCollection",
                PersonalAccessToken = "SECRET!!"
            };

            var pullRequest = new GitPullRequest();

            var gitClientMock = new Mock<GitHttpClient>(new object[]
            {
                new Uri("https://fabrikam.visualstudio.com/DefaultCollection"),
                (VssCredentials) new VssBasicCredential()
            });

            gitClientMock.Setup(m => m.GetPullRequestAsync(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pullRequest);

            var connectionFactoryMock = new Mock<IVssConnectionFactory>();
            connectionFactoryMock.Setup(m => m.CreateFactory(It.IsAny<Uri>(), It.IsAny<VssCredentials>()))
                .Returns(connectionFactoryMock.Object);
            connectionFactoryMock.Setup(m => m.GetClientAsync<GitHttpClient>())
                .ReturnsAsync(gitClientMock.Object);

            var testPolicyMock = new Mock<StatusPolicy>(new object[]
            {
                connectionFactoryMock.Object
            });
            testPolicyMock.Setup(m => m.EvaluateAsync(It.IsAny<GitPullRequest>()));

            const string policyName = "TestPolicy";

            var statusPoliciesServiceMock = new Mock<IStatusPoliciesService>();
            statusPoliciesServiceMock.Setup(m => m.GetPolicies())
                .Returns(new string[] { policyName });
            statusPoliciesServiceMock.Setup(m => m.GetPolicy(It.IsAny<IVssConnectionFactory>(), It.IsAny<string>()))
                .Returns(testPolicyMock.Object);

            var configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepositoryMock.Setup(m => m.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(accountConfiguration);

            var pullRequestService = new PullRequestService(
                new Uri("https://aitpullrequests.azurewebsites.net/"),
                connectionFactoryMock.Object,
                statusPoliciesServiceMock.Object,
                configurationRepositoryMock.Object);

            await pullRequestService.EvaluateAsync(pullRequestUpdate);

            configurationRepositoryMock.Verify(m => m.GetAsync(pullRequestUpdate.CollectionId), Times.Once);

            connectionFactoryMock.Verify(m => m.CreateFactory(new Uri(accountConfiguration.BaseUrl), It.IsAny<VssCredentials>()), Times.Once);

            gitClientMock.Verify(m => m.GetPullRequestAsync(pullRequestUpdate.RepositoryId, pullRequestUpdate.Id,
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

            statusPoliciesServiceMock.Verify(m => m.GetPolicies(), Times.Once);

            statusPoliciesServiceMock.Verify(m => m.GetPolicy(connectionFactoryMock.Object, policyName), Times.Never);

            testPolicyMock.Verify(m => m.EvaluateAsync(pullRequest), Times.Never);
        }
    }
}
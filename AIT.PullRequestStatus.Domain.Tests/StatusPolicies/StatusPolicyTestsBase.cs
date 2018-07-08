using AIT.PullRequestStatus.Domain.Services;
using AIT.PullRequestStatus.Domain.StatusPolicies;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.Tests.StatusPolicies
{
    using PullRequestStatus = Microsoft.TeamFoundation.SourceControl.WebApi.PullRequestStatus;

    public abstract class StatusPolicyTestsBase<T> where T : StatusPolicy
    {
        protected readonly Mock<IVssServiceFactory> _connectionFactoryMock;
        protected readonly Mock<GitHttpClient> _gitClientMock;

        protected StatusPolicyTestsBase()
        {
            _gitClientMock = new Mock<GitHttpClient>(new object[]
            {
                new Uri("https://fabrikam.visualstudio.com/DefaultCollection"),
                (VssCredentials) new VssBasicCredential()
            });

            _gitClientMock.Setup(m => m.CreatePullRequestStatusAsync(It.IsAny<GitPullRequestStatus>(),
                    It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync<GitPullRequestStatus, Guid, int, object, CancellationToken, GitHttpClient, GitPullRequestStatus>((s, _, __, ___, ____) => s);

            _connectionFactoryMock = new Mock<IVssServiceFactory>();
        }

        public async Task EvaluateAsync(GitPullRequest pullRequest, GitStatusState expectedState)
        {
            pullRequest.PullRequestId = 5;
            pullRequest.Status = PullRequestStatus.Active;
            pullRequest.Repository = new GitRepository
            {
                Id = Guid.NewGuid()
            };

            _connectionFactoryMock.Setup(c => c.GetClientAsync<GitHttpClient>()).ReturnsAsync(_gitClientMock.Object);

            var statusPolicy = Activator.CreateInstance(typeof(T), _connectionFactoryMock.Object) as StatusPolicy;
            await statusPolicy.EvaluateAsync(pullRequest);

            _gitClientMock.Verify(m => m.CreatePullRequestStatusAsync(It.Is<GitPullRequestStatus>(p =>
                    p.State == expectedState && p.Context.Name == typeof(T).Name.Replace("StatusPolicy", string.Empty).ToKebabCase()),
                pullRequest.Repository.Id, pullRequest.PullRequestId, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
using AIT.PullRequestStatus.Domain.StatusPolicies;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AIT.PullRequestStatus.Domain.Tests.StatusPolicies
{
    public class OutOfDateWithBaseBranchStatusPolicyTests : StatusPolicyTestsBase<OutOfDateWithBaseBranchStatusPolicy>
    {
        [Fact]
        public async Task EvaluateAsync_BehindBaseBranch_ShouldSetStatusToFailed()
        {
            var pullRequest = new GitPullRequest
            {
                SourceRefName = "feature/branch"
            };

            var branchStats = new GitBranchStats
            {
                BehindCount = 1
            };

            _gitClientMock.Setup(g => g.GetBranchAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<GitVersionDescriptor>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(branchStats);

            await EvaluateAsync(pullRequest, GitStatusState.Failed);

            _gitClientMock.Verify(g => g.GetBranchAsync(pullRequest.Repository.Id, pullRequest.SourceRefName,
                    It.IsAny<GitVersionDescriptor>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EvaluateAsync_NotBehindBaseBranch_ShouldSetStatusToSuceeded()
        {
            var pullRequest = new GitPullRequest
            {
                SourceRefName = "feature/branch"
            };

            var branchStats = new GitBranchStats
            {
                BehindCount = 0
            };

            _gitClientMock.Setup(g => g.GetBranchAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<GitVersionDescriptor>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(branchStats);

            await EvaluateAsync(pullRequest, GitStatusState.Succeeded);

            _gitClientMock.Verify(g => g.GetBranchAsync(pullRequest.Repository.Id, pullRequest.SourceRefName,
                    It.IsAny<GitVersionDescriptor>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
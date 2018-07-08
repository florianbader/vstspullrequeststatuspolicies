using AIT.PullRequestStatus.DataAccess.Entities;
using FluentAssertions;
using System;
using Xunit;

namespace AIT.PullRequestStatus.DataAccess.Tests.Entities
{
    public class AccountConfigurationTests
    {
        [Fact]
        public void ActivateProject_Activated_ShouldActivateProjectAndReturnsServiceHookIds()
        {
            var projectId = Guid.NewGuid().ToString();
            var serviceHookIds1 = new[]
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            var serviceHookIds2 = new[]
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            var accountConfiguration = new AccountConfiguration();
            accountConfiguration.Activate(projectId, new Guid().ToString(), serviceHookIds1);
            accountConfiguration.Activate(projectId, new Guid().ToString(), serviceHookIds2);

            accountConfiguration.IsActivated(projectId).Should().BeTrue();
            accountConfiguration.GetServiceHookIds(projectId).Should().Contain(serviceHookIds1);
        }

        [Fact]
        public void ActivateProject_NotActivated_ShouldActivateProjectAndReturnsServiceHookIds()
        {
            var projectId = Guid.NewGuid().ToString();
            var serviceHookIds = new[]
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            var accountConfiguration = new AccountConfiguration();
            accountConfiguration.Activate(projectId, new Guid().ToString(), serviceHookIds);

            accountConfiguration.IsActivated(projectId).Should().BeTrue();
            accountConfiguration.GetServiceHookIds(projectId).Should().Contain(serviceHookIds);
        }

        [Fact]
        public void ActivateStatusPolicy_NotActivated_ShouldReturnActivated()
        {
            var projectId = Guid.NewGuid().ToString();
            var repositoryId = Guid.NewGuid().ToString();
            var statusPolicyName = "TestPolic";

            var accountConfiguration = new AccountConfiguration();
            accountConfiguration.Activate(projectId, repositoryId, statusPolicyName);

            accountConfiguration.IsStatusPolicyEnabled(projectId, repositoryId, statusPolicyName).Should().BeTrue();
            accountConfiguration.HasActiveRepositories(projectId).Should().BeTrue();
        }

        [Fact]
        public void Constructor_NotActivated_ShouldReturnNotActivated()
        {
            var projectId = Guid.NewGuid().ToString();

            var accountConfiguration = new AccountConfiguration();

            accountConfiguration.IsActivated(projectId).Should().BeFalse();
            accountConfiguration.GetServiceHookIds(projectId).Should().BeEmpty();
        }

        [Fact]
        public void DeactivateProject_Activated_ShouldDeactivateProjectAndNotReturnsServiceHookIds()
        {
            var projectId = Guid.NewGuid().ToString();
            var serviceHookIds = new[]
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            var accountConfiguration = new AccountConfiguration();
            accountConfiguration.Activate(projectId, new Guid().ToString(), serviceHookIds);
            accountConfiguration.Deactivate(projectId);

            accountConfiguration.IsActivated(projectId).Should().BeFalse();
            accountConfiguration.GetServiceHookIds(projectId).Should().BeEmpty();
        }

        [Fact]
        public void DeactivateProject_NotActivated_ShouldReturnNotActivated()
        {
            var projectId = Guid.NewGuid().ToString();

            var accountConfiguration = new AccountConfiguration();
            accountConfiguration.Deactivate(projectId);

            accountConfiguration.IsActivated(projectId).Should().BeFalse();
        }

        [Fact]
        public void DeactivateStatusPolicy_Activated_ShouldReturnNotActivated()
        {
            var projectId = Guid.NewGuid().ToString();
            var repositoryId = Guid.NewGuid().ToString();
            var statusPolicyName = "TestPolic";

            var accountConfiguration = new AccountConfiguration();
            accountConfiguration.Activate(projectId, repositoryId, statusPolicyName);
            accountConfiguration.Deactivate(projectId, repositoryId, statusPolicyName);

            accountConfiguration.IsStatusPolicyEnabled(projectId, repositoryId, statusPolicyName).Should().BeFalse();
            accountConfiguration.HasActiveRepositories(projectId).Should().BeFalse();
        }

        [Fact]
        public void HasActivateRepositories_StillOneActive_ShouldReturnTrue()
        {
            var projectId = Guid.NewGuid().ToString();
            var repositoryId = Guid.NewGuid().ToString();
            var statusPolicyName = "TestPolic";

            var accountConfiguration = new AccountConfiguration();
            accountConfiguration.Activate(projectId, repositoryId, statusPolicyName);
            accountConfiguration.Activate(projectId, repositoryId, statusPolicyName + "2");
            accountConfiguration.Deactivate(projectId, repositoryId, statusPolicyName);

            accountConfiguration.HasActiveRepositories(projectId).Should().BeTrue();
        }
    }
}
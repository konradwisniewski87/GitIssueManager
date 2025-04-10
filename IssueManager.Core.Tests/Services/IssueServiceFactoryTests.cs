using FluentAssertions;
using IssueManager.Core.Models.Enums;
using IssueManager.Core.Services;
using Moq;

namespace IssueManager.Core.Tests.Services;

public class IssueServiceFactoryTests
{
    [Fact]
    public void GetService_Should_Return_GitHubService_When_Type_Is_GitHub()
    {
        var gitHubServiceMock = new Mock<GitHubIssueService>(new HttpClient(), "dummy-token");
        var gitLabServiceMock = new Mock<GitLabIssueService>(new HttpClient(), "dummy-token");

        var factory = new IssueServiceFactory(gitHubServiceMock.Object, gitLabServiceMock.Object);

        var result = factory.GetService(IssueServiceType.GitHub);

        result.Should().BeSameAs(gitHubServiceMock.Object);
    }

    [Fact]
    public void GetService_Should_Return_GitLabService_When_Type_Is_GitLab()
    {
        var gitHubServiceMock = new Mock<GitHubIssueService>(new HttpClient(), "dummy-token");
        var gitLabServiceMock = new Mock<GitLabIssueService>(new HttpClient(), "dummy-token");

        var factory = new IssueServiceFactory(gitHubServiceMock.Object, gitLabServiceMock.Object);

        var result = factory.GetService(IssueServiceType.GitLab);

        result.Should().BeSameAs(gitLabServiceMock.Object);
    }

    [Fact]
    public void GetService_Should_Throw_ArgumentOutOfRangeException_When_Type_Is_Unknown()
    {
        var gitHubServiceMock = new Mock<GitHubIssueService>(new HttpClient(), "dummy-token");
        var gitLabServiceMock = new Mock<GitLabIssueService>(new HttpClient(), "dummy-token");

        var factory = new IssueServiceFactory(gitHubServiceMock.Object, gitLabServiceMock.Object);

        var act = () => factory.GetService((IssueServiceType)999);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("Unsupported provider*");
    }
}

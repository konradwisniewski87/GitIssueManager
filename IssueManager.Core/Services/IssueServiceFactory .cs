using IssueManager.Core.Interfaces;
using IssueManager.Core.Models.Enums;

namespace IssueManager.Core.Services;

public class IssueServiceFactory : IIssueServiceFactory
{
    private readonly GitHubIssueService _gitHubService;
    private readonly GitLabIssueService _gitLabService;

    public IssueServiceFactory(GitHubIssueService gitHubService, GitLabIssueService gitLabService)
    {
        _gitHubService = gitHubService;
        _gitLabService = gitLabService;
    }

    public IIssueService GetService(IssueServiceType type)
    {
        return type switch
        {
            IssueServiceType.GitHub => _gitHubService,
            IssueServiceType.GitLab => _gitLabService,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported provider: {type}")
        };
    }
}

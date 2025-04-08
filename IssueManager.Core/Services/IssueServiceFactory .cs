using IssueManager.Core.Interfaces;

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

    public IIssueService GetService(string provider)
    {
        return provider.ToLower() switch
        {
            "github" => _gitHubService,
            "gitlab" => _gitLabService,
            _ => throw new ArgumentException("Unsupported provider", nameof(provider))
        };
    }
}

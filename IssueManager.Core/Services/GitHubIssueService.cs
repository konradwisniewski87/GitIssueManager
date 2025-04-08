using IssueManager.Core.Interfaces;
using IssueManager.Core.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using IssueManager.Core.Services.Helpers;

namespace IssueManager.Core.Services;

public class GitHubIssueService : IIssueService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GitHubIssueService(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _token = token;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("IssueManagerApp");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    public async Task<IssueResponse> CreateIssueAsync(string repository, IssueRequest issue)
    {
        var url = $"https://api.github.com/repos/{repository}/issues";

        var payload = new
        {
            title = issue.Title,
            body = issue.Description
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var githubIssue = JsonSerializer.Deserialize<GitHubIssueDto>(json, _jsonOptions);

        return new IssueResponse
        {
            Id = githubIssue!.Number,
            Title = githubIssue.Title,
            State = HelpersMethod.MapState(githubIssue.State),
            Url = githubIssue.HtmlUrl
        };
    }

    public async Task<IssueResponse> UpdateIssueAsync(string repository, int issueId, IssueRequest issue)
    {
        var url = $"https://api.github.com/repos/{repository}/issues/{issueId}";

        var payload = new
        {
            title = issue.Title,
            body = issue.Description
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var githubIssue = JsonSerializer.Deserialize<GitHubIssueDto>(json, _jsonOptions);

        return new IssueResponse
        {
            Id = githubIssue!.Number,
            Title = githubIssue.Title,
            State = MapState(githubIssue.State),
            Url = githubIssue.HtmlUrl
        };
    }

    public async Task CloseIssueAsync(string repository, int issueId)
    {
        var url = $"https://api.github.com/repos/{repository}/issues/{issueId}";

        var payload = new
        {
            state = "closed"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync(url, content); // <- rozszerzenie
        response.EnsureSuccessStatusCode();
    }

    private class GitHubIssueDto
    {
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
    }
}

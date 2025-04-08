using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IssueManager.Core.Interfaces;
using IssueManager.Core.Models;
using IssueManager.Core.Models.Helpers;

namespace IssueManager.Core.Services;

public class GitLabIssueService : IIssueService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GitLabIssueService(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _token = token;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("IssueManagerApp");
    }

    public async Task<IssueResponse> CreateIssueAsync(string repository, IssueRequest issue)
    {
        // repository = namespace/project, np. "group-name/repo-name"
        var encodedRepo = Uri.EscapeDataString(repository);
        var url = $"https://gitlab.com/api/v4/projects/{encodedRepo}/issues";

        var payload = new
        {
            title = issue.Title,
            description = issue.Description
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var gitlabIssue = JsonSerializer.Deserialize<GitLabIssueDto>(json, _jsonOptions)!;

        return new IssueResponse
        {
            Id = gitlabIssue.Iid,
            Title = gitlabIssue.Title,
            State = MapState(gitlabIssue.State),
            Url = gitlabIssue.WebUrl
        };
    }

    private IssueState MapState(string state)
    {
        throw new NotImplementedException();
    }

    public async Task<IssueResponse> UpdateIssueAsync(string repository, int issueId, IssueRequest issue)
    {
        var encodedRepo = Uri.EscapeDataString(repository);
        var url = $"https://gitlab.com/api/v4/projects/{encodedRepo}/issues/{issueId}";

        var payload = new
        {
            title = issue.Title,
            description = issue.Description
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var gitlabIssue = JsonSerializer.Deserialize<GitLabIssueDto>(json, _jsonOptions)!;

        return new IssueResponse
        {
            Id = gitlabIssue.Iid,
            Title = gitlabIssue.Title,
            State = MapState(gitlabIssue.State),
            Url = gitlabIssue.WebUrl
        };
    }

    public async Task CloseIssueAsync(string repository, int issueId)
    {
        var encodedRepo = Uri.EscapeDataString(repository);
        var url = $"https://gitlab.com/api/v4/projects/{encodedRepo}/issues/{issueId}";

        var payload = new
        {
            state_event = "close"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    private class GitLabIssueDto
    {
        public int Iid { get; set; } // NOT "id" - "iid" is the issue number within project
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string WebUrl { get; set; } = string.Empty;
    }
}

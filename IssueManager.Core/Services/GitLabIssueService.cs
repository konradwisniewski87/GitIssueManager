using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IssueManager.Core.Interfaces;
using IssueManager.Core.Models;
using IssueManager.Core.Services.Helpers;

namespace IssueManager.Core.Services;

public class GitLabIssueService : IIssueService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly string _accept = "application/json";
    private readonly string _contentType = "application/json";
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
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_accept));
    }

    public async Task<IssueResponse> CreateIssueAsync(string repository, IssueRequest issue)
    {
        var encodedRepo = Uri.EscapeDataString(repository);
        var url = $"https://gitlab.com/api/v4/projects/{encodedRepo}/issues";

        var payload = new
        {
            title = issue.Title,
            description = issue.Description
        };

        var content = CreateJsonContent(payload);
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var gitlabIssue = JsonSerializer.Deserialize<GitLabIssueDto>(json, _jsonOptions)!;

        return new IssueResponse
        {
            Id = gitlabIssue.Iid,
            Title = gitlabIssue.Title,
            State = HelpersMethod.MapState(gitlabIssue.State),
            Url = gitlabIssue.WebUrl
        };
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

        var content = CreateJsonContent(payload);
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var gitlabIssue = JsonSerializer.Deserialize<GitLabIssueDto>(json, _jsonOptions)!;

        return new IssueResponse
        {
            Id = gitlabIssue.Iid,
            Title = gitlabIssue.Title,
            State = HelpersMethod.MapState(gitlabIssue.State),
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

        var content = CreateJsonContent(payload);
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    private StringContent CreateJsonContent(object payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, _contentType);
    }

    private class GitLabIssueDto
    {
        public int Iid { get; set; }
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string WebUrl { get; set; } = string.Empty;
    }
}

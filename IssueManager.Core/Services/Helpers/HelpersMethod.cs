using IssueManager.Core.Models.Enums;
using System.Net;

namespace IssueManager.Core.Services.Helpers;

internal static class HelpersMethod
{
    internal static IssueState MapState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new InvalidOperationException($"Unknown value, variable is WhiteSpace or null");

        return state.ToLower() switch
        {
            "open" or "opened" => IssueState.Open,
            "closed" => IssueState.Closed,
            _ => throw new InvalidOperationException($"Unknown Git issue state: {state}")
        };
    }

    internal static void HandleGitHubErrors(HttpResponseMessage response, string context)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException($"{context} not found.");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Unauthorized GitHub access.");

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"GitHub error: {(int)response.StatusCode} {response.ReasonPhrase}");
    }
}

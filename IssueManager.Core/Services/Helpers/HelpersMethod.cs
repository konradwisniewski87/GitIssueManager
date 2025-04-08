using IssueManager.Core.Models.Helpers;

namespace IssueManager.Core.Services.Helpers;

internal static class HelpersMethod
{
    internal static IssueState MapState(string state) => state.ToLower() switch
    {
        "open" => IssueState.Open,
        "closed" => IssueState.Closed,
        _ => throw new InvalidOperationException($"Unknown Git issue state: {state}")
    };
}

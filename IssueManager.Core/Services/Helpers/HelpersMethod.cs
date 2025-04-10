using IssueManager.Core.Models.Enums;

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
}

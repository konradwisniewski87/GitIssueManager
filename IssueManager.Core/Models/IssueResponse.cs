using IssueManager.Core.Models.Enums;

namespace IssueManager.Core.Models;

public class IssueResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public IssueState State { get; set; }
    public string Url { get; set; } = string.Empty;
}

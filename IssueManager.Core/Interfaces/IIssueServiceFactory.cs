using IssueManager.Core.Models.Enums;

namespace IssueManager.Core.Interfaces;

public interface IIssueServiceFactory
{
    IIssueService GetService(IssueServiceType type);
}
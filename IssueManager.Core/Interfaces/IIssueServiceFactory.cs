namespace IssueManager.Core.Interfaces;

public interface IIssueServiceFactory
{
    IIssueService GetService(string provider);
}

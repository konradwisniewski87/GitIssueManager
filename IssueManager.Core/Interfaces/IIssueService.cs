namespace IssueManager.Core.Interfaces;

using IssueManager.Core.Models;
using System.Threading.Tasks;

public interface IIssueService
{
    public Task<IssueResponse> CreateIssueAsync(string repository, IssueRequest issue);
    public Task<IssueResponse> UpdateIssueAsync(string repository, int issueId, IssueRequest issue);
    public Task CloseIssueAsync(string repository, int issueId);
}

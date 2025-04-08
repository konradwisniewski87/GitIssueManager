using IssueManager.Core.Interfaces;
using IssueManager.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace IssueManager.API.Controllers;

[ApiController]
[Route("api/issues/{provider}/{repo}")]
public class IssuesController : ControllerBase
{
    private readonly IIssueServiceFactory _serviceFactory;

    public IssuesController(IIssueServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    [HttpPost]
    public async Task<IActionResult> CreateIssue(string provider, string repo, [FromBody] IssueRequest request)
    {
        var service = _serviceFactory.GetService(provider);
        var response = await service.CreateIssueAsync(repo, request);
        return Ok(response);
    }

    [HttpPut("{issueId:int}")]
    public async Task<IActionResult> UpdateIssue(string provider, string repo, int issueId, [FromBody] IssueRequest request)
    {
        var service = _serviceFactory.GetService(provider);
        var response = await service.UpdateIssueAsync(repo, issueId, request);
        return Ok(response);
    }

    [HttpPatch("{issueId:int}/close")]
    public async Task<IActionResult> CloseIssue(string provider, string repo, int issueId)
    {
        var service = _serviceFactory.GetService(provider);
        await service.CloseIssueAsync(repo, issueId);
        return NoContent();
    }
}

using IssueManager.API.Controllers.V1.Helpers;
using IssueManager.Core.Interfaces;
using IssueManager.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace IssueManager.API.Controllers;

[ApiController]
[Route("api/issues")]
public class IssuesController : ControllerBase
{
    private readonly IIssueServiceFactory _serviceFactory;

    public IssuesController(IIssueServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    [HttpPost("{provider}/{owner}/{repo}")]
    public async Task<IActionResult> CreateIssue(string provider, string owner, string repo, [FromBody] IssueRequest request)
    {
        if (!IssueServiceHelper.TryGetServiceAndRepo(_serviceFactory, provider, owner, repo, out var service, out var fullRepo, out var errorResult))
            return errorResult;

        var response = await service.CreateIssueAsync(fullRepo, request);
        return Ok(response);
    }

    [HttpPut("{provider}/{owner}/{repo}/{issueId:int}")]
    public async Task<IActionResult> UpdateIssue(string provider, string owner, string repo, int issueId, [FromBody] IssueRequest request)
    {
        if (!IssueServiceHelper.TryGetServiceAndRepo(_serviceFactory, provider, owner, repo, out var service, out var fullRepo, out var errorResult))
            return errorResult;

        var response = await service.UpdateIssueAsync(fullRepo, issueId, request);
        return Ok(response);
    }

    [HttpPatch("{provider}/{owner}/{repo}/{issueId:int}/close")]
    public async Task<IActionResult> CloseIssue(string provider, string owner, string repo, int issueId)
    {
        if (!IssueServiceHelper.TryGetServiceAndRepo(_serviceFactory, provider, owner, repo, out var service, out var fullRepo, out var errorResult))
            return errorResult;

        await service.CloseIssueAsync(fullRepo, issueId);
        return NoContent();
    }
}

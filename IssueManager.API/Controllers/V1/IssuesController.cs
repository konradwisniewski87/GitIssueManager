using IssueManager.Core.Interfaces;
using IssueManager.Core.Models;
using IssueManager.Core.Models.Enums;
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
        if (!TryGetServiceType(provider, out var serviceType))
            return BadRequest("Invalid provider. Allowed values: GitHub, GitLab");

        var fullRepo = $"{owner}/{repo}";
        var service = _serviceFactory.GetService(serviceType);
        var response = await service.CreateIssueAsync(fullRepo, request);
        return Ok(response);
    }

    [HttpPut("{provider}/{owner}/{repo}/{issueId:int}")]
    public async Task<IActionResult> UpdateIssue(string provider, string owner, string repo, int issueId, [FromBody] IssueRequest request)
    {
        if (!TryGetServiceType(provider, out var serviceType))
            return BadRequest("Invalid provider. Allowed values: GitHub, GitLab");

        var fullRepo = $"{owner}/{repo}";
        var service = _serviceFactory.GetService(serviceType);
        var response = await service.UpdateIssueAsync(fullRepo, issueId, request);
        return Ok(response);
    }

    [HttpPatch("{provider}/{owner}/{repo}/{issueId:int}/close")]
    public async Task<IActionResult> CloseIssue(string provider, string owner, string repo, int issueId)
    {
        if (!TryGetServiceType(provider, out var serviceType))
            return BadRequest("Invalid provider. Allowed values: GitHub, GitLab");

        var fullRepo = $"{owner}/{repo}";
        var service = _serviceFactory.GetService(serviceType);
        await service.CloseIssueAsync(fullRepo, issueId);
        return NoContent();
    }

    private bool TryGetServiceType(string provider, out IssueServiceType serviceType)
    {
        return Enum.TryParse(provider, ignoreCase: true, out serviceType);
    }
}

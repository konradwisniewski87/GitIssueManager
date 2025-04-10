using IssueManager.Core.Interfaces;
using IssueManager.Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace IssueManager.API.Controllers.V1.Helpers;

public static class IssueServiceHelper
{
    public static bool TryGetServiceAndRepo(IIssueServiceFactory factory, string provider, string owner, string repo,
        out IIssueService service, out string fullRepo, out IActionResult errorResult)
    {
        fullRepo = $"{owner}/{repo}";

        if (!Enum.TryParse<IssueServiceType>(provider, ignoreCase: true, out var serviceType))
        {
            var allowedValues = string.Join(", ", Enum.GetNames(typeof(IssueServiceType)));
            errorResult = new BadRequestObjectResult($"Invalid provider. Allowed values: {allowedValues}");
            service = null!;
            return false;
        }

        service = factory.GetService(serviceType);
        errorResult = null!;
        return true;
    }
}

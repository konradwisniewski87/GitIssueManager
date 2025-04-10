using System.Text;
using System.Text.Json;

namespace IssueManager.Core.Services.Helpers;

public static class JsonContentHelper
{
    private static string _contentType = "application/json";
    public static StringContent Create(object payload)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return new StringContent(JsonSerializer.Serialize(payload, options), Encoding.UTF8, _contentType);
    }
}

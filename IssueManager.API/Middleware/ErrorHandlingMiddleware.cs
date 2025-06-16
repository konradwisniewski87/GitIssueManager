using System.Net;
using System.Text.Json;

namespace IssueManager.API.Middleware;

public class ErrorHandlingMiddleware 
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var code = ex switch
        {
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound => HttpStatusCode.NotFound,
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Unauthorized => HttpStatusCode.Unauthorized,
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Forbidden => HttpStatusCode.Forbidden,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var errorResponse = new
        {
            status = (int)code,
            message = ex.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var json = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(json);
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace uPreviewShare.Middleware;

/// <summary>
/// Middleware that intercepts requests to /upreviewshare/ routes.
/// Handles cross-cutting concerns: request logging and exception handling.
/// Returns 404 for any unhandled exception on public routes to avoid information leakage.
/// </summary>
public class uPreviewShareMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<uPreviewShareMiddleware> _logger;

    public uPreviewShareMiddleware(RequestDelegate next, ILogger<uPreviewShareMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.Value;
        var requestMethod = context.Request.Method;

        _logger.LogDebug("uPreviewShare request: {Method} {Path}", requestMethod, requestPath);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in uPreviewShare pipeline: {Method} {Path}", requestMethod, requestPath);
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}

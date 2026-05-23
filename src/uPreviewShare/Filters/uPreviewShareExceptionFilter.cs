using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace uPreviewShare.Filters;

/// <summary>
/// Global exception filter for uPreviewShare controllers.
/// Logs exceptions internally and returns generic error responses
/// without stack traces or internal details to prevent information leakage.
/// </summary>
public class uPreviewShareExceptionFilter : IExceptionFilter
{
    private readonly ILogger<uPreviewShareExceptionFilter> _logger;

    public uPreviewShareExceptionFilter(ILogger<uPreviewShareExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in uPreviewShare controller: {Path}", context.HttpContext.Request.Path);

        if (context.HttpContext.Request.Path.StartsWithSegments("/upreviewshare"))
        {
            context.Result = new NotFoundResult();
        }
        else
        {
            context.Result = new ObjectResult(new { error = "An internal error occurred." }) { StatusCode = 500 };
        }

        context.ExceptionHandled = true;
    }
}

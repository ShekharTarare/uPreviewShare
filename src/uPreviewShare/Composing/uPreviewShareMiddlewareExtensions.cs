using Microsoft.AspNetCore.Builder;
using uPreviewShare.Middleware;

namespace uPreviewShare.Composing;

/// <summary>
/// Extension methods for registering the uPreviewShare middleware in the ASP.NET Core pipeline.
/// </summary>
public static class uPreviewShareMiddlewareExtensions
{
    /// <summary>
    /// Registers the uPreviewShare middleware with a path filter so it only activates
    /// for /upreviewshare/ routes, avoiding overhead on all other Umbraco requests.
    /// </summary>
    public static IApplicationBuilder UseuPreviewShareMiddleware(this IApplicationBuilder app)
    {
        app.UseWhen(
            context => context.Request.Path.StartsWithSegments("/upreviewshare"),
            appBuilder => appBuilder.UseMiddleware<uPreviewShareMiddleware>());

        return app;
    }
}

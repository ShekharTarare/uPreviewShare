using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace uPreviewShare.Filters;

/// <summary>
/// Result filter that injects a floating "Draft Preview" bar at the bottom of
/// pages rendered through Umbraco's template engine via uPreviewShare.
/// Only activates when the request has the uPreviewShare preview marker set.
/// </summary>
public class uPreviewSharePreviewBarFilter : IAsyncResultFilter
{
    private const string PreviewMarkerKey = "uPreviewShare.IsPreview";

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Only inject the bar if this is a template-rendered preview
        if (!context.HttpContext.Items.ContainsKey(PreviewMarkerKey))
        {
            await next();
            return;
        }

        var isDraft = context.HttpContext.Items.TryGetValue("uPreviewShare.IsDraft", out var draftVal) && draftVal is true;

        // Capture the original response body
        var originalBody = context.HttpContext.Response.Body;
        using var memoryStream = new MemoryStream();
        context.HttpContext.Response.Body = memoryStream;

        await next();

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        // Inject the floating bar before </body>
        var barHtml = GetPreviewBarHtml(isDraft);
        var closingBodyIndex = responseBody.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
        if (closingBodyIndex >= 0)
        {
            responseBody = responseBody.Insert(closingBodyIndex, barHtml);
        }
        else
        {
            // No </body> tag found, append at the end
            responseBody += barHtml;
        }

        var bytes = Encoding.UTF8.GetBytes(responseBody);
        context.HttpContext.Response.Body = originalBody;
        context.HttpContext.Response.ContentLength = bytes.Length;
        await originalBody.WriteAsync(bytes);
    }

    private static string GetPreviewBarHtml(bool isDraft)
    {
        var badgeText = isDraft ? "Draft Preview" : "Published";
        var badgeColor = isDraft ? "#8B5CF6" : "#059669";
        var message = isDraft
            ? "You are viewing an unpublished draft of this page."
            : "You are viewing the published version of this page.";

        return $"""
        <div id="ups-preview-bar" style="position:fixed;bottom:0;left:0;right:0;z-index:999999;background:#1e293b;color:#fff;padding:10px 20px;display:flex;align-items:center;justify-content:space-between;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;font-size:13px;box-shadow:0 -2px 8px rgba(0,0,0,0.15);">
            <div style="display:flex;align-items:center;gap:10px;">
                <span style="background:{badgeColor};color:#fff;padding:3px 10px;border-radius:9999px;font-size:11px;font-weight:600;text-transform:uppercase;letter-spacing:0.5px;">{badgeText}</span>
                <span style="color:#94a3b8;">{message}</span>
            </div>
            <div style="color:#64748b;font-size:11px;">
                Powered by <a href="https://github.com/ShekharTarare/uPreviewShare" target="_blank" rel="noopener" style="color:#A78BFA;text-decoration:none;">uPreviewShare</a>
            </div>
        </div>
        """ + "<style>#ups-preview-bar ~ * { margin-bottom: 50px; } body { padding-bottom: 50px !important; }</style>";
    }
}

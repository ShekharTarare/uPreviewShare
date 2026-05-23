using System.Net;
using System.Text;
using uPreviewShare.ViewModels;

namespace uPreviewShare.Rendering;

/// <summary>
/// Static HTML renderer for uPreviewShare public pages.
/// Generates complete HTML strings for the Preview and PIN pages.
/// </summary>
public static class uPreviewShareHtmlRenderer
{
    public static string RenderPreviewPage(PreviewViewModel model)
    {
        var primaryColor = model.BrandingConfig?.PrimaryColor;
        if (string.IsNullOrEmpty(primaryColor)) primaryColor = "#8B5CF6";
        primaryColor = WebUtility.HtmlEncode(primaryColor);

        var backgroundColor = model.BrandingConfig?.BackgroundColor;
        if (string.IsNullOrEmpty(backgroundColor)) backgroundColor = "#f8fafc";
        backgroundColor = WebUtility.HtmlEncode(backgroundColor);

        var textColor = model.BrandingConfig?.TextColor;
        if (string.IsNullOrEmpty(textColor))
            textColor = GetContrastColor(model.BrandingConfig?.PrimaryColor ?? "#8B5CF6");
        textColor = WebUtility.HtmlEncode(textColor);
        var logoPath = model.BrandingConfig?.LogoPath;
        var nodeName = WebUtility.HtmlEncode(model.NodeName);

        var pageStyles = GetPreviewStyles();

        var bodyContent = new StringBuilder();
        bodyContent.Append("<div class=\"ups-preview-container\">");
        bodyContent.Append("<div class=\"ups-preview-infobar\">");
        bodyContent.Append("<div class=\"ups-preview-infobar__left\">");
        bodyContent.Append($"<h1 class=\"ups-preview-title\">{nodeName}</h1>");
        bodyContent.Append("</div>");
        bodyContent.Append("<div class=\"ups-preview-infobar__right\">");

        var badgeText = model.IsDraft ? "Draft Preview" : "Published";
        var badgeClass = model.IsDraft ? "ups-preview-badge" : "ups-preview-badge ups-preview-badge--published";
        bodyContent.Append($"<span class=\"{badgeClass}\">");
        if (model.IsDraft)
        {
            bodyContent.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">");
            bodyContent.Append("<path d=\"M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm-1 2l5 5h-5V4zM6 20V4h5v7h7v9H6z\"/>");
            bodyContent.Append("</svg>");
        }
        else
        {
            bodyContent.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">");
            bodyContent.Append("<path d=\"M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41L9 16.17z\"/>");
            bodyContent.Append("</svg>");
        }
        bodyContent.Append(badgeText);
        bodyContent.Append("</span>");
        bodyContent.Append("</div>");
        bodyContent.Append("</div>");
        bodyContent.Append("<div class=\"ups-preview-content\">");
        bodyContent.Append(model.Content);
        bodyContent.Append("</div>");
        bodyContent.Append("</div>");

        return RenderShell(primaryColor, backgroundColor, logoPath, textColor, pageStyles, bodyContent.ToString());
    }

    public static string RenderPinPage(PinViewModel model)
    {
        var primaryColor = model.BrandingConfig?.PrimaryColor;
        if (string.IsNullOrEmpty(primaryColor)) primaryColor = "#8B5CF6";
        primaryColor = WebUtility.HtmlEncode(primaryColor);

        var backgroundColor = model.BrandingConfig?.BackgroundColor;
        if (string.IsNullOrEmpty(backgroundColor)) backgroundColor = "#f8fafc";
        backgroundColor = WebUtility.HtmlEncode(backgroundColor);

        var textColor = model.BrandingConfig?.TextColor;
        if (string.IsNullOrEmpty(textColor))
            textColor = GetContrastColor(model.BrandingConfig?.PrimaryColor ?? "#8B5CF6");
        textColor = WebUtility.HtmlEncode(textColor);
        var logoPath = model.BrandingConfig?.LogoPath;

        var pageStyles = GetPinStyles();
        string bodyContent;

        if (model.IsLockedOut) bodyContent = RenderLockoutContent(model);
        else bodyContent = RenderPinFormContent(model);

        return RenderShell(primaryColor, backgroundColor, logoPath, textColor, pageStyles, bodyContent);
    }

    private static string RenderLockoutContent(PinViewModel model)
    {
        var minutes = model.RetryAfterSeconds / 60;
        var seconds = model.RetryAfterSeconds % 60;
        var timerDisplay = $"{minutes}:{seconds:D2}";

        var sb = new StringBuilder();
        sb.Append("<div class=\"ups-pin-card\">");
        sb.Append("<div class=\"ups-pin-card__icon\">");
        sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">");
        sb.Append("<path d=\"M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1s3.1 1.39 3.1 3.1v2z\"/>");
        sb.Append("</svg>");
        sb.Append("</div>");
        sb.Append("<div class=\"ups-pin-lockout\">");
        sb.Append("<div class=\"ups-pin-lockout__title\">Access Temporarily Locked</div>");
        sb.Append("<p>Too many failed PIN attempts. Please wait before trying again.</p>");
        sb.Append($"<div class=\"ups-pin-lockout__timer\" id=\"lockout-timer\">{timerDisplay}</div>");
        sb.Append("</div>");
        sb.Append("<script>");
        sb.Append("(function(){");
        sb.Append($"var remaining={model.RetryAfterSeconds};");
        sb.Append("var timerEl=document.getElementById('lockout-timer');");
        sb.Append("var interval=setInterval(function(){");
        sb.Append("remaining--;");
        sb.Append("if(remaining<=0){clearInterval(interval);window.location.reload();return;}");
        sb.Append("var m=Math.floor(remaining/60);");
        sb.Append("var s=remaining%60;");
        sb.Append("timerEl.textContent=m+':'+(s<10?'0':'')+s;");
        sb.Append("},1000);");
        sb.Append("})();");
        sb.Append("</script>");
        sb.Append("</div>");

        return sb.ToString();
    }

    private static string RenderPinFormContent(PinViewModel model)
    {
        var token = WebUtility.HtmlEncode(model.Token);

        var sb = new StringBuilder();
        sb.Append("<div class=\"ups-pin-card\">");
        sb.Append("<div class=\"ups-pin-card__icon\">");
        sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">");
        sb.Append("<path d=\"M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1s3.1 1.39 3.1 3.1v2z\"/>");
        sb.Append("</svg>");
        sb.Append("</div>");
        sb.Append("<h1 class=\"ups-pin-card__title\">Enter PIN</h1>");
        sb.Append("<p class=\"ups-pin-card__subtitle\">Enter the 6-digit PIN to access this draft.</p>");

        if (!string.IsNullOrEmpty(model.ErrorMessage))
        {
            var encodedError = WebUtility.HtmlEncode(model.ErrorMessage);
            sb.Append($"<div class=\"ups-pin-error\" role=\"alert\">{encodedError}</div>");
        }

        // Anti-forgery token is not used here because:
        // 1. The form is rendered as raw HTML outside Razor's anti-forgery infrastructure
        // 2. The PIN verification endpoint is rate-limited and doesn't modify authenticated user state
        // 3. The token parameter in the form provides request binding to a specific link
        sb.Append("<form method=\"post\" action=\"/upreviewshare/pin/verify\">");
        sb.Append($"<input type=\"hidden\" name=\"token\" value=\"{token}\" />");
        sb.Append("<label for=\"pin-input\" style=\"position:absolute;width:1px;height:1px;overflow:hidden;clip:rect(0,0,0,0);\">PIN</label>");
        sb.Append("<input type=\"text\" id=\"pin-input\" name=\"pin\" class=\"ups-pin-input\" maxlength=\"6\" pattern=\"[0-9]{6}\" title=\"Enter exactly 6 digits (0-9)\" inputmode=\"numeric\" autocomplete=\"one-time-code\" placeholder=\"&#x2022;&#x2022;&#x2022;&#x2022;&#x2022;&#x2022;\" required autofocus aria-label=\"6-digit PIN\" />");
        sb.Append("<button type=\"submit\" class=\"ups-pin-submit\">Verify PIN</button>");
        sb.Append("</form>");

        if (model.RemainingAttempts < 5 && model.RemainingAttempts > 0)
        {
            var attemptWord = model.RemainingAttempts == 1 ? "attempt" : "attempts";
            sb.Append($"<p class=\"ups-pin-attempts\">{model.RemainingAttempts} {attemptWord} remaining</p>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    private static string RenderShell(string primaryColor, string backgroundColor, string? logoPath, string textColor, string pageStyles, string bodyContent)
    {
        var headerContent = string.IsNullOrEmpty(logoPath)
            ? "<span class=\"ups-header__title\">uPreviewShare</span>"
            : $"<img src=\"/{WebUtility.HtmlEncode(logoPath)}\" alt=\"Logo\" class=\"ups-header__logo\" />";

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        sb.AppendLine("<meta name=\"robots\" content=\"noindex, nofollow\" />");
        sb.AppendLine("<title>uPreviewShare</title>");
        sb.AppendLine("<style>");
        var headerTextColor = IsColorTooLight(primaryColor) ? "#1e293b" : primaryColor;
        var footerLinkColor = IsColorTooLight(primaryColor) ? "#8B5CF6" : primaryColor;
        sb.AppendLine($":root {{ --ups-primary-color: {primaryColor}; --ups-background-color: {backgroundColor}; --ups-text-color: {textColor}; --ups-header-text-color: {headerTextColor}; --ups-footer-link-color: {footerLinkColor}; --ups-font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif; }}");
        sb.AppendLine(GetShellStyles());
        sb.AppendLine(pageStyles);
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<header class=\"ups-header\">{headerContent}</header>");
        sb.AppendLine($"<main class=\"ups-main\">{bodyContent}</main>");
        sb.AppendLine("<footer class=\"ups-footer\">Powered by <a href=\"https://github.com/ShekharTarare/uPreviewShare\" target=\"_blank\" rel=\"noopener noreferrer\">uPreviewShare</a></footer>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string GetShellStyles()
    {
        return """
            *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
            html, body { height: 100%; }
            body { font-family: var(--ups-font-family); background-color: #ffffff; color: #1e293b; line-height: 1.6; display: flex; flex-direction: column; min-height: 100vh; }
            .ups-header { background-color: #ffffff; border-bottom: 1px solid #e2e8f0; padding: 1rem 2rem; display: flex; align-items: center; justify-content: center; box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05); }
            .ups-header__logo { max-height: 48px; max-width: 200px; object-fit: contain; }
            .ups-header__title { font-size: 1.25rem; font-weight: 600; color: var(--ups-header-text-color); letter-spacing: -0.025em; }
            .ups-main { flex: 1; display: flex; align-items: flex-start; justify-content: center; padding: 0; }
            .ups-footer { background-color: #ffffff; border-top: 1px solid #e2e8f0; padding: 1rem 2rem; text-align: center; font-size: 0.8125rem; color: #64748b; }
            .ups-footer a { color: var(--ups-footer-link-color); text-decoration: none; }
            .ups-footer a:hover { text-decoration: underline; }
            @media (max-width: 640px) { .ups-header { padding: 0.75rem 1rem; } }
            """;
    }

    private static string GetPreviewStyles()
    {
        return """
            .ups-preview-container { width: 100%; max-width: 1200px; padding: 0 3rem; margin: 0 auto; }
            .ups-preview-infobar { display: flex; align-items: center; justify-content: space-between; padding: 1rem 1.5rem; margin: 1.5rem 0; background: #f1f5f9; border: 1px solid #e2e8f0; border-radius: 8px; flex-wrap: wrap; gap: 0.75rem; }
            .ups-preview-infobar__left { display: flex; align-items: center; gap: 0.75rem; }
            .ups-preview-infobar__right { display: flex; align-items: center; }
            .ups-preview-title { font-size: 1.125rem; font-weight: 600; color: #1e293b; margin: 0; line-height: 1.3; }
            .ups-preview-badge { display: inline-flex; align-items: center; gap: 0.375rem; background-color: var(--ups-primary-color); color: var(--ups-text-color); font-size: 0.6875rem; font-weight: 600; padding: 0.3rem 0.625rem; border-radius: 9999px; text-transform: uppercase; letter-spacing: 0.025em; white-space: nowrap; border: 1px solid rgba(0,0,0,0.1); }
            .ups-preview-badge--published { background-color: #dcfce7; color: #166534; border-color: #bbf7d0; }
            .ups-preview-badge svg { width: 12px; height: 12px; fill: currentColor; }
            .ups-preview-content { line-height: 1.8; color: #334155; word-wrap: break-word; overflow-wrap: break-word; font-size: 1.0625rem; padding: 1.5rem 0; }
            .ups-preview-content h1, .ups-preview-content h2, .ups-preview-content h3, .ups-preview-content h4, .ups-preview-content h5, .ups-preview-content h6 { color: #1e293b; margin-top: 1.5rem; margin-bottom: 0.75rem; line-height: 1.3; }
            .ups-preview-content h1 { font-size: 2rem; }
            .ups-preview-content h2 { font-size: 1.5rem; }
            .ups-preview-content h3 { font-size: 1.25rem; }
            .ups-preview-content p { margin-bottom: 1rem; }
            .ups-preview-content img { max-width: 100%; height: auto; border-radius: 8px; }
            .ups-preview-content a { color: var(--ups-primary-color); text-decoration: underline; }
            .ups-preview-content ul, .ups-preview-content ol { margin-bottom: 1rem; padding-left: 1.5rem; }
            .ups-preview-content blockquote { border-left: 4px solid var(--ups-primary-color); padding-left: 1rem; margin: 1rem 0; color: #64748b; font-style: italic; }
            .ups-preview-content table { width: 100%; border-collapse: collapse; margin-bottom: 1rem; }
            .ups-preview-content th, .ups-preview-content td { border: 1px solid #e2e8f0; padding: 0.5rem 0.75rem; text-align: left; }
            .ups-preview-content th { background-color: #f8fafc; font-weight: 600; }
            .ups-preview-content pre { background-color: #f1f5f9; border-radius: 8px; padding: 1rem; overflow-x: auto; margin-bottom: 1rem; }
            .ups-preview-content code { font-family: 'SF Mono', 'Fira Code', 'Fira Mono', Menlo, monospace; font-size: 0.875em; }
            @media (max-width: 768px) { .ups-preview-container { padding: 0 1.25rem; } .ups-preview-infobar { flex-direction: column; align-items: flex-start; padding: 0.875rem 1rem; margin: 1rem 0; } .ups-preview-title { font-size: 1rem; } .ups-preview-content { font-size: 1rem; padding: 1rem 0; } }
            """;
    }

    private static string GetPinStyles()
    {
        return """
            .ups-main { align-items: center; justify-content: center; padding: 2rem 1rem; background-color: var(--ups-background-color); }
            .ups-pin-card { background: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1); padding: 2.5rem 2rem; width: 100%; max-width: 400px; text-align: center; }
            .ups-pin-card__icon { width: 56px; height: 56px; background-color: var(--ups-primary-color); border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 1.5rem; border: 1px solid rgba(0,0,0,0.08); }
            .ups-pin-card__icon svg { width: 28px; height: 28px; fill: var(--ups-text-color); }
            .ups-pin-card__title { font-size: 1.5rem; font-weight: 700; color: #1e293b; margin-bottom: 0.5rem; }
            .ups-pin-card__subtitle { font-size: 0.9375rem; color: #64748b; margin-bottom: 2rem; }
            .ups-pin-input { width: 100%; font-size: 2rem; font-weight: 600; text-align: center; letter-spacing: 0.75rem; padding: 0.75rem 1rem; border: 2px solid #e2e8f0; border-radius: 8px; outline: none; transition: border-color 0.2s ease; -moz-appearance: textfield; }
            .ups-pin-input:focus { border-color: var(--ups-primary-color); box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.1); }
            .ups-pin-input::-webkit-outer-spin-button, .ups-pin-input::-webkit-inner-spin-button { -webkit-appearance: none; margin: 0; }
            .ups-pin-submit { width: 100%; padding: 0.875rem 1.5rem; margin-top: 1.5rem; background-color: var(--ups-primary-color); color: var(--ups-text-color); font-size: 1rem; font-weight: 600; border: none; border-radius: 8px; cursor: pointer; transition: opacity 0.2s ease, transform 0.1s ease; }
            .ups-pin-submit:hover { opacity: 0.9; }
            .ups-pin-submit:active { transform: scale(0.98); }
            .ups-pin-submit:disabled { opacity: 0.5; cursor: not-allowed; }
            .ups-pin-error { background-color: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; padding: 0.75rem 1rem; margin-bottom: 1.5rem; color: #dc2626; font-size: 0.875rem; font-weight: 500; }
            .ups-pin-attempts { margin-top: 1rem; font-size: 0.8125rem; color: #64748b; }
            .ups-pin-lockout { background-color: #fffbeb; border: 1px solid #fde68a; border-radius: 8px; padding: 1.5rem; color: #92400e; font-size: 0.9375rem; }
            .ups-pin-lockout__title { font-weight: 700; font-size: 1.125rem; margin-bottom: 0.5rem; color: #78350f; }
            .ups-pin-lockout__timer { font-size: 1.5rem; font-weight: 700; color: #b45309; margin-top: 0.75rem; }
            """;
    }

    private static string GetContrastColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor) || hexColor.Length < 7) return "#1e293b";
        try
        {
            var r = Convert.ToInt32(hexColor.Substring(1, 2), 16) / 255.0;
            var g = Convert.ToInt32(hexColor.Substring(3, 2), 16) / 255.0;
            var b = Convert.ToInt32(hexColor.Substring(5, 2), 16) / 255.0;
            var luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
            return luminance > 0.5 ? "#1e293b" : "#ffffff";
        }
        catch { return "#1e293b"; }
    }

    private static bool IsColorTooLight(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor) || hexColor.Length < 7) return false;
        try
        {
            var r = Convert.ToInt32(hexColor.Substring(1, 2), 16) / 255.0;
            var g = Convert.ToInt32(hexColor.Substring(3, 2), 16) / 255.0;
            var b = Convert.ToInt32(hexColor.Substring(5, 2), 16) / 255.0;
            return (0.2126 * r + 0.7152 * g + 0.0722 * b) > 0.85;
        }
        catch { return false; }
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using uPreviewShare.Filters;
using uPreviewShare.Models;
using uPreviewShare.Rendering;
using uPreviewShare.Services;
using uPreviewShare.ViewModels;

namespace uPreviewShare.Controllers;

[Route("upreviewshare")]
[ServiceFilter(typeof(uPreviewShareExceptionFilter))]
[ServiceFilter(typeof(uPreviewSharePreviewBarFilter))]
public class uPreviewSharePublicController : Controller
{
    private readonly ITokenLinkService _tokenLinkService;
    private readonly IAuditLogService _auditLogService;
    private readonly IRateLimitService _rateLimitService;
    private readonly IBrandingService _brandingService;
    private readonly IDataProtector _dataProtector;
    private readonly ILogger<uPreviewSharePublicController> _logger;
    private readonly Umbraco.Cms.Core.Services.IContentService _contentService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;

    private const string SessionCookieName = "uPreviewShare.Session";
    private const string CookiePurpose = "uPreviewShare.SessionCookie.v1";
    private readonly TimeSpan _sessionDuration;

    public uPreviewSharePublicController(
        ITokenLinkService tokenLinkService,
        IAuditLogService auditLogService,
        IRateLimitService rateLimitService,
        IBrandingService brandingService,
        IDataProtectionProvider dataProtectionProvider,
        Umbraco.Cms.Core.Services.IContentService contentService,
        IHostEnvironment hostEnvironment,
        IUmbracoContextAccessor umbracoContextAccessor,
        IVariationContextAccessor variationContextAccessor,
        IOptions<uPreviewShareOptions> options,
        ILogger<uPreviewSharePublicController> logger)
    {
        _tokenLinkService = tokenLinkService;
        _auditLogService = auditLogService;
        _rateLimitService = rateLimitService;
        _brandingService = brandingService;
        _dataProtector = dataProtectionProvider.CreateProtector(CookiePurpose);
        _contentService = contentService;
        _hostEnvironment = hostEnvironment;
        _umbracoContextAccessor = umbracoContextAccessor;
        _variationContextAccessor = variationContextAccessor;
        _sessionDuration = TimeSpan.FromMinutes(options.Value.SessionDurationMinutes);
        _logger = logger;
    }

    [HttpGet("preview")]
    public async Task<IActionResult> Preview([FromQuery] string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return NotFound();
        var validationResult = await _tokenLinkService.ValidateTokenAsync(token, ct);
        if (!validationResult.IsValid) return NotFound();
        if (validationResult.HasPin && !HasValidSessionCookie(validationResult.LinkId!.Value))
            return RedirectToAction(nameof(Pin), new { token });

        var newViewCount = await _tokenLinkService.IncrementViewCountAtomicallyAsync(validationResult.LinkId!.Value, ct);
        if (validationResult.MaxViews.HasValue && newViewCount >= validationResult.MaxViews.Value)
            _logger.LogInformation("Link {LinkId} has reached max views ({MaxViews}). This is the final access.", validationResult.LinkId, validationResult.MaxViews);

        try
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();
            await _auditLogService.LogAccessAsync(validationResult.LinkId!.Value, ipAddress, userAgent, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist audit log for link {LinkId}", validationResult.LinkId);
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        // Try to render through Umbraco's template engine (draft content via preview mode)
        var content = _contentService.GetById(validationResult.NodeId!.Value);
        if (content != null && _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
        {
            // Set the variation context for culture-variant content
            var culture = validationResult.Culture;
            if (!string.IsNullOrEmpty(culture))
            {
                _variationContextAccessor.VariationContext = new VariationContext(culture);
            }

            var draftContent = umbracoContext.Content?.GetById(true, content.Key);
            if (draftContent != null && draftContent.TemplateId > 0)
            {
                var templateAlias = draftContent.GetTemplateAlias();
                if (!string.IsNullOrEmpty(templateAlias))
                {
                    HttpContext.Items["uPreviewShare.IsPreview"] = true;
                    HttpContext.Items["uPreviewShare.IsDraft"] = !content.Published || content.Edited;
                    HttpContext.Items["uPreviewShare.Culture"] = culture;
                    return View(templateAlias, new ContentModel(draftContent));
                }
            }
        }

        // Fallback: render using the built-in property renderer if no template is available
        var brandingConfig = await _brandingService.GetBrandingAsync(ct);
        var nodeName = content?.Name ?? "Untitled";
        var contentHtml = RenderNodeContent(content, validationResult.Culture);
        var isDraft = content != null && (!content.Published || content.Edited);

        var viewModel = new PreviewViewModel
        {
            NodeId = validationResult.NodeId!.Value,
            NodeName = nodeName,
            Content = contentHtml,
            BrandingConfig = brandingConfig,
            IsDraft = isDraft
        };

        var html = uPreviewShareHtmlRenderer.RenderPreviewPage(viewModel);
        Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'unsafe-inline'";
        return Content(html, "text/html");
    }

    [HttpGet("pin")]
    public async Task<IActionResult> Pin([FromQuery] string? token, [FromQuery] string? error, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return NotFound();
        var validationResult = await _tokenLinkService.ValidateTokenAsync(token, ct);
        if (!validationResult.IsValid) return NotFound();

        var ipAddress = GetClientIpAddress();
        if (_rateLimitService.IsLockedOut(ipAddress, validationResult.LinkId!.Value))
        {
            var lockoutRemaining = _rateLimitService.GetLockoutRemaining(ipAddress, validationResult.LinkId.Value);
            var retryAfterSeconds = lockoutRemaining.HasValue ? (int)Math.Ceiling(lockoutRemaining.Value.TotalSeconds) : 900;
            Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
            var brandingConfigLockout = await _brandingService.GetBrandingAsync(ct);
            var lockoutViewModel = new PinViewModel
            {
                Token = token, IsLockedOut = true, RetryAfterSeconds = retryAfterSeconds, RemainingAttempts = 0,
                ErrorMessage = $"Too many failed attempts. Please try again after {retryAfterSeconds / 60} minutes.",
                BrandingConfig = brandingConfigLockout
            };
            Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'unsafe-inline'";
            return Content(uPreviewShareHtmlRenderer.RenderPinPage(lockoutViewModel), "text/html");
        }

        var brandingConfig = await _brandingService.GetBrandingAsync(ct);
        var remainingAttempts = _rateLimitService.GetRemainingAttempts(ipAddress, validationResult.LinkId.Value);
        var viewModel = new PinViewModel
        {
            Token = token, RemainingAttempts = remainingAttempts, BrandingConfig = brandingConfig,
            ErrorMessage = error == "incorrect" ? "The PIN you entered is incorrect." : null
        };
        Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'unsafe-inline'";
        return Content(uPreviewShareHtmlRenderer.RenderPinPage(viewModel), "text/html");
    }

    [HttpPost("pin/verify")]
    public async Task<IActionResult> VerifyPin([FromForm] string? token, [FromForm] string? pin, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(pin)) return NotFound();
        var validationResult = await _tokenLinkService.ValidateTokenAsync(token, ct);
        if (!validationResult.IsValid) return NotFound();

        var ipAddress = GetClientIpAddress();
        var linkId = validationResult.LinkId!.Value;
        if (_rateLimitService.IsLockedOut(ipAddress, linkId)) return RedirectToAction(nameof(Pin), new { token });

        var isPinCorrect = TokenLinkService.VerifyPin(pin, validationResult.PinHash!);
        if (isPinCorrect)
        {
            _rateLimitService.ResetAttempts(ipAddress, linkId);
            SetSessionCookie(linkId);
            return RedirectToAction(nameof(Preview), new { token });
        }
        else
        {
            var remainingAttempts = _rateLimitService.RecordFailedAttempt(ipAddress, linkId);
            try { await _auditLogService.LogFailedPinAsync(linkId, ipAddress, GetUserAgent(), ct); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to persist failed PIN audit log for link {LinkId}", linkId); return StatusCode(StatusCodes.Status503ServiceUnavailable); }

            if (_rateLimitService.IsLockedOut(ipAddress, linkId))
            {
                try { await _auditLogService.LogLockoutAsync(linkId, ipAddress, GetUserAgent(), ct); }
                catch (Exception ex2) { _logger.LogError(ex2, "Failed to persist lockout audit log for link {LinkId}", linkId); }
                return RedirectToAction(nameof(Pin), new { token });
            }
            return RedirectToAction(nameof(Pin), new { token, error = "incorrect" });
        }
    }

    [HttpGet("logo/{filename}")]
    public IActionResult Logo(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename)) return NotFound();
        var sanitized = Path.GetFileName(filename);
        if (string.IsNullOrEmpty(sanitized) || sanitized != filename) return NotFound();
        var logoPath = Path.Combine(_hostEnvironment.ContentRootPath, "App_Data", "uPreviewShare", "logos", sanitized);
        if (!System.IO.File.Exists(logoPath)) return NotFound();
        var ext = Path.GetExtension(sanitized).ToLowerInvariant();
        var contentType = ext switch { ".png" => "image/png", ".svg" => "image/svg+xml", _ => "application/octet-stream" };
        Response.Headers["Cache-Control"] = "public, max-age=86400";
        if (ext == ".svg")
        {
            Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'unsafe-inline'";
        }
        return PhysicalFile(logoPath, contentType);
    }

    #region Private Helpers

    private static string RenderNodeContent(Umbraco.Cms.Core.Models.IContent? content, string? culture = null)
    {
        if (content == null) return "<p><em>Content not found.</em></p>";
        var sb = new System.Text.StringBuilder();
        var name = System.Net.WebUtility.HtmlEncode(content.Name ?? "Untitled");
        sb.Append($"<h1 style=\"font-size: 2rem; font-weight: 700; color: #1e293b; margin-bottom: 1.5rem;\">{name}</h1>");
        var hasProperties = false;
        foreach (var property in content.Properties)
        {
            var value = property.GetValue(culture);
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) continue;
            hasProperties = true;
            var label = System.Net.WebUtility.HtmlEncode(property.Alias);
            var displayValue = value.ToString()!;
            var renderedValue = System.Net.WebUtility.HtmlEncode(displayValue);
            sb.Append($"<div style=\"margin-bottom: 1.5rem;\">");
            sb.Append($"<div style=\"font-weight: 600; color: #64748b; font-size: 0.85rem; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.25rem;\">{label}</div>");
            sb.Append($"<div style=\"font-size: 1rem;\">{renderedValue}</div>");
            sb.Append("</div>");
        }
        if (!hasProperties) sb.Append("<p><em>This content node has no properties with values.</em></p>");
        return sb.ToString();
    }

    private bool HasValidSessionCookie(Guid linkId)
    {
        if (!Request.Cookies.TryGetValue(SessionCookieName, out var cookieValue) || string.IsNullOrEmpty(cookieValue)) return false;
        try
        {
            var decrypted = _dataProtector.Unprotect(cookieValue);
            var sessionData = JsonSerializer.Deserialize<SessionCookieData>(decrypted);
            if (sessionData == null) return false;
            return sessionData.LinkId == linkId && sessionData.ExpiresAt > DateTime.UtcNow;
        }
        catch (Exception ex) { _logger.LogDebug(ex, "Failed to decrypt/validate session cookie"); return false; }
    }

    private void SetSessionCookie(Guid linkId)
    {
        var sessionData = new SessionCookieData { LinkId = linkId, ExpiresAt = DateTime.UtcNow.Add(_sessionDuration) };
        var json = JsonSerializer.Serialize(sessionData);
        var encrypted = _dataProtector.Protect(json);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict,
            Path = "/upreviewshare/", Expires = DateTimeOffset.UtcNow.Add(_sessionDuration), IsEssential = true
        };
        Response.Cookies.Append(SessionCookieName, encrypted, cookieOptions);
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP (original client) from the comma-separated list
            var ip = forwardedFor.Split(',', StringSplitOptions.TrimEntries).FirstOrDefault();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        return userAgent.Length > 512 ? userAgent[..512] : userAgent;
    }

    #endregion

    private sealed class SessionCookieData
    {
        public Guid LinkId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}

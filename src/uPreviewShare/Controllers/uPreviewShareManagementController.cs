using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using uPreviewShare.Models.Api;
using uPreviewShare.Models.DTOs;
using uPreviewShare.Services;

namespace uPreviewShare.Controllers;

[VersionedApiBackOfficeRoute("upreviewshare")]
[ApiExplorerSettings(GroupName = "uPreviewShare")]
[Authorize(Policy = "BackOfficeAccess")]
public class uPreviewShareManagementController : ManagementApiControllerBase
{
    private readonly ITokenLinkService _tokenLinkService;
    private readonly IAuditLogService _auditLogService;
    private readonly IBrandingService _brandingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentService _contentService;

    public uPreviewShareManagementController(
        ITokenLinkService tokenLinkService, IAuditLogService auditLogService, IBrandingService brandingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IContentService contentService)
    {
        _tokenLinkService = tokenLinkService;
        _auditLogService = auditLogService;
        _brandingService = brandingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentService = contentService;
    }

    [HttpPost("links")]
    [ProducesResponseType(typeof(uPreviewShareLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLink([FromBody] CreateLinkApiRequest request, CancellationToken ct)
    {
        var content = _contentService.GetById(request.NodeKey);
        if (content == null) return NotFound("Content not found");
        var nodeId = content.Id;
        var currentUserKey = GetCurrentUserKey();
        var createRequest = new CreateLinkRequest { NodeId = nodeId, CreatedBy = currentUserKey, ExpiresAt = request.ExpiresAt, MaxViews = request.MaxViews, Pin = request.Pin };
        try
        {
            var link = await _tokenLinkService.CreateLinkAsync(createRequest, ct);
            var dto = new uPreviewShareLinkDto
            {
                Id = link.Id, NodeId = link.NodeId, Token = link.Token, CreatedBy = link.CreatedBy, CreatedAt = link.CreatedAt,
                ExpiresAt = link.ExpiresAt, MaxViews = link.MaxViews, ViewCount = link.ViewCount,
                HasPin = !string.IsNullOrEmpty(link.PinHash), Status = (Models.Enums.LinkStatus)link.Status,
                RevokedAt = link.RevokedAt, RevokedBy = link.RevokedBy
            };
            return StatusCode(StatusCodes.Status201Created, dto);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("links/{nodeKey:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<uPreviewShareLinkDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLinksForNode(Guid nodeKey, CancellationToken ct)
    {
        var content = _contentService.GetById(nodeKey);
        if (content == null) return NotFound("Content not found");
        var links = await _tokenLinkService.GetLinksForNodeAsync(content.Id, ct);
        return Ok(links);
    }

    [HttpDelete("links/{linkId:guid}")]
    [ProducesResponseType(typeof(RevocationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeLink(Guid linkId, CancellationToken ct)
    {
        var currentUserKey = GetCurrentUserKey();
        try
        {
            await _tokenLinkService.RevokeLinkAsync(linkId, currentUserKey, ct);
            await _auditLogService.LogRevocationAsync(linkId, 0, currentUserKey, ct);
        }
        catch (InvalidOperationException) { return NotFound("Link not found"); }
        return Ok(new RevocationResponse { Count = 1, Message = "Link revoked successfully." });
    }

    [HttpDelete("links/{linkId:guid}/permanent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteLink(Guid linkId, CancellationToken ct)
    {
        await _tokenLinkService.DeleteLinkAsync(linkId, ct);
        return Ok(new { message = "Link permanently deleted." });
    }

    [HttpDelete("links/node/{nodeKey:guid}")]
    [ProducesResponseType(typeof(RevocationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeAllLinksForNode(Guid nodeKey, CancellationToken ct)
    {
        var content = _contentService.GetById(nodeKey);
        if (content == null) return NotFound("Content not found");
        var currentUserKey = GetCurrentUserKey();
        var links = await _tokenLinkService.GetLinksForNodeAsync(content.Id, ct);
        var activeLinks = links.Where(l => l.Status == Models.Enums.LinkStatus.Active).ToList();
        var revokedCount = await _tokenLinkService.RevokeAllLinksForNodeAsync(content.Id, currentUserKey, ct);
        foreach (var link in activeLinks) await _auditLogService.LogRevocationAsync(link.Id, content.Id, currentUserKey, ct);
        return Ok(new RevocationResponse { Count = revokedCount, Message = revokedCount == 1 ? "1 link revoked successfully." : $"{revokedCount} links revoked successfully." });
    }

    [HttpGet("audit/{nodeKey:guid}")]
    [ProducesResponseType(typeof(PagedResult<AuditLogEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLog(Guid nodeKey, [FromQuery] int page = 1, [FromQuery] string? eventType = null, [FromQuery] Guid? linkId = null, CancellationToken ct = default)
    {
        var content = _contentService.GetById(nodeKey);
        if (content == null) return NotFound("Content not found");
        var result = await _auditLogService.GetLogsForNodeAsync(content.Id, page, 50, eventType, linkId, ct);
        return Ok(result);
    }

    [HttpGet("audit/{nodeKey:guid}/links")]
    [ProducesResponseType(typeof(IReadOnlyList<uPreviewShareLinkDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLinksForAudit(Guid nodeKey, CancellationToken ct)
    {
        var content = _contentService.GetById(nodeKey);
        if (content == null) return NotFound("Content not found");
        var links = await _tokenLinkService.GetAllLinksForNodeAsync(content.Id, ct);
        return Ok(links);
    }

    [HttpGet("branding")]
    [ProducesResponseType(typeof(BrandingConfigDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranding(CancellationToken ct)
    {
        var branding = await _brandingService.GetBrandingAsync(ct);
        return Ok(branding);
    }

    [HttpPut("branding")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBranding([FromBody] UpdateBrandingRequest request, CancellationToken ct)
    {
        try { await _brandingService.SaveBrandingAsync(request.PrimaryColor, request.BackgroundColor, request.TextColor, ct); return Ok(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("branding")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetBranding(CancellationToken ct)
    {
        await _brandingService.ResetBrandingAsync(ct);
        return Ok();
    }

    [HttpPost("branding/logo")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest("No file provided.");
        try
        {
            using var stream = file.OpenReadStream();
            var logoPath = await _brandingService.SaveLogoAsync(stream, file.FileName, file.Length, ct);
            return Ok(new { logoPath });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    private Guid GetCurrentUserKey() => _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key ?? Guid.Empty;
}

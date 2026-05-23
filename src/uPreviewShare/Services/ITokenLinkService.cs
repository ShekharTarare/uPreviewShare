using uPreviewShare.Models;
using uPreviewShare.Models.DTOs;

namespace uPreviewShare.Services;

/// <summary>
/// Core service for managing preview share links.
/// Handles token generation, validation, view counting, and revocation.
/// </summary>
public interface ITokenLinkService
{
    Task<uPreviewShareLink> CreateLinkAsync(CreateLinkRequest request, CancellationToken ct = default);
    Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken ct = default);
    Task<int> IncrementViewCountAtomicallyAsync(Guid linkId, CancellationToken ct = default);
    Task RevokeLinkAsync(Guid linkId, Guid revokedBy, CancellationToken ct = default);
    Task<int> RevokeAllLinksForNodeAsync(int nodeId, Guid revokedBy, CancellationToken ct = default);
    Task<IReadOnlyList<uPreviewShareLinkDto>> GetLinksForNodeAsync(int nodeId, CancellationToken ct = default);
    Task<IReadOnlyList<uPreviewShareLinkDto>> GetAllLinksForNodeAsync(int nodeId, CancellationToken ct = default);
    Task DeleteLinkAsync(Guid linkId, CancellationToken ct = default);
}

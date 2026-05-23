namespace uPreviewShare.Models.Api;

/// <summary>
/// API response model for link revocation operations.
/// </summary>
public class RevocationResponse
{
    /// <summary>
    /// The number of links that were revoked.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// A human-readable confirmation message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

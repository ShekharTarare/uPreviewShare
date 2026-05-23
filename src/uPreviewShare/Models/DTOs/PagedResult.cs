namespace uPreviewShare.Models.DTOs;

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public long TotalItems { get; set; }

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
}

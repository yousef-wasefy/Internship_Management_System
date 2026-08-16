namespace InternshipManagement.Api.DTOs.Common;

// Generic wrapper for any paginated list response - first used by
// GET /api/internships (Phase 12), reusable by any future paginated endpoint.
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.Entities;

public class InternshipPost
{
    public int Id { get; set; }

    public int CompanyId { get; set; }
    public CompanyProfile Company { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public WorkMode WorkMode { get; set; }

    // Free text (e.g. "3 months") rather than structured start/end dates - simplest
    // option for v1, see DATABASE_DESIGN.md §3.4.
    public string? Duration { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    // Every post starts as a Draft (Phase 8 publishing workflow).
    public InternshipStatus Status { get; set; } = InternshipStatus.Draft;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<InternshipApplication> Applications { get; set; } = new List<InternshipApplication>();
}

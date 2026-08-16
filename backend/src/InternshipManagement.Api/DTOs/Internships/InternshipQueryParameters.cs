using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Internships;

// Query string parameters for GET /api/internships - all optional. No "status" filter
// here on purpose: the public listing only ever shows Open posts (Phase 8's rule), so a
// client-supplied status filter would either be a no-op or, worse, imply it could be
// used to see other statuses, which it can't. A status filter on the company's own
// listing (GET /api/companies/me/internships) makes sense instead, since that endpoint
// already shows every status to its owner.
public class InternshipQueryParameters
{
    public string? Location { get; set; }
    public WorkMode? WorkMode { get; set; }

    // Matches against Title only, case-insensitive, partial match.
    public string? Search { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

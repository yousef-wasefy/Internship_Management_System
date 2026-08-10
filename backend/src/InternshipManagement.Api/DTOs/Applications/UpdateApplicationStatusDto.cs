using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Applications;

// Shape of the JSON body for PATCH /api/applications/{id}/status. Status must be
// Shortlisted, Accepted, or Rejected - a company cannot set Pending (the default) or
// Withdrawn (a student-only action) through this endpoint. CompanyNotes is optional and
// only overwrites the existing note when provided (see ApplicationService.UpdateStatusAsync).
public class UpdateApplicationStatusDto
{
    public ApplicationStatus Status { get; set; }
    public string? CompanyNotes { get; set; }
}

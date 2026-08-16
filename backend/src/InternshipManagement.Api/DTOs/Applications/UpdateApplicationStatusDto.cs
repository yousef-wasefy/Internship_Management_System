using System.ComponentModel.DataAnnotations;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Applications;

// Shape of the JSON body for PATCH /api/applications/{id}/status. Status must be
// Shortlisted, Accepted, or Rejected - a company cannot set Pending (the default) or
// Withdrawn (a student-only action) through this endpoint. That specific 3-value
// restriction is still a business rule enforced in ApplicationService.UpdateStatusAsync
// (Phase 10), not a DTO-level [Required]/enum-range check - [EnumDataType] would only
// confirm the value is *some* defined ApplicationStatus, not one of these three.
// CompanyNotes is optional and only overwrites the existing note when provided.
public class UpdateApplicationStatusDto
{
    public ApplicationStatus Status { get; set; }

    [StringLength(2000)]
    public string? CompanyNotes { get; set; }
}

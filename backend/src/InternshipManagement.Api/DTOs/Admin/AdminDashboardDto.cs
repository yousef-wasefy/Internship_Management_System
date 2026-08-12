namespace InternshipManagement.Api.DTOs.Admin;

// Shape returned by GET /api/admin/dashboard. Covers the 6 stats named in the phase
// plan, plus AcceptedApplications/RejectedApplications from the module's own suggested
// statistics (docs' original brief §6.6) - cheap to add and directly useful.
public class AdminDashboardDto
{
    public int TotalStudents { get; set; }
    public int TotalCompanies { get; set; }
    public int PendingCompanies { get; set; }
    public int TotalInternships { get; set; }
    public int OpenInternships { get; set; }
    public int TotalApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
}

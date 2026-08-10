using InternshipManagement.Api.Data;
using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InternshipManagement.Api.Services.Implementations;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _context;

    public ApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicationDto>> GetMyApplicationsAsync(int userId)
    {
        var applications = await _context.InternshipApplications
            .Include(a => a.InternshipPost).ThenInclude(p => p.Company)
            .Where(a => a.Student.UserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return applications.Select(ToDto).ToList();
    }

    public async Task<(OperationResult Result, string? ErrorMessage, ApplicationDto? Application)> ApplyAsync(
        int internshipPostId, int userId, ApplyToInternshipDto dto)
    {
        var internship = await _context.InternshipPosts
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == internshipPostId);

        if (internship is null)
        {
            return (OperationResult.NotFound, null, null);
        }

        // A student can only apply to a currently Open internship (REQUIREMENTS.md §4.1
        // rule 2) - Draft/Closed/Cancelled all fail here with the same message; which
        // one it actually is isn't the student's business.
        if (internship.Status != InternshipStatus.Open)
        {
            return (OperationResult.ValidationFailed, "This internship is not open for applications.", null);
        }

        // Deadline can pass without a company ever closing the post (no background job
        // flips Status automatically), so this is checked separately from Status itself
        // (REQUIREMENTS.md §4.1 rule 3).
        if (internship.ApplicationDeadline <= DateTime.UtcNow)
        {
            return (OperationResult.ValidationFailed, "The application deadline for this internship has passed.", null);
        }

        // Every Student-role user has exactly one StudentProfile, created atomically at
        // registration (Phase 6).
        var student = await _context.StudentProfiles.FirstAsync(s => s.UserId == userId);

        // Friendly pre-check for the common case - the composite unique index on
        // (StudentId, InternshipPostId) from Phase 4 is the real, final enforcement of
        // "no duplicate applications" (REQUIREMENTS.md §4.1 rule 4).
        var alreadyApplied = await _context.InternshipApplications
            .AnyAsync(a => a.StudentId == student.Id && a.InternshipPostId == internshipPostId);
        if (alreadyApplied)
        {
            return (OperationResult.ValidationFailed, "You have already applied to this internship.", null);
        }

        var now = DateTime.UtcNow;
        var application = new InternshipApplication
        {
            Student = student,
            InternshipPost = internship,
            CoverLetter = dto.CoverLetter,
            CVUrl = dto.CVUrl,
            Status = ApplicationStatus.Pending,
            AppliedAt = now,
            UpdatedAt = now
        };

        _context.InternshipApplications.Add(application);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Defense in depth: two near-simultaneous requests both passed the
            // AnyAsync check above before either had committed. The database's own
            // unique constraint is what actually stops the duplicate in that race -
            // this just turns the resulting raw DB error into the same friendly message.
            return (OperationResult.ValidationFailed, "You have already applied to this internship.", null);
        }

        return (OperationResult.Success, null, ToDto(application));
    }

    public async Task<(OperationResult Result, string? ErrorMessage, ApplicationDto? Application)> WithdrawAsync(
        int applicationId, int userId)
    {
        var application = await _context.InternshipApplications
            .Include(a => a.Student)
            .Include(a => a.InternshipPost).ThenInclude(p => p.Company)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application is null)
        {
            return (OperationResult.NotFound, null, null);
        }

        if (application.Student.UserId != userId)
        {
            return (OperationResult.Forbidden, null, null); // REQUIREMENTS.md STU-8: only your own
        }

        if (application.Status != ApplicationStatus.Pending)
        {
            return (OperationResult.ValidationFailed, "Only a pending application can be withdrawn.", null);
        }

        application.Status = ApplicationStatus.Withdrawn;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (OperationResult.Success, null, ToDto(application));
    }

    public async Task<List<ApplicantDto>> GetApplicantsForCompanyAsync(int userId)
    {
        var applications = await _context.InternshipApplications
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.InternshipPost)
            .Where(a => a.InternshipPost.Company.UserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return applications.Select(ToApplicantDto).ToList();
    }

    public async Task<(OperationResult Result, string? ErrorMessage, List<ApplicantDto>? Applicants)> GetApplicantsForInternshipAsync(
        int internshipPostId, int userId)
    {
        var internship = await _context.InternshipPosts.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == internshipPostId);
        if (internship is null)
        {
            return (OperationResult.NotFound, null, null);
        }

        if (internship.Company.UserId != userId)
        {
            return (OperationResult.Forbidden, null, null); // REQUIREMENTS.md CO-7: only for own internships
        }

        var applications = await _context.InternshipApplications
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.InternshipPost)
            .Where(a => a.InternshipPostId == internshipPostId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return (OperationResult.Success, null, applications.Select(ToApplicantDto).ToList());
    }

    public async Task<(OperationResult Result, string? ErrorMessage, ApplicantDto? Applicant)> UpdateStatusAsync(
        int applicationId, int userId, UpdateApplicationStatusDto dto)
    {
        var application = await _context.InternshipApplications
            .Include(a => a.Student).ThenInclude(s => s.User)
            .Include(a => a.InternshipPost).ThenInclude(p => p.Company)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application is null)
        {
            return (OperationResult.NotFound, null, null);
        }

        if (application.InternshipPost.Company.UserId != userId)
        {
            return (OperationResult.Forbidden, null, null); // REQUIREMENTS.md CO-8: only for own internships
        }

        // Checked before the requested status value itself - a withdrawn application is
        // completely off-limits regardless of what the company tried to set it to
        // (REQUIREMENTS.md §4.2 rule 5).
        if (application.Status == ApplicationStatus.Withdrawn)
        {
            return (OperationResult.ValidationFailed, "A withdrawn application cannot be reviewed.", null);
        }

        // A company may only move an application to one of these three statuses - not
        // back to Pending, and not to Withdrawn (that's a student-only action, Phase 9).
        if (dto.Status is not (ApplicationStatus.Shortlisted or ApplicationStatus.Accepted or ApplicationStatus.Rejected))
        {
            return (OperationResult.ValidationFailed, "Status must be Shortlisted, Accepted, or Rejected.", null);
        }

        application.Status = dto.Status;

        // Only overwrite existing notes if new ones were actually provided - a company
        // accepting an application shouldn't accidentally erase a note it left when
        // shortlisting it earlier.
        if (dto.CompanyNotes is not null)
        {
            application.CompanyNotes = dto.CompanyNotes;
        }

        application.ReviewedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (OperationResult.Success, null, ToApplicantDto(application));
    }

    private static ApplicantDto ToApplicantDto(InternshipApplication application) => new()
    {
        Id = application.Id,
        InternshipPostId = application.InternshipPostId,
        InternshipTitle = application.InternshipPost.Title,
        StudentFullName = application.Student.FullName,
        StudentEmail = application.Student.User.Email,
        StudentUniversity = application.Student.University,
        StudentMajor = application.Student.Major,
        StudentSkills = application.Student.Skills,
        StudentLinkedInUrl = application.Student.LinkedInUrl,
        StudentGitHubUrl = application.Student.GitHubUrl,
        CoverLetter = application.CoverLetter,
        CVUrl = application.CVUrl,
        Status = application.Status,
        AppliedAt = application.AppliedAt,
        UpdatedAt = application.UpdatedAt,
        ReviewedAt = application.ReviewedAt,
        CompanyNotes = application.CompanyNotes
    };

    private static ApplicationDto ToDto(InternshipApplication application) => new()
    {
        Id = application.Id,
        InternshipPostId = application.InternshipPostId,
        InternshipTitle = application.InternshipPost.Title,
        CompanyName = application.InternshipPost.Company.CompanyName,
        CoverLetter = application.CoverLetter,
        CVUrl = application.CVUrl,
        Status = application.Status,
        AppliedAt = application.AppliedAt,
        UpdatedAt = application.UpdatedAt,
        ReviewedAt = application.ReviewedAt,
        CompanyNotes = application.CompanyNotes
    };
}

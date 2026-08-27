using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Tests.TestHelpers;
using static InternshipManagement.Tests.TestHelpers.EntityFactory;

namespace InternshipManagement.Tests.Services;

public class ApplicationServiceTests
{
    [Fact]
    public async Task ApplyAsync_InternshipNotFound_ReturnsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var (student, _) = await CreateStudentAsync(db);
        var service = new ApplicationService(db);

        var (result, _, _) = await service.ApplyAsync(999, student.Id, new ApplyToInternshipDto());

        Assert.Equal(OperationResult.NotFound, result);
    }

    [Fact]
    public async Task ApplyAsync_ClosedInternship_ReturnsValidationFailed()
    {
        // REQUIREMENTS.md §4.1 rule 2: only an Open internship can be applied to.
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Closed);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);

        var (result, error, _) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("open", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApplyAsync_PastDeadline_ReturnsValidationFailed()
    {
        // REQUIREMENTS.md §4.1 rule 3: the deadline is checked independently of Status,
        // since nothing automatically flips an Open post to Closed once its date passes.
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open, deadline: DateTime.UtcNow.AddDays(-1));
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);

        var (result, error, _) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("deadline", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApplyAsync_AlreadyApplied_ReturnsValidationFailed()
    {
        // REQUIREMENTS.md §4.1 rule 4: no duplicate applications. This exercises the
        // service's own AnyAsync pre-check, not the database's unique-index fallback
        // (that race-condition path needs a real Postgres exception - see
        // docs/DECISIONS.md D20).
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());

        var (result, error, _) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("already applied", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApplyAsync_ValidOpenPost_CreatesPendingApplication()
    {
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);

        var (result, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto { CoverLetter = "Hire me!" });

        Assert.Equal(OperationResult.Success, result);
        Assert.Equal(ApplicationStatus.Pending, application!.Status);
        Assert.Equal("Hire me!", application.CoverLetter);
    }

    [Fact]
    public async Task WithdrawAsync_NotOwner_ReturnsForbidden()
    {
        // REQUIREMENTS.md STU-8: only the applying student can withdraw their own application.
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());
        var (otherStudentUser, _) = await CreateStudentAsync(db);

        var (result, _, _) = await service.WithdrawAsync(application!.Id, otherStudentUser.Id);

        Assert.Equal(OperationResult.Forbidden, result);
    }

    [Fact]
    public async Task WithdrawAsync_NotPending_ReturnsValidationFailed()
    {
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (companyUser, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());
        await service.UpdateStatusAsync(application!.Id, companyUser.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Shortlisted });

        var (result, error, _) = await service.WithdrawAsync(application.Id, studentUser.Id);

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("pending", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WithdrawAsync_PendingApplication_SetsWithdrawn()
    {
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());

        var (result, _, updated) = await service.WithdrawAsync(application!.Id, studentUser.Id);

        Assert.Equal(OperationResult.Success, result);
        Assert.Equal(ApplicationStatus.Withdrawn, updated!.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_NotOwnerCompany_ReturnsForbidden()
    {
        // REQUIREMENTS.md CO-8: only the company that owns the internship can review it.
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());
        var (otherCompanyUser, _) = await CreateCompanyAsync(db);

        var (result, _, _) = await service.UpdateStatusAsync(application!.Id, otherCompanyUser.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Accepted });

        Assert.Equal(OperationResult.Forbidden, result);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithdrawnApplication_ReturnsValidationFailed()
    {
        // REQUIREMENTS.md §4.2 rule 5: a withdrawn application is off-limits regardless
        // of what status the company tries to set.
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (companyUser, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());
        await service.WithdrawAsync(application!.Id, studentUser.Id);

        var (result, error, _) = await service.UpdateStatusAsync(application.Id, companyUser.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Accepted });

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("withdrawn", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidTargetStatus_ReturnsValidationFailed()
    {
        // A company may only move an application to Shortlisted/Accepted/Rejected - not
        // back to Pending, and not to Withdrawn (a student-only action).
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (companyUser, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());

        var (result, _, _) = await service.UpdateStatusAsync(application!.Id, companyUser.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Pending });

        Assert.Equal(OperationResult.ValidationFailed, result);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_SetsStatusAndReviewedAt()
    {
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (companyUser, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());

        var (result, _, applicant) = await service.UpdateStatusAsync(application!.Id, companyUser.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Shortlisted, CompanyNotes = "Looks good" });

        Assert.Equal(OperationResult.Success, result);
        Assert.Equal(ApplicationStatus.Shortlisted, applicant!.Status);
        Assert.Equal("Looks good", applicant.CompanyNotes);
        Assert.NotNull(applicant.ReviewedAt);
    }

    [Fact]
    public async Task UpdateStatusAsync_NoNotesProvided_KeepsExistingNotes()
    {
        // A company accepting an application right after shortlisting it shouldn't
        // accidentally erase the note it left the first time.
        await using var db = TestDbContextFactory.Create();
        var (studentUser, _) = await CreateStudentAsync(db);
        var (companyUser, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new ApplicationService(db);
        var (_, _, application) = await service.ApplyAsync(post.Id, studentUser.Id, new ApplyToInternshipDto());
        await service.UpdateStatusAsync(application!.Id, companyUser.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Shortlisted, CompanyNotes = "Initial note" });

        var (_, _, applicant) = await service.UpdateStatusAsync(application.Id, companyUser.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Accepted });

        Assert.Equal("Initial note", applicant!.CompanyNotes);
    }
}

using InternshipManagement.Api.Data;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Tests.TestHelpers;

// Shared entity builders for tests that need a User+profile (or an internship post)
// already sitting in the database before exercising a service method - avoids
// repeating the same handful of required fields in every test file. Each call adds its
// entities to the given context and saves immediately, so callers get back a real,
// already-persisted entity with a real Id to use.
public static class EntityFactory
{
    public static async Task<(User User, StudentProfile Student)> CreateStudentAsync(AppDbContext db, string? fullName = null)
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            Email = $"{Guid.NewGuid()}@example.com",
            PasswordHash = "irrelevant-for-this-test",
            Role = UserRole.Student,
            CreatedAt = now,
            UpdatedAt = now
        };
        var student = new StudentProfile { User = user, FullName = fullName ?? "Test Student", CreatedAt = now, UpdatedAt = now };
        db.Users.Add(user);
        db.StudentProfiles.Add(student);
        await db.SaveChangesAsync();
        return (user, student);
    }

    public static async Task<(User User, CompanyProfile Company)> CreateCompanyAsync(AppDbContext db, bool isApproved = true, string? companyName = null)
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            Email = $"{Guid.NewGuid()}@example.com",
            PasswordHash = "irrelevant-for-this-test",
            Role = UserRole.Company,
            CreatedAt = now,
            UpdatedAt = now
        };
        var company = new CompanyProfile { User = user, CompanyName = companyName ?? "Test Co", IsApproved = isApproved, CreatedAt = now, UpdatedAt = now };
        db.Users.Add(user);
        db.CompanyProfiles.Add(company);
        await db.SaveChangesAsync();
        return (user, company);
    }

    public static InternshipPost NewPost(
        CompanyProfile company,
        InternshipStatus status = InternshipStatus.Draft,
        string? title = "A Title",
        string? description = "A description",
        DateTime? deadline = null,
        WorkMode workMode = WorkMode.Onsite)
    {
        var now = DateTime.UtcNow;
        return new InternshipPost
        {
            Company = company,
            Title = title ?? string.Empty,
            Description = description,
            WorkMode = workMode,
            ApplicationDeadline = deadline ?? now.AddDays(30),
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}

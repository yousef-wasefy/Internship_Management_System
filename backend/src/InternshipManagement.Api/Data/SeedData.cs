using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Data;

// TEMPORARY: seeds one placeholder company so InternshipPost.CompanyId has something
// valid to point at before authentication/company registration exist. InternshipService
// currently assigns every new post to this company. This whole file is removed in
// Phase 8, once posts are tied to the actual logged-in company instead.
public static class SeedData
{
    public static async Task EnsureSeededAsync(AppDbContext context)
    {
        if (await context.CompanyProfiles.AnyAsync())
        {
            return; // already seeded
        }

        var now = DateTime.UtcNow;

        var companyProfile = new CompanyProfile
        {
            User = new User
            {
                Email = "placeholder-company@example.com",
                PasswordHash = "not-a-real-account-no-auth-yet",
                Role = UserRole.Company,
                CreatedAt = now,
                UpdatedAt = now
            },
            CompanyName = "Placeholder Company (temporary - see Phase 8)",
            IsApproved = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.CompanyProfiles.Add(companyProfile);
        await context.SaveChangesAsync();
    }
}

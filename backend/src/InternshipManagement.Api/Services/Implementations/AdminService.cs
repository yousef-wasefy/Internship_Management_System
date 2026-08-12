using InternshipManagement.Api.Data;
using InternshipManagement.Api.DTOs.Admin;
using InternshipManagement.Api.DTOs.Companies;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        // Simple, readable counts - not combined into one query, since clarity matters
        // more than shaving a handful of round-trips at this project's scale.
        return new AdminDashboardDto
        {
            TotalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student),
            TotalCompanies = await _context.Users.CountAsync(u => u.Role == UserRole.Company),
            PendingCompanies = await _context.CompanyProfiles.CountAsync(c => !c.IsApproved && !c.User.IsDisabled),
            TotalInternships = await _context.InternshipPosts.CountAsync(),
            OpenInternships = await _context.InternshipPosts.CountAsync(p => p.Status == InternshipStatus.Open),
            TotalApplications = await _context.InternshipApplications.CountAsync(),
            AcceptedApplications = await _context.InternshipApplications.CountAsync(a => a.Status == ApplicationStatus.Accepted),
            RejectedApplications = await _context.InternshipApplications.CountAsync(a => a.Status == ApplicationStatus.Rejected)
        };
    }

    public async Task<List<CompanyProfileDto>> GetPendingCompaniesAsync()
    {
        // "Pending" = never approved AND not already rejected (rejecting disables the
        // account - see RejectCompanyAsync - so a rejected company naturally drops off
        // this list instead of showing up forever).
        var companies = await _context.CompanyProfiles
            .Include(c => c.User)
            .Where(c => !c.IsApproved && !c.User.IsDisabled)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return companies.Select(CompanyService.ToDto).ToList();
    }

    public async Task<CompanyProfileDto?> ApproveCompanyAsync(int companyProfileId)
    {
        var profile = await _context.CompanyProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == companyProfileId);
        if (profile is null)
        {
            return null;
        }

        profile.IsApproved = true;
        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return CompanyService.ToDto(profile);
    }

    public async Task<CompanyProfileDto?> RejectCompanyAsync(int companyProfileId)
    {
        var profile = await _context.CompanyProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == companyProfileId);
        if (profile is null)
        {
            return null;
        }

        // The schema has no separate "Rejected" state (only IsApproved, a boolean) -
        // see docs/DECISIONS.md D16. Rejecting keeps IsApproved false and disables the
        // account outright, so a rejected company can't log back in and re-apply for
        // posts on repeat.
        profile.IsApproved = false;
        profile.User.IsDisabled = true;
        profile.UpdatedAt = DateTime.UtcNow;
        profile.User.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return CompanyService.ToDto(profile);
    }

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.StudentProfile)
            .Include(u => u.CompanyProfile)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(ToAdminUserDto).ToList();
    }

    public async Task<AdminUserDto?> DisableUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.StudentProfile)
            .Include(u => u.CompanyProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return null;
        }

        user.IsDisabled = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ToAdminUserDto(user);
    }

    private static AdminUserDto ToAdminUserDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.StudentProfile?.FullName ?? user.CompanyProfile?.CompanyName,
        Role = user.Role,
        IsDisabled = user.IsDisabled,
        CreatedAt = user.CreatedAt
    };
}

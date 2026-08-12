using InternshipManagement.Api.Data;
using InternshipManagement.Api.DTOs.Companies;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyProfileDto?> GetMyProfileAsync(int userId)
    {
        var profile = await _context.CompanyProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return profile is null ? null : ToDto(profile);
    }

    public async Task<bool> UpdateMyProfileAsync(int userId, UpdateCompanyProfileDto dto)
    {
        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null)
        {
            return false;
        }

        profile.CompanyName = dto.CompanyName;
        profile.Industry = dto.Industry;
        profile.WebsiteUrl = dto.WebsiteUrl;
        profile.Description = dto.Description;
        profile.Location = dto.Location;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    internal static CompanyProfileDto ToDto(CompanyProfile profile) => new()
    {
        Id = profile.Id,
        Email = profile.User.Email,
        CompanyName = profile.CompanyName,
        Industry = profile.Industry,
        WebsiteUrl = profile.WebsiteUrl,
        Description = profile.Description,
        Location = profile.Location,
        IsApproved = profile.IsApproved,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}

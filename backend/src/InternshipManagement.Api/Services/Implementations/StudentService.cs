using InternshipManagement.Api.Data;
using InternshipManagement.Api.DTOs.Students;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;

    public StudentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StudentProfileDto?> GetMyProfileAsync(int userId)
    {
        var profile = await _context.StudentProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return profile is null ? null : ToDto(profile);
    }

    public async Task<bool> UpdateMyProfileAsync(int userId, UpdateStudentProfileDto dto)
    {
        var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null)
        {
            return false;
        }

        profile.FullName = dto.FullName;
        profile.University = dto.University;
        profile.Faculty = dto.Faculty;
        profile.Major = dto.Major;
        profile.AcademicYear = dto.AcademicYear;
        profile.Skills = dto.Skills;
        profile.CVUrl = dto.CVUrl;
        profile.LinkedInUrl = dto.LinkedInUrl;
        profile.GitHubUrl = dto.GitHubUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static StudentProfileDto ToDto(StudentProfile profile) => new()
    {
        Id = profile.Id,
        Email = profile.User.Email,
        FullName = profile.FullName,
        University = profile.University,
        Faculty = profile.Faculty,
        Major = profile.Major,
        AcademicYear = profile.AcademicYear,
        Skills = profile.Skills,
        CVUrl = profile.CVUrl,
        LinkedInUrl = profile.LinkedInUrl,
        GitHubUrl = profile.GitHubUrl,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}

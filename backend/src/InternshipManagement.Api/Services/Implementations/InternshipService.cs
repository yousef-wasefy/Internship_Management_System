using InternshipManagement.Api.Data;
using InternshipManagement.Api.DTOs.Internships;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Services.Implementations;

public class InternshipService : IInternshipService
{
    private readonly AppDbContext _context;

    public InternshipService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InternshipListDto>> GetAllAsync()
    {
        var posts = await _context.InternshipPosts
            .Include(p => p.Company)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(ToListDto).ToList();
    }

    public async Task<InternshipDetailsDto?> GetByIdAsync(int id)
    {
        var post = await _context.InternshipPosts
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == id);

        return post is null ? null : ToDetailsDto(post);
    }

    public async Task<InternshipDetailsDto> CreateAsync(CreateInternshipDto dto)
    {
        // TEMPORARY: every post is assigned to the single seeded placeholder company
        // until Phase 6/7 add real authentication and Phase 8 wires posts to the
        // logged-in company instead (see Data/SeedData.cs).
        var company = await _context.CompanyProfiles.FirstAsync();

        var now = DateTime.UtcNow;
        var post = new InternshipPost
        {
            Company = company,
            Title = dto.Title,
            Description = dto.Description,
            Requirements = dto.Requirements,
            Responsibilities = dto.Responsibilities,
            Location = dto.Location,
            WorkMode = dto.WorkMode,
            Duration = dto.Duration,
            ApplicationDeadline = AsUtc(dto.ApplicationDeadline),
            Status = InternshipStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.InternshipPosts.Add(post);
        await _context.SaveChangesAsync();

        return ToDetailsDto(post);
    }

    public async Task<bool> UpdateAsync(int id, UpdateInternshipDto dto)
    {
        var post = await _context.InternshipPosts.FindAsync(id);
        if (post is null)
        {
            return false;
        }

        post.Title = dto.Title;
        post.Description = dto.Description;
        post.Requirements = dto.Requirements;
        post.Responsibilities = dto.Responsibilities;
        post.Location = dto.Location;
        post.WorkMode = dto.WorkMode;
        post.Duration = dto.Duration;
        post.ApplicationDeadline = AsUtc(dto.ApplicationDeadline);
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var post = await _context.InternshipPosts.FindAsync(id);
        if (post is null)
        {
            return false;
        }

        _context.InternshipPosts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }

    // Npgsql requires "timestamp with time zone" columns to receive a DateTime whose
    // Kind is explicitly Utc. A client-supplied date with no timezone offset comes in
    // as Kind=Unspecified, which Npgsql would otherwise reject at save time - this
    // treats such values as UTC instead of throwing.
    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static InternshipListDto ToListDto(InternshipPost post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Location = post.Location,
        WorkMode = post.WorkMode,
        ApplicationDeadline = post.ApplicationDeadline,
        Status = post.Status,
        CompanyName = post.Company.CompanyName
    };

    private static InternshipDetailsDto ToDetailsDto(InternshipPost post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Description = post.Description,
        Requirements = post.Requirements,
        Responsibilities = post.Responsibilities,
        Location = post.Location,
        WorkMode = post.WorkMode,
        Duration = post.Duration,
        ApplicationDeadline = post.ApplicationDeadline,
        Status = post.Status,
        CompanyName = post.Company.CompanyName,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt
    };
}

using InternshipManagement.Api.Data;
using InternshipManagement.Api.DTOs.Common;
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

    public async Task<PagedResult<InternshipListDto>> GetAllAsync(InternshipQueryParameters query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50); // cap prevents an absurdly large page request

        // Public listing - only Open posts are visible to students (REQUIREMENTS.md §5).
        var baseQuery = _context.InternshipPosts
            .Include(p => p.Company)
            .Where(p => p.Status == InternshipStatus.Open);

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            // EF.Functions.ILike is Postgres' case-insensitive LIKE, provided by the
            // Npgsql EF Core provider - the natural choice for this specific database.
            baseQuery = baseQuery.Where(p => p.Location != null && EF.Functions.ILike(p.Location, $"%{query.Location}%"));
        }

        if (query.WorkMode.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.WorkMode == query.WorkMode.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            baseQuery = baseQuery.Where(p => EF.Functions.ILike(p.Title, $"%{query.Search}%"));
        }

        var totalCount = await baseQuery.CountAsync();

        var posts = await baseQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<InternshipListDto>
        {
            Items = posts.Select(ToListDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<InternshipDetailsDto?> GetByIdAsync(int id)
    {
        var post = await _context.InternshipPosts
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == InternshipStatus.Open);

        return post is null ? null : ToDetailsDto(post);
    }

    public async Task<List<InternshipListDto>> GetByCompanyUserIdAsync(int userId, InternshipStatus? status)
    {
        var query = _context.InternshipPosts
            .Include(p => p.Company)
            .Where(p => p.Company.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return posts.Select(ToListDto).ToList();
    }

    public async Task<InternshipDetailsDto> CreateAsync(CreateInternshipDto dto, int userId)
    {
        // Every Company-role user has exactly one CompanyProfile, created atomically at
        // registration (Phase 6) - FirstAsync throws if that invariant is ever broken,
        // which is preferable to silently guessing which company "owns" a new post.
        var company = await _context.CompanyProfiles.FirstAsync(c => c.UserId == userId);

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

    public async Task<OperationResult> UpdateAsync(int id, UpdateInternshipDto dto, int userId)
    {
        var post = await _context.InternshipPosts.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null)
        {
            return OperationResult.NotFound;
        }

        if (post.Company.UserId != userId)
        {
            return OperationResult.Forbidden; // REQUIREMENTS.md CO-2: only the owner can edit
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
        return OperationResult.Success;
    }

    public async Task<OperationResult> DeleteAsync(int id, int userId)
    {
        var post = await _context.InternshipPosts.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null)
        {
            return OperationResult.NotFound;
        }

        if (post.Company.UserId != userId)
        {
            return OperationResult.Forbidden;
        }

        _context.InternshipPosts.Remove(post);
        await _context.SaveChangesAsync();
        return OperationResult.Success;
    }

    public async Task<(OperationResult Result, string? ErrorMessage, InternshipDetailsDto? Internship)> OpenAsync(int id, int userId)
    {
        var post = await _context.InternshipPosts.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null)
        {
            return (OperationResult.NotFound, null, null);
        }

        if (post.Company.UserId != userId)
        {
            return (OperationResult.Forbidden, null, null);
        }

        if (post.Status == InternshipStatus.Cancelled)
        {
            return (OperationResult.ValidationFailed, "A cancelled internship cannot be reopened.", null);
        }

        // Only an approved company may publish (REQUIREMENTS.md CO-1 / §7.2 rule 1).
        if (!post.Company.IsApproved)
        {
            return (OperationResult.ValidationFailed,
                "Your company must be approved by an admin before you can open internship posts.", null);
        }

        if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Description))
        {
            return (OperationResult.ValidationFailed,
                "Title and description are required before opening an internship.", null);
        }

        if (post.ApplicationDeadline <= DateTime.UtcNow)
        {
            return (OperationResult.ValidationFailed,
                "The application deadline must be in the future to open this internship.", null);
        }

        post.Status = InternshipStatus.Open;
        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (OperationResult.Success, null, ToDetailsDto(post));
    }

    public async Task<(OperationResult Result, string? ErrorMessage, InternshipDetailsDto? Internship)> CloseAsync(int id, int userId)
    {
        var post = await _context.InternshipPosts.Include(p => p.Company).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null)
        {
            return (OperationResult.NotFound, null, null);
        }

        if (post.Company.UserId != userId)
        {
            return (OperationResult.Forbidden, null, null);
        }

        if (post.Status != InternshipStatus.Open)
        {
            return (OperationResult.ValidationFailed, "Only an open internship can be closed.", null);
        }

        post.Status = InternshipStatus.Closed;
        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (OperationResult.Success, null, ToDetailsDto(post));
    }

    // Npgsql requires "timestamp with time zone" columns to receive a DateTime whose
    // Kind is explicitly Utc - client-supplied dates without a timezone offset come in
    // as Unspecified, so this treats them as UTC rather than throwing at save time.
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

using InternshipManagement.Api.Data;
using InternshipManagement.Api.DTOs.Auth;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto?> RegisterStudentAsync(RegisterStudentDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return null; // an account with this email already exists
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Student,
            CreatedAt = now,
            UpdatedAt = now,
            StudentProfile = new StudentProfile
            {
                FullName = dto.FullName,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto?> RegisterCompanyAsync(RegisterCompanyDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return null; // an account with this email already exists
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Company,
            CreatedAt = now,
            UpdatedAt = now,
            CompanyProfile = new CompanyProfile
            {
                CompanyName = dto.CompanyName,
                IsApproved = false, // companies start unapproved (REQUIREMENTS.md CO-3)
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // A disabled user (REQUIREMENTS.md AD-5) must not be able to log in at all.
        if (user is null || user.IsDisabled || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return null; // deliberately the same error for "no such user" and "wrong
                          // password" - never reveal which one it was.
        }

        return BuildAuthResponse(user);
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user is null
            ? null
            : new CurrentUserDto { Id = user.Id, Email = user.Email, Role = user.Role };
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Email = user.Email,
            Role = user.Role
        };
    }
}

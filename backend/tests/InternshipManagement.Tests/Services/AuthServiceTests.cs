using InternshipManagement.Api.DTOs.Auth;
using InternshipManagement.Api.Entities;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Tests.TestHelpers;

namespace InternshipManagement.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterStudentAsync_NewEmail_CreatesUserAndReturnsToken()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());

        var result = await service.RegisterStudentAsync(new RegisterStudentDto
        {
            Email = "new.student@example.com",
            Password = "Password123",
            FullName = "New Student"
        });

        Assert.NotNull(result);
        Assert.Equal("new.student@example.com", result!.Email);
        Assert.Equal(UserRole.Student, result.Role);
        Assert.NotEmpty(result.Token);

        var savedUser = Assert.Single(db.Users);
        Assert.Equal(UserRole.Student, savedUser.Role);
        // REQUIREMENTS.md: passwords are never stored in plain text.
        Assert.NotEqual("Password123", savedUser.PasswordHash);
    }

    [Fact]
    public async Task RegisterStudentAsync_DuplicateEmail_ReturnsNull()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());
        await service.RegisterStudentAsync(new RegisterStudentDto { Email = "dup@example.com", Password = "Password123", FullName = "First" });

        var result = await service.RegisterStudentAsync(new RegisterStudentDto { Email = "dup@example.com", Password = "Password123", FullName = "Second" });

        Assert.Null(result);
        Assert.Single(db.Users); // the second attempt must not have created a row
    }

    [Fact]
    public async Task RegisterCompanyAsync_NewEmail_CreatesUnapprovedCompany()
    {
        // REQUIREMENTS.md CO-3: every company starts unapproved until an admin acts.
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());

        var result = await service.RegisterCompanyAsync(new RegisterCompanyDto
        {
            Email = "new.company@example.com",
            Password = "Password123",
            CompanyName = "New Co"
        });

        Assert.NotNull(result);
        Assert.Equal(UserRole.Company, result!.Role);
        var savedCompany = Assert.Single(db.CompanyProfiles);
        Assert.False(savedCompany.IsApproved);
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsToken()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());
        await service.RegisterStudentAsync(new RegisterStudentDto
        {
            Email = "login@example.com",
            Password = "Password123",
            FullName = "Login Test"
        });

        var result = await service.LoginAsync(new LoginDto { Email = "login@example.com", Password = "Password123" });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());
        await service.RegisterStudentAsync(new RegisterStudentDto
        {
            Email = "login2@example.com",
            Password = "Password123",
            FullName = "Login Test"
        });

        var result = await service.LoginAsync(new LoginDto { Email = "login2@example.com", Password = "WrongPassword" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsNull()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());

        var result = await service.LoginAsync(new LoginDto { Email = "nobody@example.com", Password = "Password123" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_DisabledUser_ReturnsNull()
    {
        // REQUIREMENTS.md AD-5 / Phase 11: a disabled account must be blocked from
        // logging in at all, even with the correct password.
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());
        await service.RegisterStudentAsync(new RegisterStudentDto
        {
            Email = "disabled@example.com",
            Password = "Password123",
            FullName = "Disabled Student"
        });
        var user = db.Users.Single();
        user.IsDisabled = true;
        await db.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginDto { Email = "disabled@example.com", Password = "Password123" });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserAsync_DisabledUser_ReturnsNull()
    {
        // Regression test for the Phase 11 fix: a disabled user's still-valid token must
        // not resolve to a usable identity via /auth/me, even though the JWT itself is
        // still cryptographically valid (tokens aren't revoked, see docs/DECISIONS.md D16).
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());
        var user = new User
        {
            Email = "stale-token@example.com",
            PasswordHash = "irrelevant-for-this-test",
            Role = UserRole.Student,
            IsDisabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await service.GetCurrentUserAsync(user.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ActiveUser_ReturnsUser()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AuthService(db, new FakeJwtTokenGenerator());
        var registered = await service.RegisterStudentAsync(new RegisterStudentDto
        {
            Email = "active@example.com",
            Password = "Password123",
            FullName = "Active Student"
        });
        var userId = db.Users.Single().Id;

        var result = await service.GetCurrentUserAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(registered!.Email, result!.Email);
    }
}

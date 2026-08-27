using InternshipManagement.Api.DTOs.Companies;
using InternshipManagement.Api.DTOs.Students;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Tests.TestHelpers;
using static InternshipManagement.Tests.TestHelpers.EntityFactory;

namespace InternshipManagement.Tests.Services;

// StudentService and CompanyService (Phase 7) are simple, near-identical "manage your
// own profile" services with very little branching - one shared file rather than two
// near-empty ones, since there's little else to say about either beyond these two cases.
public class ProfileServiceTests
{
    [Fact]
    public async Task StudentService_UpdateMyProfileAsync_UnknownUser_ReturnsFalse()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new StudentService(db);

        var result = await service.UpdateMyProfileAsync(999, new UpdateStudentProfileDto { FullName = "Doesn't matter" });

        Assert.False(result);
    }

    [Fact]
    public async Task StudentService_UpdateMyProfileAsync_ExistingUser_UpdatesAndReturnsTrue()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, _) = await CreateStudentAsync(db, fullName: "Old Name");
        var service = new StudentService(db);

        var result = await service.UpdateMyProfileAsync(user.Id, new UpdateStudentProfileDto { FullName = "New Name", University = "Cairo University" });

        Assert.True(result);
        var profile = await service.GetMyProfileAsync(user.Id);
        Assert.Equal("New Name", profile!.FullName);
        Assert.Equal("Cairo University", profile.University);
    }

    [Fact]
    public async Task CompanyService_UpdateMyProfileAsync_UnknownUser_ReturnsFalse()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new CompanyService(db);

        var result = await service.UpdateMyProfileAsync(999, new UpdateCompanyProfileDto { CompanyName = "Doesn't matter" });

        Assert.False(result);
    }

    [Fact]
    public async Task CompanyService_UpdateMyProfileAsync_ExistingUser_UpdatesAndReturnsTrue()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, _) = await CreateCompanyAsync(db, isApproved: false, companyName: "Old Name");
        var service = new CompanyService(db);

        var result = await service.UpdateMyProfileAsync(user.Id, new UpdateCompanyProfileDto { CompanyName = "New Name", Industry = "Software" });

        Assert.True(result);
        var profile = await service.GetMyProfileAsync(user.Id);
        Assert.Equal("New Name", profile!.CompanyName);
        Assert.Equal("Software", profile.Industry);
        // Confirms UpdateMyProfileAsync can't touch IsApproved - it's not part of the DTO.
        Assert.False(profile.IsApproved);
    }
}

using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Tests.TestHelpers;
using static InternshipManagement.Tests.TestHelpers.EntityFactory;

namespace InternshipManagement.Tests.Services;

public class AdminServiceTests
{
    [Fact]
    public async Task ApproveCompanyAsync_ExistingCompany_SetsApprovedTrue()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, company) = await CreateCompanyAsync(db, isApproved: false);
        var service = new AdminService(db);

        var result = await service.ApproveCompanyAsync(company.Id);

        Assert.NotNull(result);
        Assert.True(result!.IsApproved);
    }

    [Fact]
    public async Task ApproveCompanyAsync_NotFound_ReturnsNull()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AdminService(db);

        var result = await service.ApproveCompanyAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task RejectCompanyAsync_SetsUnapprovedAndDisablesUser()
    {
        // docs/DECISIONS.md D16: there's no separate "Rejected" state - rejecting keeps
        // IsApproved false and disables the account outright, so it can't log back in
        // and re-apply for approval indefinitely.
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db, isApproved: false);
        var service = new AdminService(db);

        var result = await service.RejectCompanyAsync(company.Id);

        Assert.NotNull(result);
        Assert.False(result!.IsApproved);
        Assert.True(db.Users.Single(u => u.Id == user.Id).IsDisabled);
    }

    [Fact]
    public async Task DisableUserAsync_SetsIsDisabledTrue()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, _) = await CreateStudentAsync(db);
        var service = new AdminService(db);

        var result = await service.DisableUserAsync(user.Id);

        Assert.NotNull(result);
        Assert.True(result!.IsDisabled);
    }

    [Fact]
    public async Task DisableUserAsync_NotFound_ReturnsNull()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new AdminService(db);

        var result = await service.DisableUserAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPendingCompaniesAsync_ExcludesApprovedAndRejectedCompanies()
    {
        await using var db = TestDbContextFactory.Create();
        await CreateCompanyAsync(db, isApproved: false, companyName: "Pending Co");
        await CreateCompanyAsync(db, isApproved: true, companyName: "Approved Co");
        var (rejectedUser, _) = await CreateCompanyAsync(db, isApproved: false, companyName: "Rejected Co");
        rejectedUser.IsDisabled = true;
        await db.SaveChangesAsync();
        var service = new AdminService(db);

        var result = await service.GetPendingCompaniesAsync();

        var onlyCompany = Assert.Single(result);
        Assert.Equal("Pending Co", onlyCompany.CompanyName);
    }

    [Fact]
    public async Task GetDashboardAsync_CountsMatchSeededData()
    {
        await using var db = TestDbContextFactory.Create();
        await CreateStudentAsync(db);
        await CreateStudentAsync(db);
        var (_, approvedCompany) = await CreateCompanyAsync(db, isApproved: true);
        await CreateCompanyAsync(db, isApproved: false);
        db.InternshipPosts.Add(NewPost(approvedCompany, InternshipStatus.Open));
        db.InternshipPosts.Add(NewPost(approvedCompany, InternshipStatus.Draft));
        await db.SaveChangesAsync();
        var service = new AdminService(db);

        var result = await service.GetDashboardAsync();

        Assert.Equal(2, result.TotalStudents);
        Assert.Equal(2, result.TotalCompanies);
        Assert.Equal(1, result.PendingCompanies);
        Assert.Equal(2, result.TotalInternships);
        Assert.Equal(1, result.OpenInternships);
    }
}

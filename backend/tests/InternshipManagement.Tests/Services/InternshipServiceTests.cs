using InternshipManagement.Api.DTOs.Internships;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Services.Implementations;
using InternshipManagement.Tests.TestHelpers;
using static InternshipManagement.Tests.TestHelpers.EntityFactory;

namespace InternshipManagement.Tests.Services;

public class InternshipServiceTests
{
    [Fact]
    public async Task CreateAsync_NewPost_StartsAsDraft()
    {
        // Phase 8: every internship starts as Draft, regardless of what's submitted.
        await using var db = TestDbContextFactory.Create();
        var (user, _) = await CreateCompanyAsync(db);
        var service = new InternshipService(db);

        var result = await service.CreateAsync(new CreateInternshipDto
        {
            Title = "Backend Intern",
            WorkMode = WorkMode.Remote,
            ApplicationDeadline = DateTime.UtcNow.AddDays(10)
        }, user.Id);

        Assert.Equal(InternshipStatus.Draft, result.Status);
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsOpenPosts()
    {
        // REQUIREMENTS.md §5: the public listing must never show Draft/Closed/Cancelled posts.
        await using var db = TestDbContextFactory.Create();
        var (_, company) = await CreateCompanyAsync(db);
        db.InternshipPosts.AddRange(
            NewPost(company, InternshipStatus.Draft, title: "Draft Post"),
            NewPost(company, InternshipStatus.Open, title: "Open Post"),
            NewPost(company, InternshipStatus.Closed, title: "Closed Post"),
            NewPost(company, InternshipStatus.Cancelled, title: "Cancelled Post"));
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var result = await service.GetAllAsync(new InternshipQueryParameters());

        var onlyPost = Assert.Single(result.Items);
        Assert.Equal("Open Post", onlyPost.Title);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByWorkMode()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, company) = await CreateCompanyAsync(db);
        db.InternshipPosts.AddRange(
            NewPost(company, InternshipStatus.Open, title: "Remote Post", workMode: WorkMode.Remote),
            NewPost(company, InternshipStatus.Open, title: "Onsite Post", workMode: WorkMode.Onsite));
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var result = await service.GetAllAsync(new InternshipQueryParameters { WorkMode = WorkMode.Remote });

        var onlyPost = Assert.Single(result.Items);
        Assert.Equal("Remote Post", onlyPost.Title);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPageAndTotalPages()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, company) = await CreateCompanyAsync(db);
        for (var i = 0; i < 5; i++)
        {
            db.InternshipPosts.Add(NewPost(company, InternshipStatus.Open, title: $"Post {i}"));
        }
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var page1 = await service.GetAllAsync(new InternshipQueryParameters { Page = 1, PageSize = 2 });
        var page3 = await service.GetAllAsync(new InternshipQueryParameters { Page = 3, PageSize = 2 });

        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages); // ceil(5 / 2)
        Assert.Single(page3.Items); // the last, partial page
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new InternshipService(db);

        var result = await service.UpdateAsync(999, new UpdateInternshipDto { Title = "x", ApplicationDeadline = DateTime.UtcNow }, userId: 1);

        Assert.Equal(OperationResult.NotFound, result);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ReturnsForbidden()
    {
        // REQUIREMENTS.md CO-2: only the owning company can edit its own post.
        await using var db = TestDbContextFactory.Create();
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var (otherUser, _) = await CreateCompanyAsync(db);
        var service = new InternshipService(db);

        var result = await service.UpdateAsync(post.Id, new UpdateInternshipDto { Title = "x", ApplicationDeadline = DateTime.UtcNow }, otherUser.Id);

        Assert.Equal(OperationResult.Forbidden, result);
    }

    [Fact]
    public async Task UpdateAsync_Owner_UpdatesFields()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, title: "Old Title");
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var result = await service.UpdateAsync(post.Id, new UpdateInternshipDto
        {
            Title = "New Title",
            WorkMode = WorkMode.Hybrid,
            ApplicationDeadline = DateTime.UtcNow.AddDays(5)
        }, user.Id);

        Assert.Equal(OperationResult.Success, result);
        Assert.Equal("New Title", db.InternshipPosts.Single().Title);
    }

    [Fact]
    public async Task DeleteAsync_Owner_RemovesPost()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var result = await service.DeleteAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.Success, result);
        Assert.Empty(db.InternshipPosts);
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ReturnsForbidden()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var (otherUser, _) = await CreateCompanyAsync(db);
        var service = new InternshipService(db);

        var result = await service.DeleteAsync(post.Id, otherUser.Id);

        Assert.Equal(OperationResult.Forbidden, result);
        Assert.Single(db.InternshipPosts); // untouched
    }

    [Fact]
    public async Task OpenAsync_UnapprovedCompany_ReturnsValidationFailed()
    {
        // REQUIREMENTS.md CO-1: an unapproved company cannot publish, even a well-formed post.
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db, isApproved: false);
        var post = NewPost(company);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, error, _) = await service.OpenAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("approved", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenAsync_PastDeadline_ReturnsValidationFailed()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, deadline: DateTime.UtcNow.AddDays(-1));
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, error, _) = await service.OpenAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("deadline", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenAsync_MissingDescription_ReturnsValidationFailed()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, description: null);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, error, _) = await service.OpenAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("description", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenAsync_CancelledPost_ReturnsValidationFailed()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Cancelled);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, error, _) = await service.OpenAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.ValidationFailed, result);
        Assert.Contains("cancelled", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenAsync_ValidDraft_SetsOpenStatus()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, _, internship) = await service.OpenAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.Success, result);
        Assert.Equal(InternshipStatus.Open, internship!.Status);
    }

    [Fact]
    public async Task CloseAsync_NotOpenPost_ReturnsValidationFailed()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Draft);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, _, _) = await service.CloseAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.ValidationFailed, result);
    }

    [Fact]
    public async Task CloseAsync_OpenPost_SetsClosedStatus()
    {
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Open);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, _, internship) = await service.CloseAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.Success, result);
        Assert.Equal(InternshipStatus.Closed, internship!.Status);
    }

    [Fact]
    public async Task GetOwnedByIdAsync_NotOwner_ReturnsForbidden()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, company) = await CreateCompanyAsync(db);
        var post = NewPost(company);
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var (otherUser, _) = await CreateCompanyAsync(db);
        var service = new InternshipService(db);

        var (result, internship) = await service.GetOwnedByIdAsync(post.Id, otherUser.Id);

        Assert.Equal(OperationResult.Forbidden, result);
        Assert.Null(internship);
    }

    [Fact]
    public async Task GetOwnedByIdAsync_Owner_ReturnsDraftPost()
    {
        // Phase 14: this is exactly the gap the public GetByIdAsync can't fill - a Draft
        // post's owner must still be able to fetch its full details (e.g. to edit it).
        await using var db = TestDbContextFactory.Create();
        var (user, company) = await CreateCompanyAsync(db);
        var post = NewPost(company, InternshipStatus.Draft, title: "My Draft");
        db.InternshipPosts.Add(post);
        await db.SaveChangesAsync();
        var service = new InternshipService(db);

        var (result, internship) = await service.GetOwnedByIdAsync(post.Id, user.Id);

        Assert.Equal(OperationResult.Success, result);
        Assert.Equal("My Draft", internship!.Title);
        Assert.Equal(InternshipStatus.Draft, internship.Status);
    }
}

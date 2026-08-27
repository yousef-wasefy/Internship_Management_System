using InternshipManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Tests.TestHelpers;

// Every test gets its own throwaway database, named with a fresh Guid - no shared state
// between tests, and no setup/teardown needed. EF Core's InMemory provider still
// enforces the unique indexes configured in AppDbContext.OnModelCreating (e.g. the
// composite (StudentId, InternshipPostId) index), so most of the schema-level
// enforcement this project relies on is exercised here too.
//
// What this provider can't do: translate Npgsql-specific query syntax
// (EF.Functions.ILike, used by InternshipService.GetAllAsync's location/search
// filters) or throw the real PostgresException the duplicate-application race-condition
// fallback catches. Those two paths are deliberately left to the manual/live testing
// already done in Phases 9 and 12 - see docs/DECISIONS.md D20.
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

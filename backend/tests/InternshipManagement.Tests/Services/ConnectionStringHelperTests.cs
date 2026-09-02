using InternshipManagement.Api.Helpers;
using Npgsql;

namespace InternshipManagement.Tests.Services;

public class ConnectionStringHelperTests
{
    [Fact]
    public void ToNpgsqlConnectionString_KeywordFormat_PassesThroughUnchanged()
    {
        // Local dev (User Secrets) and Docker Compose already supply Npgsql's own
        // format directly - nothing here should touch it.
        const string value = "Host=localhost;Port=5432;Database=internship_management;Username=internship_app;Password=secret";

        var result = ConnectionStringHelper.ToNpgsqlConnectionString(value);

        Assert.Equal(value, result);
    }

    [Fact]
    public void ToNpgsqlConnectionString_PostgresUri_ConvertsToKeywordFormat()
    {
        // Render's shape: postgres://user:pass@host:port/db. Asserting on the parsed
        // builder's properties (not the raw string) means this doesn't depend on
        // Npgsql's exact key-name formatting (e.g. "SSL Mode" vs "Ssl Mode").
        const string value = "postgres://internship_app:s3cret@dpg-example-a.oregon-postgres.render.com:5432/internship_management";

        var result = new NpgsqlConnectionStringBuilder(ConnectionStringHelper.ToNpgsqlConnectionString(value));

        Assert.Equal("dpg-example-a.oregon-postgres.render.com", result.Host);
        Assert.Equal(5432, result.Port);
        Assert.Equal("internship_app", result.Username);
        Assert.Equal("s3cret", result.Password);
        Assert.Equal("internship_management", result.Database);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Fact]
    public void ToNpgsqlConnectionString_PostgresqlScheme_AlsoConverts()
    {
        // Some providers use the longer "postgresql://" scheme instead of "postgres://" -
        // both are the same URI shape and should be handled identically.
        const string value = "postgresql://user:pass@example.com:5432/mydb";

        var result = new NpgsqlConnectionStringBuilder(ConnectionStringHelper.ToNpgsqlConnectionString(value));

        Assert.Equal("example.com", result.Host);
        Assert.Equal("mydb", result.Database);
    }

    [Fact]
    public void ToNpgsqlConnectionString_UriWithEncodedPassword_DecodesIt()
    {
        // A password containing characters like '@' or '/' arrives percent-encoded in
        // the URI (e.g. "p@ss" -> "p%40ss") - the raw encoded form must not end up in
        // the actual Npgsql password.
        const string value = "postgres://user:p%40ss@example.com:5432/mydb";

        var result = new NpgsqlConnectionStringBuilder(ConnectionStringHelper.ToNpgsqlConnectionString(value));

        Assert.Equal("p@ss", result.Password);
    }
}

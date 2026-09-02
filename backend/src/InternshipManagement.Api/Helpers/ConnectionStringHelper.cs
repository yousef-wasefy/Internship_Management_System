using Npgsql;

namespace InternshipManagement.Api.Helpers;

public static class ConnectionStringHelper
{
    // Render's managed Postgres (and most other cloud Postgres providers) hand out a
    // connection string as a URI - postgres://user:pass@host:port/db - which Npgsql's
    // UseNpgsql doesn't parse directly (it expects its own Host=...;Port=...;... keyword
    // format). Local dev and Docker Compose already supply that keyword format
    // directly, so anything not starting with postgres(ql):// passes through unchanged.
    // SslMode.Require is added only on the URI path, since that's specifically the
    // shape a cloud provider's connection string takes, and those almost always
    // require an encrypted connection. See docs/DECISIONS.md D22.
    public static string ToNpgsqlConnectionString(string value)
    {
        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require
        };
        return builder.ConnectionString;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace ReadingSpot.Data;

// אותו מודל בדיוק כמו AppDbContext, רק עבור PostgreSQL (הענן – Render).
// מחלקה נפרדת כדי שלכל בסיס נתונים יהיו Migrations משלו (Migrations/Postgres).
public class PostgresDbContext(DbContextOptions<PostgresDbContext> options) : AppDbContext(options)
{
    // Render נותן כתובת בפורמט postgresql://user:pass@host:port/db – Npgsql צריך key=value.
    public static string NormalizeConnectionString(string value)
    {
        if (!value.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
            return value;

        var uri = new Uri(value);
        var user = uri.UserInfo.Split(':', 2);

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(user[0]),
            Password = user.Length > 1 ? Uri.UnescapeDataString(user[1]) : null,
            SslMode = SslMode.Prefer,
        }.ToString();
    }
}

// משמש רק את dotnet ef ליצירת Migrations של PostgreSQL (לא מתחבר לבסיס נתונים).
public class PostgresDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PostgresDbContext>
{
    public PostgresDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql("Host=localhost;Database=readingspot;Username=postgres;Password=postgres")
            .Options);
}

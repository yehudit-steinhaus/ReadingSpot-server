using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReadingSpot.Data;

// משמש רק את dotnet ef ליצירת Migrations. בזמן ריצה ה-connection string מגיע מ-User Secrets.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ReadingSpot;Trusted_Connection=True")
            .Options);
}

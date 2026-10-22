using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // לשימוש PostgresDbContext – אותו מודל, Migrations נפרדים.
    protected AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<StudySpot> StudySpots => Set<StudySpot>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Amenity> Amenities => Set<Amenity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    // כל ה-DateTime נשמרים ונקראים כ-UTC.
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        => builder.Properties<DateTime>().HaveConversion<UtcConverter>();

    // החלפת ה-Version – במקום אחד בלבד.
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<IVersioned>().Where(e => e.State == EntityState.Modified))
            e.Entity.Version = Guid.NewGuid();

        return base.SaveChangesAsync(ct);
    }

    private class UtcConverter() : ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
}

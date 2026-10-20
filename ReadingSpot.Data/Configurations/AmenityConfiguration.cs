using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadingSpot.Core.Entities;
using ReadingSpot.Data.Seed;

namespace ReadingSpot.Data.Configurations;

public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> b)
    {
        b.Property(a => a.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(a => a.Name).IsUnique();

        // קשר Many-to-Many עם טבלת קשר בשם SpotAmenities.
        b.HasMany(a => a.Spots)
         .WithMany(s => s.Amenities)
         .UsingEntity("SpotAmenities", j => j.HasData(SeedData.SpotAmenities));

        b.HasData(SeedData.Amenities);
    }
}

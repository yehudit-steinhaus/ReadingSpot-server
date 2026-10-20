using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadingSpot.Core.Entities;
using ReadingSpot.Data.Seed;

namespace ReadingSpot.Data.Configurations;

public class StudySpotConfiguration : IEntityTypeConfiguration<StudySpot>
{
    public void Configure(EntityTypeBuilder<StudySpot> b)
    {
        b.Property(s => s.Name).HasMaxLength(100).IsRequired();
        b.Property(s => s.Description).HasMaxLength(500);

        b.HasMany(s => s.TimeSlots)
         .WithOne(t => t.Spot)
         .HasForeignKey(t => t.StudySpotId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasData(SeedData.Spots);
    }
}

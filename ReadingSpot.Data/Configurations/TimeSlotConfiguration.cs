using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadingSpot.Core.Entities;

namespace ReadingSpot.Data.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> b)
    {
        // אין שני חלונות עם אותה שעת התחלה באותו אזור.
        b.HasIndex(t => new { t.StudySpotId, t.StartUtc }).IsUnique();

        // Version מוגדר כ-concurrency token ע"י [ConcurrencyCheck] ב-Core.

        b.HasMany(t => t.Bookings)
         .WithOne(bk => bk.TimeSlot)
         .HasForeignKey(bk => bk.TimeSlotId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadingSpot.Core.Entities;

namespace ReadingSpot.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.Property(bk => bk.Status).HasConversion<string>().HasMaxLength(20);

        // תומך בבדיקה "כבר יש למשתמשת הזמנה בחלון הזה".
        b.HasIndex(bk => new { bk.UserId, bk.TimeSlotId });
    }
}

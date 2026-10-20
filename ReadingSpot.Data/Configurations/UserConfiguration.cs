using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadingSpot.Core.Entities;
using ReadingSpot.Data.Seed;

namespace ReadingSpot.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        b.Property(u => u.Email).HasMaxLength(200).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
        b.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        b.HasMany(u => u.Bookings)
         .WithOne(bk => bk.User)
         .HasForeignKey(bk => bk.UserId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasData(SeedData.Users);
    }
}

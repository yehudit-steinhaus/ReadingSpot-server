using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Enums;
using ReadingSpot.Service.Mapping;

namespace ReadingSpot.Tests.Helpers;

// כל נתוני הבדיקה במקום אחד.
public static class TestData
{
    public const int MemberId = 2;
    public const int OtherMemberId = 3;
    public const int SlotId = 10;

    // אותו MappingProfile שרץ בייצור, דרך אותו רישום DI.
    public static IMapper Mapper { get; } = new ServiceCollection()
        .AddLogging()
        .AddAutoMapper(typeof(MappingProfile))
        .BuildServiceProvider()
        .GetRequiredService<IMapper>();

    public static CurrentUser Member => new(MemberId, IsAdmin: false);

    public static StudySpot Spot(int capacity = 2, bool active = true)
        => new() { Id = 1, Name = "חדר שקט", Capacity = capacity, IsActive = active };

    public static TimeSlot Slot(int capacity = 2, int booked = 0, bool active = true, int hoursFromNow = 24)
    {
        var spot = Spot(capacity, active);
        var start = DateTime.UtcNow.AddHours(hoursFromNow);
        return new TimeSlot
        {
            Id = SlotId, StudySpotId = spot.Id, Spot = spot,
            StartUtc = start, EndUtc = start.AddHours(2), BookedCount = booked,
        };
    }

    public static Booking Booking(int userId, TimeSlot slot) => new()
    {
        Id = 100,
        UserId = userId,
        User = new User { Id = userId, FullName = "משתמשת", Email = $"u{userId}@test.com" },
        TimeSlotId = slot.Id,
        TimeSlot = slot,
    };

    public static User UserWithPassword(string password) => new()
    {
        Id = MemberId,
        FullName = "משתמשת",
        Email = "member@test.com",
        Role = Role.Member,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4),
    };
}

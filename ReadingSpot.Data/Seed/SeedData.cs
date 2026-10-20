using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Enums;

namespace ReadingSpot.Data.Seed;

// נתונים קבועים ל-HasData. ה-hash מחושב מראש (BCrypt) כדי שה-Migration יהיה דטרמיניסטי.
// admin@readingspot.local / Admin123!   ·   member@readingspot.local / Member123!
internal static class SeedData
{
    public static readonly User[] Users =
    [
        new() { Id = 1, FullName = "מנהלת הספרייה", Email = "admin@readingspot.local", Role = Role.Admin,
                PasswordHash = "$2b$11$UySZcFhWwl/GZbbD6PqvROP7kPXhoIdgHy2bcsxN9zixDI5.UMAqq" },
        new() { Id = 2, FullName = "משתמשת לדוגמה", Email = "member@readingspot.local", Role = Role.Member,
                PasswordHash = "$2b$11$TtnjGLwK3ju7RQrphGJcMedh6A5owJNmIECkDLGWiPyx4kU2yyXTe" },
    ];

    public static readonly StudySpot[] Spots =
    [
        new() { Id = 1, Name = "חדר שקט", Description = "שקט מוחלט, לימוד יחידני", Capacity = 12 },
        new() { Id = 2, Name = "אזור קבוצתי", Description = "שולחנות לעבודה בקבוצות", Capacity = 8 },
        new() { Id = 3, Name = "עמדות מחשב", Description = "מחשבים עם גישה למאגרי הספרייה", Capacity = 4 },
        new() { Id = 4, Name = "פינת חלון", Description = "שני מקומות ליד החלון", Capacity = 2 },
    ];

    public static readonly Amenity[] Amenities =
    [
        new() { Id = 1, Name = "שקע חשמל" },
        new() { Id = 2, Name = "שקט מוחלט" },
        new() { Id = 3, Name = "חלון" },
        new() { Id = 4, Name = "מחשב" },
    ];

    // שורות לטבלת הקשר SpotAmenities (מפתחות לפי מוסכמת EF: SpotsId, AmenitiesId).
    public static readonly object[] SpotAmenities =
    [
        new { SpotsId = 1, AmenitiesId = 1 }, new { SpotsId = 1, AmenitiesId = 2 },
        new { SpotsId = 2, AmenitiesId = 1 },
        new { SpotsId = 3, AmenitiesId = 1 }, new { SpotsId = 3, AmenitiesId = 4 },
        new { SpotsId = 4, AmenitiesId = 2 }, new { SpotsId = 4, AmenitiesId = 3 },
    ];
}

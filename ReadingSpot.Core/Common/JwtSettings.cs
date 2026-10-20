namespace ReadingSpot.Core.Common;

// נטען מ-appsettings (Issuer, Audience) ומ-User Secrets (Key) דרך IOptions<JwtSettings>.
public class JwtSettings
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 120;
}

// שמות ה-claims ב-token – מוגדרים פעם אחת, משמשים גם ביצירה וגם בקריאה.
public static class AppClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Role = "role";
}

using System.Security.Claims;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.Enums;
using ReadingSpot.Core.Exceptions;

namespace ReadingSpot.API.Extensions;

// פענוח זהות המשתמשת מה-token – במקום אחד בלבד.
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirstValue(AppClaims.UserId), out var id)
            ? id
            : throw new UnauthorizedException("נדרשת התחברות.");

    public static CurrentUser GetCurrentUser(this ClaimsPrincipal user)
        => new(user.GetUserId(), user.IsInRole(nameof(Role.Admin)));
}

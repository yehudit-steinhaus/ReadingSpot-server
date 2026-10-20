using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.Service.Auth;

public class JwtTokenGenerator(IOptions<JwtSettings> options) : IJwtTokenGenerator
{
    public string Generate(User user)
    {
        var settings = options.Value;

        Claim[] claims =
        [
            new(AppClaims.UserId, user.Id.ToString()),
            new(AppClaims.Email, user.Email),
            new(AppClaims.Role, user.Role.ToString()),
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

using AutoMapper;
using Microsoft.Extensions.Logging;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Enums;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Core.Interfaces;

namespace ReadingSpot.Service.Auth;

public class AuthService(
    IRepository<User> users,
    IJwtTokenGenerator jwt,
    IMapper mapper,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        var email = Normalize(dto.Email);

        if (await users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("כתובת המייל כבר רשומה במערכת.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Role.Member,
        };

        users.Add(user);
        await users.SaveChangesAsync(ct);

        return ToResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var email = Normalize(dto.Email);
        var user = await users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            // רק המייל נרשם – לעולם לא הסיסמה.
            logger.LogWarning("Failed login attempt for {Email}", email);
            throw new UnauthorizedException("מייל או סיסמה שגויים.");
        }

        return ToResponse(user);
    }

    private AuthResponseDto ToResponse(User user)
    {
        var response = mapper.Map<AuthResponseDto>(user);
        response.Token = jwt.Generate(user);
        return response;
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}

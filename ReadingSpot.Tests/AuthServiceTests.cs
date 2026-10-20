using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReadingSpot.Core.DTOs;
using ReadingSpot.Core.Entities;
using ReadingSpot.Core.Exceptions;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Service.Auth;
using ReadingSpot.Tests.Helpers;

namespace ReadingSpot.Tests;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _users = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var jwt = new Mock<IJwtTokenGenerator>();
        jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns("test-token");
        _service = new AuthService(_users.Object, jwt.Object, TestData.Mapper, NullLogger<AuthService>.Instance);
    }

    private void GivenUser(User? user)
        => _users.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        GivenUser(TestData.UserWithPassword("Correct123!"));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginDto { Email = "member@test.com", Password = "Wrong123!" }, default));
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        GivenUser(TestData.UserWithPassword("Correct123!"));

        var result = await _service.LoginAsync(new LoginDto { Email = "MEMBER@test.com ", Password = "Correct123!" }, default);

        Assert.Equal("test-token", result.Token);
        Assert.Equal(TestData.MemberId, result.UserId);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsConflict_AndDoesNotAdd()
    {
        _users.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(
            new RegisterDto { FullName = "בדיקה", Email = "member@test.com", Password = "Test123!" }, default));

        _users.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }
}

using API.Exceptions;
using API.Services.Services;
using API.Utility;
using CloudKeep.Tests.Support;
using HostedService.Entities;

namespace CloudKeep.Tests.Services;

public class LoginServiceTests
{
    [Fact]
    public async Task LoginUser_throws_when_no_user()
    {
        using var db = new TestAppDatabase();
        var sut = new LoginService(db.Context);
        await Assert.ThrowsAsync<InternalServerException>(() => sut.LoginUser("any"));
    }

    [Fact]
    public async Task LoginUser_returns_jwt_when_password_matches()
    {
        using var db = new TestAppDatabase();
        db.Context.Users.Add(new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword1!") });
        await db.Context.SaveChangesAsync();

        var sut = new LoginService(db.Context);
        var token = await sut.LoginUser("TestPassword1!");
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(await sut.isTokenValid(token));
    }

    [Fact]
    public async Task LoginUser_throws_when_password_wrong()
    {
        using var db = new TestAppDatabase();
        db.Context.Users.Add(new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword1!") });
        await db.Context.SaveChangesAsync();

        var sut = new LoginService(db.Context);
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.LoginUser("WrongPassword1!"));
    }

    [Fact]
    public async Task ChangePassword_updates_hash()
    {
        using var db = new TestAppDatabase();
        db.Context.Users.Add(new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword1!") });
        await db.Context.SaveChangesAsync();

        var sut = new LoginService(db.Context);
        await sut.ChangePassword("OldPassword1!", "NewPassword1!x");

        var user = db.Context.Users.Single();
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword1!x", user.PasswordHash));
    }

    [Fact]
    public async Task ChangePassword_rejects_same_as_old()
    {
        using var db = new TestAppDatabase();
        db.Context.Users.Add(new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("SamePassword1!") });
        await db.Context.SaveChangesAsync();

        var sut = new LoginService(db.Context);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.ChangePassword("SamePassword1!", "SamePassword1!"));
    }

    [Fact]
    public async Task isTokenValid_returns_false_for_garbage()
    {
        using var db = new TestAppDatabase();
        var sut = new LoginService(db.Context);
        Assert.False(await sut.isTokenValid("not-a-jwt"));
    }
}

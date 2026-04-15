using API.Exceptions;
using API.Utility;

namespace CloudKeep.Tests;

public class PasswordPolicyTests
{
    [Fact]
    public void ValidateNewPassword_accepts_strong_password()
    {
        var ex = Record.Exception(() => PasswordPolicy.ValidateNewPassword("Aa1!aaaaaaaa"));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateNewPassword_rejects_empty(string? password)
    {
        var ex = Assert.Throws<BadRequestException>(() => PasswordPolicy.ValidateNewPassword(password));
        Assert.Contains("obligatoria", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateNewPassword_rejects_too_short()
    {
        var ex = Assert.Throws<BadRequestException>(() => PasswordPolicy.ValidateNewPassword("Aa1!aaaaaaa"));
        Assert.Contains($"{PasswordPolicy.MinLength}", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateNewPassword_rejects_missing_uppercase()
    {
        var ex = Assert.Throws<BadRequestException>(() => PasswordPolicy.ValidateNewPassword("aa1!aaaaaaaa"));
        Assert.Contains("mayúscula", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateNewPassword_rejects_missing_lowercase()
    {
        var ex = Assert.Throws<BadRequestException>(() => PasswordPolicy.ValidateNewPassword("AA1!AAAAAAAA"));
        Assert.Contains("minúscula", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateNewPassword_rejects_missing_digit()
    {
        var ex = Assert.Throws<BadRequestException>(() => PasswordPolicy.ValidateNewPassword("Aa!aaaaaaaaaa"));
        Assert.Contains("dígito", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateNewPassword_rejects_missing_special()
    {
        var ex = Assert.Throws<BadRequestException>(() => PasswordPolicy.ValidateNewPassword("Aa1aaaaaaaaaa"));
        Assert.Contains("símbolo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateNewPassword_rejects_too_long()
    {
        var tooLong = new string('a', PasswordPolicy.MaxLength - 2) + "A1!";
        Assert.True(tooLong.Length > PasswordPolicy.MaxLength);
        var ex = Assert.Throws<BadRequestException>(() => PasswordPolicy.ValidateNewPassword(tooLong));
        Assert.Contains($"{PasswordPolicy.MaxLength}", ex.Message, StringComparison.Ordinal);
    }
}

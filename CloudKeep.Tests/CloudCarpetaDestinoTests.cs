using API;
using API.Exceptions;

namespace CloudKeep.Tests;

public class CloudCarpetaDestinoTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("a", "a/")]
    [InlineData("a/", "a/")]
    [InlineData("/a/b", "a/b/")]
    [InlineData(@"x\y", "x/y/")]
    [InlineData("  backups  ", "backups/")]
    public void NormalizePrefijo_returns_expected_prefix(string? input, string expected)
    {
        Assert.Equal(expected, CloudCarpetaDestino.NormalizePrefijo(input));
    }

    [Theory]
    [InlineData("a/./b")]
    [InlineData("a/../b")]
    [InlineData("..")]
    [InlineData(".")]
    public void NormalizePrefijo_rejects_dot_segments(string input)
    {
        var ex = Assert.Throws<BadRequestException>(() => CloudCarpetaDestino.NormalizePrefijo(input));
        Assert.Contains("carpetaDestino", ex.Message, StringComparison.Ordinal);
    }
}

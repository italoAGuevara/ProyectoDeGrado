namespace HostedService.Tests;

public class DestinoTiposTests
{
    [Fact]
    public void Allowed_contains_known_destinations()
    {
        Assert.Contains(DestinoTipos.S3, DestinoTipos.Allowed);
        Assert.Contains(DestinoTipos.GoogleDrive, DestinoTipos.Allowed);
        Assert.Contains(DestinoTipos.AzureBlob, DestinoTipos.Allowed);
        Assert.Equal(3, DestinoTipos.Allowed.Count);
    }

    [Theory]
    [InlineData("S3", true)]
    [InlineData("s3", false)]
    [InlineData("AzureBlob", true)]
    [InlineData("azureblob", false)]
    [InlineData("unknown", false)]
    public void Allowed_uses_ordinal_case_sensitive_lookup(string value, bool expected)
    {
        Assert.Equal(expected, DestinoTipos.Allowed.Contains(value));
    }
}

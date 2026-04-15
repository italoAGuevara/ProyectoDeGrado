using API.Services.Services;
using Microsoft.AspNetCore.DataProtection;

namespace CloudKeep.Tests.Services;

public class DestinoCredentialProtectorTests
{
    [Fact]
    public void Protect_and_Unprotect_roundtrip()
    {
        var provider = new EphemeralDataProtectionProvider();
        var sut = new DestinoCredentialProtector(provider);
        const string plain = "secret-value-123";

        var enc = sut.Protect(plain);
        Assert.NotEqual(plain, enc);

        var back = sut.Unprotect(enc);
        Assert.Equal(plain, back);
    }
}

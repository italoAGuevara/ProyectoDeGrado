using API.Services.Interfaces;
using API.Services.Services;
using Microsoft.AspNetCore.DataProtection;
using Moq;

namespace CloudKeep.Tests.Support;

internal static class ServiceTestHarness
{
    public static Mock<ILogAccionesUsuarioWriter> CreateLogMock()
    {
        var m = new Mock<ILogAccionesUsuarioWriter>();
        m.Setup(x => x.RegistrarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return m;
    }

    public static DestinoCredentialProtector CreateCredentialProtector()
    {
        var provider = new EphemeralDataProtectionProvider();
        return new DestinoCredentialProtector(provider);
    }
}

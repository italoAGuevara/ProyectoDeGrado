using API.Services.Services;
using CloudKeep.Tests.Support;
using HostedService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace CloudKeep.Tests.Services;

public class LogAccionesUsuarioWriterTests
{
    [Fact]
    public async Task RegistrarAsync_persists_row_with_sub_from_http_context()
    {
        using var db = new TestAppDatabase();
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "tester-1")
                ]))
            }
        };
        var sut = new LogAccionesUsuarioWriter(db.Context, accessor, NullLogger<LogAccionesUsuarioWriter>.Instance);

        await sut.RegistrarAsync("Origen", "CREATE", null, new { x = 1 });

        var row = db.Context.LogAccionesUsuario.Single();
        Assert.Equal("Origen", row.TablaAfectada);
        Assert.Contains("tester-1", row.ValorNuevo, StringComparison.Ordinal);
    }
}

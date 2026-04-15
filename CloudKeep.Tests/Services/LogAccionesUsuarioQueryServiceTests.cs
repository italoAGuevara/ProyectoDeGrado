using API.Services.Services;
using CloudKeep.Tests.Support;
using HostedService.Entities;

namespace CloudKeep.Tests.Services;

public class LogAccionesUsuarioQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_orders_newest_first_and_caps_limit()
    {
        using var db = new TestAppDatabase();
        var ctx = db.Context;
        var t0 = DateTime.UtcNow.AddMinutes(-10);
        for (var i = 0; i < 3; i++)
        {
            ctx.LogAccionesUsuario.Add(new LogAccionesUsuario
            {
                Id = Guid.NewGuid(),
                FechaAccion = t0.AddMinutes(i),
                TablaAfectada = "T",
                Accion = "A",
                ValorAnterior = "",
                ValorNuevo = ""
            });
        }

        await ctx.SaveChangesAsync();

        var sut = new LogAccionesUsuarioQueryService(ctx);
        var list = await sut.ListarAsync(limite: 2);
        Assert.Equal(2, list.Count);
        Assert.True(list[0].FechaAccion >= list[1].FechaAccion);
    }

    [Fact]
    public async Task ListarAsync_uses_default_when_limit_invalid()
    {
        using var db = new TestAppDatabase();
        var sut = new LogAccionesUsuarioQueryService(db.Context);
        var list = await sut.ListarAsync(limite: null);
        Assert.Empty(list);
    }
}

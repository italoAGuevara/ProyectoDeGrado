using API.DTOs;
using API.Exceptions;
using API.Services.Services;
using CloudKeep.Tests.Support;
using HostedService.Entities;
using Infrastructure.Persistence;

namespace CloudKeep.Tests.Services;

public class TrabajoServiceTests
{
    [Fact]
    public async Task GetById_throws_not_found()
    {
        using var db = new TestAppDatabase();
        var sut = new TrabajoService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetById(7));
    }

    [Fact]
    public async Task Create_throws_when_origen_missing()
    {
        using var db = new TestAppDatabase();
        var sut = new TrabajoService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        var req = new CreateTrabajoRequest(
            "T", "D", 1, 1, null, null, false, false, "0 * * * *", true);
        await Assert.ThrowsAsync<BadRequestException>(() => sut.Create(req));
    }

    [Fact]
    public async Task Create_succeeds_with_origen_destino_and_script_bundle()
    {
        using var db = new TestAppDatabase();
        var ctx = db.Context;
        ctx.Origenes.Add(new Origen { Nombre = "o", Ruta = "C:\\o", Descripcion = "d" });
        var protector = ServiceTestHarness.CreateCredentialProtector();
        ctx.Destinos.Add(new Destino
        {
            Nombre = "d",
            TipoDeDestino = HostedService.DestinoTipos.S3,
            Credenciales = protector.Protect("{}"),
            BucketName = "b",
            S3Region = "us-east-1",
            CarpetaDestino = "p/"
        });
        await ctx.SaveChangesAsync();

        var sut = new TrabajoService(ctx, ServiceTestHarness.CreateLogMock().Object);
        var res = await sut.Create(new CreateTrabajoRequest(
            "Job 1",
            "Desc",
            ctx.Origenes.Single().Id,
            ctx.Destinos.Single().Id,
            null,
            null,
            false,
            false,
            "0 * * * *",
            true));

        Assert.True(res.Id > 0);
        Assert.Equal(ctx.Origenes.Single().Id, res.OrigenId);
        Assert.Equal(ctx.Destinos.Single().Id, res.DestinoId);
    }

    [Fact]
    public async Task Update_throws_when_only_one_of_origen_destino_pair_sent()
    {
        using var db = new TestAppDatabase();
        var ctx = db.Context;
        SeedMinimalTrabajo(ctx);
        await ctx.SaveChangesAsync();
        var trabajoId = ctx.Trabajos.Single().Id;

        var sut = new TrabajoService(ctx, ServiceTestHarness.CreateLogMock().Object);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.Update(trabajoId, new UpdateTrabajoRequest(
                null, null, 1, null,
                null, null, null, null,
                null, null, null, null,
                null)));
    }

    [Fact]
    public async Task Delete_returns_false_when_missing()
    {
        using var db = new TestAppDatabase();
        var sut = new TrabajoService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        Assert.False(await sut.Delete(999));
    }

    private static void SeedMinimalTrabajo(AppDbContext ctx)
    {
        ctx.Origenes.Add(new Origen { Nombre = "o", Ruta = "C:\\o", Descripcion = "d" });
        ctx.Destinos.Add(new Destino
        {
            Nombre = "d",
            TipoDeDestino = HostedService.DestinoTipos.S3,
            Credenciales = string.Empty,
            BucketName = "b",
            S3Region = "r",
            CarpetaDestino = "p/"
        });
        ctx.SaveChanges();
        var o = ctx.Origenes.Single();
        var d = ctx.Destinos.Single();
        var link = new TrabajosOrigenDestino { OrigenId = o.Id, DestinoId = d.Id };
        ctx.TrabajosOrigenDestinos.Add(link);
        ctx.SaveChanges();
        var scripts = new TrabajoScripts();
        ctx.TrabajosScripts.Add(scripts);
        ctx.SaveChanges();
        ctx.Trabajos.Add(new Trabajo
        {
            Nombre = "j",
            Descripcion = "d",
            TrabajosOrigenDestinoId = link.Id,
            TrabajosScriptsId = scripts.Id,
            CronExpression = "0 * * * *"
        });
    }
}

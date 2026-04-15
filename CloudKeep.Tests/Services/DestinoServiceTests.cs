using API.DTOs;
using API.Exceptions;
using API.Services.Services;
using CloudKeep.Tests.Support;
using HostedService;
using HostedService.Entities;

namespace CloudKeep.Tests.Services;

public class DestinoServiceTests
{
    [Fact]
    public async Task GetById_throws_not_found()
    {
        using var db = new TestAppDatabase();
        var sut = new DestinoService(
            db.Context,
            ServiceTestHarness.CreateCredentialProtector(),
            ServiceTestHarness.CreateLogMock().Object);
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetById(42));
    }

    [Fact]
    public async Task Create_S3_IAM_persists_without_remote_validation()
    {
        using var db = new TestAppDatabase();
        var protector = ServiceTestHarness.CreateCredentialProtector();
        var sut = new DestinoService(db.Context, protector, ServiceTestHarness.CreateLogMock().Object);
        var req = new CreateDestinoRequest(
            Nombre: "S3 dest",
            Tipo: DestinoTipos.S3,
            BucketName: "my-bucket",
            Region: "us-east-1",
            CarpetaDestino: "backups/app");

        var res = await sut.Create(req);

        Assert.True(res.Id > 0);
        Assert.Equal(DestinoTipos.S3, res.Tipo);
        Assert.Equal("my-bucket", res.BucketName);
        Assert.Equal("backups/app/", res.CarpetaDestino);
    }

    [Fact]
    public async Task Create_throws_on_invalid_tipo()
    {
        using var db = new TestAppDatabase();
        var sut = new DestinoService(
            db.Context,
            ServiceTestHarness.CreateCredentialProtector(),
            ServiceTestHarness.CreateLogMock().Object);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.Create(new CreateDestinoRequest("x", "FTP", CarpetaDestino: "a/")));
    }

    [Fact]
    public async Task Delete_throws_conflict_when_trabajo_uses_destino()
    {
        using var db = new TestAppDatabase();
        var ctx = db.Context;
        var protector = ServiceTestHarness.CreateCredentialProtector();
        var log = ServiceTestHarness.CreateLogMock();
        var destinoService = new DestinoService(ctx, protector, log.Object);
        var created = await destinoService.Create(new CreateDestinoRequest(
            "d1", DestinoTipos.S3, BucketName: "b", Region: "us-east-1", CarpetaDestino: "p/"));

        var origen = new Origen { Nombre = "o", Ruta = "C:\\o", Descripcion = "d" };
        ctx.Origenes.Add(origen);
        await ctx.SaveChangesAsync();

        var link = new TrabajosOrigenDestino { OrigenId = origen.Id, DestinoId = created.Id };
        ctx.TrabajosOrigenDestinos.Add(link);
        await ctx.SaveChangesAsync();

        var scripts = new TrabajoScripts();
        ctx.TrabajosScripts.Add(scripts);
        await ctx.SaveChangesAsync();

        ctx.Trabajos.Add(new Trabajo
        {
            Nombre = "job",
            Descripcion = "d",
            TrabajosOrigenDestinoId = link.Id,
            TrabajosScriptsId = scripts.Id,
            CronExpression = "0 * * * *"
        });
        await ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => destinoService.Delete(created.Id));
    }
}

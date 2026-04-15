using API.DTOs;
using API.Exceptions;
using API.Services.Services;
using CloudKeep.Tests.Support;

namespace CloudKeep.Tests.Services;

public class ScriptsServiceTests
{
    [Fact]
    public async Task GetById_throws_not_found()
    {
        using var db = new TestAppDatabase();
        var sut = new ScriptsService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetById(999));
    }

    [Fact]
    public async Task Create_throws_when_file_missing()
    {
        using var db = new TestAppDatabase();
        var sut = new ScriptsService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        var missing = Path.Combine(Path.GetTempPath(), $"no_script_{Guid.NewGuid():N}.ps1");
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.Create(new CreateScriptRequest("N", missing, "", ".ps1")));
    }

    [Fact]
    public async Task Create_throws_on_invalid_tipo()
    {
        using var db = new TestAppDatabase();
        var sut = new ScriptsService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        var path = Path.Combine(Path.GetTempPath(), $"noop_{Guid.NewGuid():N}.ps1");
        await File.WriteAllTextAsync(path, "#");
        try
        {
            await Assert.ThrowsAsync<BadRequestException>(() =>
                sut.Create(new CreateScriptRequest("N", path, "", ".exe")));
        }
        finally
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task Create_persists_when_file_exists()
    {
        using var db = new TestAppDatabase();
        var sut = new ScriptsService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        var path = Path.Combine(Path.GetTempPath(), $"ok_{Guid.NewGuid():N}.ps1");
        await File.WriteAllTextAsync(path, "# test");
        try
        {
            var res = await sut.Create(new CreateScriptRequest("Script A", path, "-x", ".PS1"));
            Assert.True(res.Id > 0);
            Assert.Equal(".ps1", res.Tipo);
        }
        finally
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task Delete_returns_false_when_missing()
    {
        using var db = new TestAppDatabase();
        var sut = new ScriptsService(db.Context, ServiceTestHarness.CreateLogMock().Object);
        Assert.False(await sut.Delete(404));
    }
}

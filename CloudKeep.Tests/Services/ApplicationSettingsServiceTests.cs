using API.Exceptions;
using API.Services.Services;
using CloudKeep.Tests.Support;
using HostedService.Entities;

namespace CloudKeep.Tests.Services;

public class ApplicationSettingsServiceTests
{
    [Fact]
    public async Task GetScriptExecutionTimeoutMinutesAsync_returns_zero_when_missing_or_below_min()
    {
        using var db = new TestAppDatabase();
        var sut = new ApplicationSettingsService(db.Context);

        Assert.Equal(0, await sut.GetScriptExecutionTimeoutMinutesAsync());

        db.Context.ApplicationSettings.Add(new ApplicationSettings
        {
            Id = ApplicationSettings.SingletonId,
            ScriptExecutionTimeoutMinutes = 0
        });
        await db.Context.SaveChangesAsync();

        Assert.Equal(0, await sut.GetScriptExecutionTimeoutMinutesAsync());
    }

    [Fact]
    public async Task Set_and_Get_roundtrip_clamps_to_bounds()
    {
        using var db = new TestAppDatabase();
        var sut = new ApplicationSettingsService(db.Context);

        await sut.SetScriptExecutionTimeoutMinutesAsync(30);
        Assert.Equal(30, await sut.GetScriptExecutionTimeoutMinutesAsync());

        db.Context.ApplicationSettings.RemoveRange(db.Context.ApplicationSettings);
        await db.Context.SaveChangesAsync();
        await sut.SetScriptExecutionTimeoutMinutesAsync(ApplicationSettingsService.MaxScriptTimeoutMinutes);
        Assert.Equal(ApplicationSettingsService.MaxScriptTimeoutMinutes, await sut.GetScriptExecutionTimeoutMinutesAsync());
    }

    [Fact]
    public async Task SetScriptExecutionTimeoutMinutesAsync_rejects_out_of_range()
    {
        using var db = new TestAppDatabase();
        var sut = new ApplicationSettingsService(db.Context);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.SetScriptExecutionTimeoutMinutesAsync(ApplicationSettingsService.MinScriptTimeoutMinutes - 1));
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.SetScriptExecutionTimeoutMinutesAsync(ApplicationSettingsService.MaxScriptTimeoutMinutes + 1));
    }
}

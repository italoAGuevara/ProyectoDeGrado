using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence;

/// <summary>
/// Permite a las herramientas de EF Core (<c>dotnet ef</c>) crear <see cref="AppDbContext"/>
/// sin arrancar la API cuando el proyecto de inicio es Infrastructure.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=mibase.db");
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
        return new AppDbContext(optionsBuilder.Options);
    }
}

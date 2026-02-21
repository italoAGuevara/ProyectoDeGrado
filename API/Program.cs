using API;
using API.Features.Login;
using API.Features.Login.Entities;
using API.Features.Settings;
using API.Middleware;
using HostedService;
using HostedService.Entities;
using HostedService.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;


string _cors = "all";

var angularPath = Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\italo\\Documents\\proyecto-grado\\ProyectoDeGrado\\Interfaz\\dist\\Interfaz\\browser");

var builder = WebApplication.CreateSlimBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()    
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "tu-app.com",
            ValidAudience = "tu-app.com",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("Tu_Llave_Super_Secreta_De_Al_Menos_32_Chars!"))
        };
    });
builder.Services.AddAuthorization();

//builder.Services.ConfigureHttpJsonOptions(options =>
//{
//    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
//});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<AppDbContext>();


builder.Services.AddCors(options =>
{
    options.AddPolicy(_cors, builder =>
      builder.SetIsOriginAllowed(origin =>
      {
          var uri = new Uri(origin);
          return uri.Host == "localhost" ||
                  uri.Host == "localhost:4200";
      })
    .AllowAnyMethod()
    .AllowAnyHeader());
});


builder.Services.AddHostedService<Robot>();

var app = builder.Build();

// Asegurar usuario único y datos de ejemplo (orígenes, scripts, jobs)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.Add(new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"), RequirePassword = true });
        db.SaveChanges();
    }

    if (!db.Origenes.Any())
    {
        db.Origenes.AddRange(
            new Origen { Name = "Documentos", Path = "C:\\Users\\Usuario\\Documents", Description = "Carpeta de documentos del usuario" },
            new Origen { Name = "Escritorio", Path = "C:\\Users\\Usuario\\Desktop", Description = "Escritorio" },
            new Origen { Name = "Proyectos", Path = "C:\\Proyectos", Description = "Carpeta de proyectos" }
        );
        db.SaveChanges();
    }

    if (!db.StorageProviders.Any())
    {
        db.StorageProviders.AddRange(
            new StorageProvider { Name = "Local", ConfigJsonSchema = "{}" },
            new StorageProvider { Name = "S3", ConfigJsonSchema = "{\"bucket\":\"\",\"region\":\"\"}" }
        );
        db.SaveChanges();
    }

    if (!db.UserStorages.Any())
    {
        var user = db.Users.AsNoTracking().First();
        db.UserStorages.AddRange(
            new UserStorages { IdUser = user.Id, CloudDestination = "Carpeta local respaldos", CredentialJson = "{\"path\":\"D:\\\\Backups\"}" },
            new UserStorages { IdUser = user.Id, CloudDestination = "S3 principal", CredentialJson = "{\"bucket\":\"mi-bucket\",\"region\":\"us-east-1\"}" }
        );
        db.SaveChanges();
    }

    if (!db.ScriptConfigurations.Any())
    {
        db.ScriptConfigurations.AddRange(
            new ScriptConfiguration { Name = "Notificar inicio", ScriptPath = "C:\\Scripts\\notify_start.ps1", Arguments = "", Trigger = ScriptTrigger.PreBackup, StopOnFailure = false, TimeoutMinutes = 2 },
            new ScriptConfiguration { Name = "Limpiar temporales", ScriptPath = "C:\\Scripts\\clean_temp.ps1", Arguments = "", Trigger = ScriptTrigger.PreBackup, StopOnFailure = false, TimeoutMinutes = 5 },
            new ScriptConfiguration { Name = "Notificar fin", ScriptPath = "C:\\Scripts\\notify_end.ps1", Arguments = "", Trigger = ScriptTrigger.PostBackup, StopOnFailure = false, TimeoutMinutes = 2 }
        );
        db.SaveChanges();
    }

    if (!db.BackupJobs.Any())
    {
        var origenDoc = db.Origenes.First(o => o.Name == "Documentos");
        var destinoLocal = db.UserStorages.First(u => u.CloudDestination == "Carpeta local respaldos");
        var scriptPre = db.ScriptConfigurations.First(s => s.Name == "Notificar inicio");
        var scriptPost = db.ScriptConfigurations.First(s => s.Name == "Notificar fin");

        var job = new BackupJob
        {
            Name = "Backup diario documentos",
            Description = "Respaldo de la carpeta Documentos a la carpeta local de respaldos.",
            UserStorageId = destinoLocal.Id,
            OrigenId = origenDoc.Id,
            CronExpression = "0 2 * * *",
            IsActive = true
        };
        db.BackupJobs.Add(job);
        db.SaveChanges();

        db.relationJobsAndScripts.Add(new RelationJobsAndScript { JobId = job.Id, ScriptId = scriptPre.Id, ExecutionOrder = 1, Pre = true, Post = false });
        db.relationJobsAndScripts.Add(new RelationJobsAndScript { JobId = job.Id, ScriptId = scriptPost.Id, ExecutionOrder = 2, Pre = false, Post = true });
        db.SaveChanges();
    }
}

app.UseCors(_cors);

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();

// LOAD STATIC FILES
app.UseDefaultFiles(); // Busca index.html por defecto
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(angularPath),
    RequestPath = ""
});

// RETURN index.html for any non-API route to allow Angular routing to work
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(angularPath)
});

app.UseMiddleware<ResponseWrapperMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapLogin();
app.MapValidateJwt();
app.MapChangePassword();
app.MapSettings();

app.Run();

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down");
    Log.CloseAndFlush();
}
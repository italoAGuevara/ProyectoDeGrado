using API.Services.Interfaces;
using HostedService.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace API.Services.Services;

public class LogAccionesUsuarioWriter : ILogAccionesUsuarioWriter
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LogAccionesUsuarioWriter> _logger;

    public LogAccionesUsuarioWriter(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LogAccionesUsuarioWriter> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task RegistrarAsync(
        string tablaAfectada,
        string accion,
        object? valorAnterior,
        object? valorNuevo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var usuario = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "desconocido";

            var va = valorAnterior is null
                ? string.Empty
                : JsonSerializer.Serialize(new { usuario, datos = valorAnterior }, JsonOpts);

            var vn = valorNuevo is null
                ? string.Empty
                : JsonSerializer.Serialize(new { usuario, datos = valorNuevo }, JsonOpts);

            _context.LogAccionesUsuario.Add(new LogAccionesUsuario
            {
                Id = Guid.NewGuid(),
                FechaAccion = DateTime.UtcNow,
                TablaAfectada = tablaAfectada,
                Accion = accion,
                ValorAnterior = va,
                ValorNuevo = vn
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo registrar en LogAccionesUsuario ({Tabla}, {Accion})", tablaAfectada, accion);
        }
    }
}

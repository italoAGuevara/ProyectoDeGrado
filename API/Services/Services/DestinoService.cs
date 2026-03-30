using API.Audit;
using API.DTOs;
using API.Exceptions;
using API.Services.Interfaces;
using HostedService.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Services;

public class DestinoService : IDestinoService
{
    private readonly AppDbContext _context;
    private readonly IDestinoCredentialProtector _credentialProtector;
    private readonly ILogAccionesUsuarioWriter _logAcciones;

    public DestinoService(
        AppDbContext context,
        IDestinoCredentialProtector credentialProtector,
        ILogAccionesUsuarioWriter logAcciones)
    {
        _context = context;
        _credentialProtector = credentialProtector;
        _logAcciones = logAcciones;
    }

    public async Task<IEnumerable<DestinoResponse>> GetAll()
    {
        var list = await _context.Destinos
            .AsNoTracking()
            .OrderBy(d => d.Nombre)
            .ToListAsync();
        return list.Select(MapToResponse).OrderBy(x => x.Id);
    }

    public async Task<DestinoResponse?> GetById(int id)
    {
        var entity = await _context.Destinos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

        if (entity is null)
            throw new NotFoundException($"Destino con Id '{id}' no existe");

        return MapToResponse(entity);
    }

    public async Task<DestinoResponse> Create(CreateDestinoRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Credenciales, nameof(request.Credenciales));
        var tipo = NormalizeAndValidateTipo(request.Tipo);

        var nombre = request.Nombre.Trim();
        await EnsureNombreUnicoAsync(nombre, excludeId: null);

        var entity = new Destino
        {
            Nombre = nombre,
            TipoDeDestino = tipo,
            Credenciales = _credentialProtector.Protect(request.Credenciales.Trim())
        };
        _context.Destinos.Add(entity);
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Destino, AccionLog.Create, null, SnapshotDestino(entity));
        return MapToResponse(entity);
    }

    public async Task<DestinoResponse?> Update(int id, UpdateDestinoRequest request)
    {
        var entity = await _context.Destinos.FirstOrDefaultAsync(d => d.Id == id);
        if (entity is null) return null;

        var antes = SnapshotDestino(entity);

        if (request.Nombre is not null)
        {
            ValidateRequired(request.Nombre, nameof(request.Nombre));
            var nombre = request.Nombre.Trim();
            await EnsureNombreUnicoAsync(nombre, excludeId: id);
            entity.Nombre = nombre;
        }

        if (request.Tipo is not null)
            entity.TipoDeDestino = NormalizeAndValidateTipo(request.Tipo);

        if (request.Credenciales is not null)
        {
            ValidateRequired(request.Credenciales, nameof(request.Credenciales));
            entity.Credenciales = _credentialProtector.Protect(request.Credenciales.Trim());
        }

        entity.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Destino, AccionLog.Update, antes, SnapshotDestino(entity));
        return MapToResponse(entity);
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await _context.Destinos.FirstOrDefaultAsync(d => d.Id == id);
        if (entity is null) return false;
        var antes = SnapshotDestino(entity);
        _context.Destinos.Remove(entity);
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Destino, AccionLog.Delete, antes, null);
        return true;
    }

    /// <summary>No persiste credenciales; solo indica si había valor almacenado.</summary>
    private static object SnapshotDestino(Destino d) => new
    {
        d.Id,
        d.Nombre,
        d.TipoDeDestino,
        credencialesConfiguradas = !string.IsNullOrEmpty(d.Credenciales),
        d.FechaCreacion,
        d.FechaModificacion
    };

    private static DestinoResponse MapToResponse(Destino d) => new(
        d.Id,
        d.Nombre,
        d.TipoDeDestino,
        !string.IsNullOrEmpty(d.Credenciales),
        d.FechaCreacion,
        d.FechaModificacion
    );

    private async Task EnsureNombreUnicoAsync(string nombre, int? excludeId)
    {
        var exists = excludeId is null
            ? await _context.Destinos.AnyAsync(d => d.Nombre == nombre)
            : await _context.Destinos.AnyAsync(d => d.Nombre == nombre && d.Id != excludeId);

        if (exists)
            throw new ConflictException($"Ya existe un destino con el nombre '{nombre}'.");
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BadRequestException($"{fieldName} es obligatorio.");
    }

    private static string NormalizeAndValidateTipo(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new BadRequestException("tipo es obligatorio.");

        var t = tipo.Trim();
        if (!DestinoTipos.Allowed.Contains(t))
            throw new BadRequestException($"tipo debe ser '{DestinoTipos.S3}' o '{DestinoTipos.GoogleDrive}'.");

        return t;
    }
}

using API.Audit;
using API.DTOs;
using API.Exceptions;
using API.Services.Interfaces;
using HostedService.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Services;

public class TrabajoService : ITrabajoService
{
    private readonly AppDbContext _context;
    private readonly ILogAccionesUsuarioWriter _logAcciones;

    public TrabajoService(AppDbContext context, ILogAccionesUsuarioWriter logAcciones)
    {
        _context = context;
        _logAcciones = logAcciones;
    }

    public async Task<IEnumerable<TrabajoResponse>> GetAll()
    {
        var list = await _context.Trabajos
            .AsNoTracking()
            .Include(t => t.TrabajosOrigenDestino)
            .Include(t => t.TrabajosScripts)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
        return list.Select(MapToResponse).OrderBy(x => x.Id);
    }

    public async Task<TrabajoResponse?> GetById(int id)
    {
        var entity = await _context.Trabajos
            .AsNoTracking()
            .Include(t => t.TrabajosOrigenDestino)
            .Include(t => t.TrabajosScripts)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entity is null)
            throw new NotFoundException($"Trabajo con Id '{id}' no existe");

        return MapToResponse(entity);
    }

    public async Task<TrabajoResponse> Create(CreateTrabajoRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Descripcion, nameof(request.Descripcion));
        ValidateRequired(request.CronExpression, nameof(request.CronExpression));

        await EnsureOrigenExistsAsync(request.OrigenId);
        await EnsureDestinoExistsAsync(request.DestinoId);
        await EnsureScriptExistsAsync(request.ScriptPreId);
        await EnsureScriptExistsAsync(request.ScriptPostId);

        var linkId = await GetOrCreateTrabajosOrigenDestinoIdAsync(request.OrigenId, request.DestinoId);
        var scriptsId = await GetOrCreateTrabajoScriptsIdAsync(
            request.ScriptPreId,
            request.ScriptPostId,
            request.PreDetenerEnFallo ?? false,
            request.PostDetenerEnFallo ?? false);

        var entity = new Trabajo
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim(),
            TrabajosOrigenDestinoId = linkId,
            TrabajosScriptsId = scriptsId,
            CronExpression = request.CronExpression.Trim(),
            Activo = request.Activo ?? true
        };
        _context.Trabajos.Add(entity);
        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(t => t.TrabajosOrigenDestino).LoadAsync();
        await _context.Entry(entity).Reference(t => t.TrabajosScripts).LoadAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Trabajo, AccionLog.Create, null, SnapshotTrabajo(entity));
        return MapToResponse(entity);
    }

    public async Task<TrabajoResponse?> Update(int id, UpdateTrabajoRequest request)
    {
        var entity = await _context.Trabajos
            .Include(t => t.TrabajosOrigenDestino)
            .Include(t => t.TrabajosScripts)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return null;

        var antes = SnapshotTrabajo(entity);

        if (request.Nombre is not null)
        {
            ValidateRequired(request.Nombre, nameof(request.Nombre));
            entity.Nombre = request.Nombre.Trim();
        }

        if (request.Descripcion is not null)
            entity.Descripcion = request.Descripcion.Trim();

        if (request.CronExpression is not null)
        {
            ValidateRequired(request.CronExpression, nameof(request.CronExpression));
            entity.CronExpression = request.CronExpression.Trim();
        }

        if (request.Activo is not null)
            entity.Activo = request.Activo.Value;

        if (request.Procesando is not null)
            entity.Procesando = request.Procesando.Value;

        if (request.EstatusPrevio is not null)
            entity.EstatusPrevio = request.EstatusPrevio.Trim();

        var changePair = request.OrigenId is not null || request.DestinoId is not null;
        if (changePair)
        {
            if (request.OrigenId is null || request.DestinoId is null)
                throw new BadRequestException("origenId y destinoId deben enviarse juntos para cambiar el vínculo.");

            await EnsureOrigenExistsAsync(request.OrigenId.Value);
            await EnsureDestinoExistsAsync(request.DestinoId.Value);
            entity.TrabajosOrigenDestinoId = await GetOrCreateTrabajosOrigenDestinoIdAsync(
                request.OrigenId.Value,
                request.DestinoId.Value);
            await _context.Entry(entity).Reference(t => t.TrabajosOrigenDestino).LoadAsync();
        }

        var changeScripts = request.ScriptPreId is not null
            || request.ScriptPostId is not null
            || request.PreDetenerEnFallo is not null
            || request.PostDetenerEnFallo is not null;
        if (changeScripts)
        {
            if (request.ScriptPreId is null || request.ScriptPostId is null)
                throw new BadRequestException("scriptPreId y scriptPostId deben enviarse juntos para cambiar los scripts.");

            await EnsureScriptExistsAsync(request.ScriptPreId.Value);
            await EnsureScriptExistsAsync(request.ScriptPostId.Value);

            var preStop = request.PreDetenerEnFallo ?? entity.TrabajosScripts.PreDetenerEnFallo;
            var postStop = request.PostDetenerEnFallo ?? entity.TrabajosScripts.PostDetenerEnFallo;
            entity.TrabajosScriptsId = await GetOrCreateTrabajoScriptsIdAsync(
                request.ScriptPreId.Value,
                request.ScriptPostId.Value,
                preStop,
                postStop);
            await _context.Entry(entity).Reference(t => t.TrabajosScripts).LoadAsync();
        }

        entity.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Trabajo, AccionLog.Update, antes, SnapshotTrabajo(entity));
        return MapToResponse(entity);
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await _context.Trabajos
            .Include(t => t.TrabajosOrigenDestino)
            .Include(t => t.TrabajosScripts)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return false;
        var antes = SnapshotTrabajo(entity);
        _context.Trabajos.Remove(entity);
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Trabajo, AccionLog.Delete, antes, null);
        return true;
    }

    private static object SnapshotTrabajo(Trabajo t)
    {
        var p = t.TrabajosOrigenDestino;
        var s = t.TrabajosScripts;
        return new
        {
            t.Id,
            t.Nombre,
            t.Descripcion,
            t.TrabajosOrigenDestinoId,
            t.TrabajosScriptsId,
            origenId = p?.OrigenId,
            destinoId = p?.DestinoId,
            scriptPreId = s?.ScriptPreId,
            scriptPostId = s?.ScriptPostId,
            preDetenerEnFallo = s?.PreDetenerEnFallo,
            postDetenerEnFallo = s?.PostDetenerEnFallo,
            t.CronExpression,
            t.Activo,
            t.Procesando,
            t.EstatusPrevio,
            t.FechaCreacion,
            t.FechaModificacion
        };
    }

    private static TrabajoResponse MapToResponse(Trabajo t)
    {
        var p = t.TrabajosOrigenDestino;
        var s = t.TrabajosScripts;
        return new TrabajoResponse(
            t.Id,
            t.Nombre,
            t.Descripcion,
            t.TrabajosOrigenDestinoId,
            p.OrigenId,
            p.DestinoId,
            t.TrabajosScriptsId,
            s.ScriptPreId,
            s.ScriptPostId,
            s.PreDetenerEnFallo,
            s.PostDetenerEnFallo,
            t.CronExpression,
            t.Activo,
            t.Procesando,
            t.EstatusPrevio,
            t.FechaCreacion,
            t.FechaModificacion
        );
    }

    private async Task<int> GetOrCreateTrabajosOrigenDestinoIdAsync(int origenId, int destinoId)
    {
        var existing = await _context.TrabajosOrigenDestinos
            .FirstOrDefaultAsync(x => x.OrigenId == origenId && x.DestinoId == destinoId);
        if (existing is not null)
            return existing.Id;

        var link = new TrabajosOrigenDestino { OrigenId = origenId, DestinoId = destinoId };
        _context.TrabajosOrigenDestinos.Add(link);
        await _context.SaveChangesAsync();
        return link.Id;
    }

    private async Task<int> GetOrCreateTrabajoScriptsIdAsync(int scriptPreId, int scriptPostId, bool preDetener, bool postDetener)
    {
        var existing = await _context.TrabajosScripts
            .FirstOrDefaultAsync(x =>
                x.ScriptPreId == scriptPreId
                && x.ScriptPostId == scriptPostId
                && x.PreDetenerEnFallo == preDetener
                && x.PostDetenerEnFallo == postDetener);
        if (existing is not null)
            return existing.Id;

        var row = new TrabajoScripts
        {
            ScriptPreId = scriptPreId,
            ScriptPostId = scriptPostId,
            PreDetenerEnFallo = preDetener,
            PostDetenerEnFallo = postDetener
        };
        _context.TrabajosScripts.Add(row);
        await _context.SaveChangesAsync();
        return row.Id;
    }

    private async Task EnsureOrigenExistsAsync(int id)
    {
        if (!await _context.Origenes.AnyAsync(o => o.Id == id))
            throw new BadRequestException($"No existe un origen con Id '{id}'.");
    }

    private async Task EnsureDestinoExistsAsync(int id)
    {
        if (!await _context.Destinos.AnyAsync(d => d.Id == id))
            throw new BadRequestException($"No existe un destino con Id '{id}'.");
    }

    private async Task EnsureScriptExistsAsync(int id)
    {
        if (!await _context.ScriptConfigurations.AnyAsync(s => s.Id == id))
            throw new BadRequestException($"No existe un script con Id '{id}'.");
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BadRequestException($"{fieldName} es obligatorio.");
    }
}

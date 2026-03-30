using API.DTOs;
using API.Exceptions;
using API.Services.Interfaces;
using HostedService.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Services;

public class OrigenService : IOrigenService
{
    private readonly AppDbContext _context;

    public OrigenService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<OrigenResponse>> GetAll()
    {
        var list = await _context.Origenes
            .AsNoTracking()
            .OrderBy(o => o.Nombre)
            .ToListAsync();
        return list.Select(MapToResponse).OrderBy(x => x.Id);
    }

    public async Task<OrigenResponse?> GetById(int id)
    {
        var entity = await _context.Origenes.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

        if (entity is null)
            throw new NotFoundException($"Origen con Id '{id}' no existe");

        return MapToResponse(entity);
    }

    public async Task<OrigenResponse> Create(CreateOrigenRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        ValidateRequired(request.Ruta, nameof(request.Ruta));

        var nombre = request.Nombre.Trim();
        await EnsureNombreUnicoAsync(nombre, excludeId: null);

        var entity = new Origen
        {
            Nombre = nombre,
            Ruta = request.Ruta.Trim(),
            Descripcion = request.Descripcion.Trim()
        };
        _context.Origenes.Add(entity);
        await _context.SaveChangesAsync();
        return MapToResponse(entity);
    }

    public async Task<OrigenResponse?> Update(int id, UpdateOrigenRequest request)
    {
        var entity = await _context.Origenes.FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null) return null;

        if (request.Nombre is not null)
        {
            ValidateRequired(request.Nombre, nameof(request.Nombre));
            var nombre = request.Nombre.Trim();
            await EnsureNombreUnicoAsync(nombre, excludeId: id);
            entity.Nombre = nombre;
        }

        if (request.Ruta is not null)
        {
            ValidateRequired(request.Ruta, nameof(request.Ruta));
            entity.Ruta = request.Ruta.Trim();
        }

        if (request.Descripcion is not null)
            entity.Descripcion = request.Descripcion.Trim();

        entity.FechaModificacion = DateTime.UtcNow;
        entity.TamanoMaximo = request.TamanoMaximo;
        entity.FiltrosExclusiones = request.FiltrosExclusiones;

        await _context.SaveChangesAsync();
        return MapToResponse(entity);
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await _context.Origenes.FirstOrDefaultAsync(o => o.Id == id);
        if (entity is null) return false;
        _context.Origenes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    private static OrigenResponse MapToResponse(Origen o) => new(
        o.Id,
        o.Nombre,
        o.Ruta,
        o.Descripcion,
        o.TamanoMaximo,
        o.FiltrosExclusiones,
        o.FechaCreacion,
        o.FechaModificacion
        );

    private async Task EnsureNombreUnicoAsync(string nombre, int? excludeId)
    {
        var exists = excludeId is null
            ? await _context.Origenes.AnyAsync(o => o.Nombre == nombre)
            : await _context.Origenes.AnyAsync(o => o.Nombre == nombre && o.Id != excludeId);

        if (exists)
            throw new ConflictException($"Ya existe un origen con el nombre '{nombre}'.");
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BadRequestException($"{fieldName} es obligatorio.");
    }
}

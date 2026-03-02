using API;
using API.Exceptions;
using HostedService.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Scripts
{
    public class ScriptsHandler
    {
        private readonly AppDbContext _context;

        public ScriptsHandler(AppDbContext context) => _context = context;

        public async Task<IEnumerable<ScriptResponse>> GetAll()
        {
            var list = await _context.ScriptConfigurations
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();
            return list.Select(MapToResponse);
        }

        public async Task<ScriptResponse?> GetById(int id)
        {
            var entity = await _context.ScriptConfigurations.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<ScriptResponse> Create(CreateScriptRequest request)
        {
            ValidateTipo((int)request.Tipo);
            var entity = new ScriptConfiguration
            {
                Name = request.Name,
                ScriptPath = request.ScriptPath,
                Arguments = request.Arguments,
                Tipo = request.Tipo
            };
            _context.ScriptConfigurations.Add(entity);
            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<ScriptResponse?> Update(int id, UpdateScriptRequest request)
        {
            var entity = await _context.ScriptConfigurations.FirstOrDefaultAsync(s => s.Id == id);
            if (entity is null) return null;

            if (request.Name is not null) entity.Name = request.Name;
            if (request.ScriptPath is not null) entity.ScriptPath = request.ScriptPath;
            if (request.Arguments is not null) entity.Arguments = request.Arguments;
            if (request.Tipo is not null)
            {
                ValidateTipo((int)request.Tipo.Value);
                entity.Tipo = request.Tipo.Value;
            }

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _context.ScriptConfigurations.FirstOrDefaultAsync(s => s.Id == id);
            if (entity is null) return false;
            _context.ScriptConfigurations.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ScriptResponse MapToResponse(ScriptConfiguration s) => new(
            s.Id,
            s.Name,
            s.ScriptPath,
            s.Arguments,
            (int)s.Tipo);

        private static void ValidateTipo(int tipo)
        {
            if (tipo < 0 || tipo > 2) // 0=Ps1, 1=Bat, 2=Js
                throw new BadRequestException("El tipo solo puede ser .ps1 (0), .bat (1) o .js (2).");
        }
    }
}

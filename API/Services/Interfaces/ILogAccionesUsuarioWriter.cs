using API.Audit;

namespace API.Services.Interfaces;

public interface ILogAccionesUsuarioWriter
{
    /// <summary>
    /// <paramref name="accion"/> debe ser <see cref="AccionLog.Create"/>, <see cref="AccionLog.Update"/> o <see cref="AccionLog.Delete"/>.
    /// Los valores se serializan a JSON incluyendo el usuario del token.
    /// </summary>
    Task RegistrarAsync(
        string tablaAfectada,
        string accion,
        object? valorAnterior,
        object? valorNuevo,
        CancellationToken cancellationToken = default);
}

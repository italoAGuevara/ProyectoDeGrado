namespace HostedService.Entities;

public class LogAccionesUsuario
{
    public Guid Id { get; set; }
    public DateTime FechaAccion { get; set; } = DateTime.UtcNow;
    /// <summary>JSON con estado previo (vacío en create).</summary>
    public string ValorAnterior { get; set; } = string.Empty;
    /// <summary>JSON con estado nuevo (vacío en delete).</summary>
    public string ValorNuevo { get; set; } = string.Empty;
    /// <summary>create, update o delete.</summary>
    public string Accion { get; set; } = string.Empty;
    /// <summary>Entidad afectada: Origen, Destino, Trabajo, Script.</summary>
    public string TablaAfectada { get; set; } = string.Empty;
}

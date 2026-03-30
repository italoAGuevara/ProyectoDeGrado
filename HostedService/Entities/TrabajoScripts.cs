namespace HostedService.Entities;

/// <summary>Fila en TrabajosScripts: script pre, script post y flags de detención en fallo.</summary>
public class TrabajoScripts
{
    public int Id { get; set; }
    public int ScriptPreId { get; set; }
    public bool PreDetenerEnFallo { get; set; }
    public int ScriptPostId { get; set; }
    public bool PostDetenerEnFallo { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

    public ScriptConfiguration ScriptPre { get; set; } = null!;
    public ScriptConfiguration ScriptPost { get; set; } = null!;
    public ICollection<Trabajo> Trabajos { get; set; } = new List<Trabajo>();
}

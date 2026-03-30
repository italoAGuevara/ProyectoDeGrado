namespace HostedService.Entities
{
    /// <summary>Origen de respaldo (carpeta local o ruta de origen).</summary>
    public class Origen
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TamanoMaximo { get; set; } = string.Empty;
        public string FiltrosExclusiones { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    }
}

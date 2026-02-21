namespace HostedService.Entities
{
    /// <summary>Origen de respaldo (carpeta local o ruta de origen).</summary>
    public class Origen
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

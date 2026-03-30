
namespace HostedService.Entities
{
    public class StorageProvider
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        /// <summary>JSON schema de configuración del proveedor (almacenado como texto en la BD).</summary>
        public string? ConfigJsonSchema { get; set; }
    }
}

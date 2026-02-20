
namespace HostedService.Entities
{
    public class StorageProvider
    {
        public int Id { get; set; }
        public int Name { get; set; }
        /// <summary>JSON schema de configuración del proveedor (almacenado como texto en la BD).</summary>
        public string? ConfigJsonSchema { get; set; }
    }
}

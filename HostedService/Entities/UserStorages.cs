
namespace HostedService.Entities
{
    public class UserStorages
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        /// <summary>Credenciales del almacenamiento en JSON (almacenado como texto en la BD).</summary>
        public string? CredentialJson { get; set; }
        public string? CloudDestination { get; set; }
    }
}

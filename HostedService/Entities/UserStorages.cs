
namespace HostedService.Entities
{
    public class UserStorages
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public object? CredentialJson { get; set; }
        public string? CloudDestination { get; set; }
    }
}

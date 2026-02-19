
namespace HostedService.Entities
{
    public class StorageProvider
    {
        public int Id { get; set; }
        public int Name { get; set; }
        public object? ConfigJsonSchema { get; set; }
    }
}

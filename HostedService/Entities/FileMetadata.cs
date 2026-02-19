
namespace HostedService.Entities
{
    public class FileMetadata
    {
        public long Id { get; set; }
        public Guid BackupJobsId { get; set; }
        public string RelativePath { get; set; }
        public long FileSize { get; set; }
        public string HashCode { get; set; }
        public DateTime LastModified { get; set; }
        public string CloudFileId { get; set; }
        public DateTime LastTimeSync { get; set; }
    }
}

namespace HostedService.Entities
{
    public class BackupJob
    {
        public int Id { get; set; }
        public int UserStorageId { get; set; }
        public int? OrigenId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CronExpression { get; set; }
        public bool IsActive { get; set; }

        public Origen? Origen { get; set; }
        public List<RelationJobsAndScript>? Scripts { get; set; }
    }
}

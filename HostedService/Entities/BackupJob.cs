namespace HostedService.Entities
{
    public class BackupJob
    {
        public int Id { get; set; }
        public int UserStorageId { get; set; }
        public string? Name { get; set; }        
        public string? SourcePath { get; set; }
        public string? CronExpression { get; set; }
        public bool IsActive { get; set; }

        public List<RelationJobsAndScript>? Scripts { get; set; }

        public int IdRelationJobsAndScript { get; set; }
    }
}

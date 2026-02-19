
namespace HostedService.Entities
{
    public class RelationJobsAndScript
    {
        public int Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ScriptId { get; set; }

        public BackupJob? BackupJob { get; set; }
        public ScriptConfiguration? Script { get; set; }

        public int ExecutionOrder { get; set; }
    }
}

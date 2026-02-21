
namespace HostedService.Entities
{
    public class RelationJobsAndScript
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int ScriptId { get; set; }

        public BackupJob? BackupJob { get; set; }
        public ScriptConfiguration? Script { get; set; }

        public int ExecutionOrder { get; set; }

        /// <summary>Ejecutar como script pre-backup.</summary>
        public bool Pre { get; set; }

        /// <summary>Ejecutar como script post-backup.</summary>
        public bool Post { get; set; }
    }
}

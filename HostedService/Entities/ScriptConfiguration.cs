using HostedService.Enums;

namespace HostedService.Entities
{
    public class ScriptConfiguration
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ScriptPath { get; set; }
        public string? Arguments { get; set; }

        public ScriptTrigger Trigger { get; set; }

        public bool StopOnFailure { get; set; }

        public int TimeoutMinutes { get; set; } = 5;

        public List<RelationJobsAndScript>? Jobs { get; set; }

        public override string ToString() => $"[{Trigger}] {Name}";
    }
}

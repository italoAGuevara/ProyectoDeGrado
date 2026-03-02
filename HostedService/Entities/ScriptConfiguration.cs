using HostedService.Enums;

namespace HostedService.Entities
{
    public class ScriptConfiguration
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ScriptPath { get; set; }
        public string? Arguments { get; set; }

        /// <summary>Tipo de script: .ps1, .bat o .js</summary>
        public ScriptType Tipo { get; set; }                

        public List<RelationJobsAndScript>? Jobs { get; set; }                
    }
}

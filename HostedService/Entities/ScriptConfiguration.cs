namespace HostedService.Entities
{
    public class ScriptConfiguration
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ScriptPath { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; set; }

        /// <summary>Tipo de script: .ps1, .bat o .js (equivalente a TipoScript en el modelo).</summary>
        public string Tipo { get; set; } = string.Empty;

        public ICollection<TrabajoScripts> TrabajoScriptsComoPre { get; set; } = new List<TrabajoScripts>();
        public ICollection<TrabajoScripts> TrabajoScriptsComoPost { get; set; } = new List<TrabajoScripts>();
    }
}

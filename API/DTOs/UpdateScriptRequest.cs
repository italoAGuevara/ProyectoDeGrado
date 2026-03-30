using System.Text.Json.Serialization;
using HostedService.Enums;

namespace API.DTOs
{
    public record UpdateScriptRequest(
        [property: JsonPropertyName("nombre")] string Nombre,
        [property: JsonPropertyName("scriptPath")] string ScriptPath,
        [property: JsonPropertyName("arguments")] string Arguments,
        [property: JsonPropertyName("tipo")] string Tipo);
}

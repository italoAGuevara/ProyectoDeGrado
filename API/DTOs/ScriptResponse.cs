using System.Text.Json.Serialization;
using HostedService.Enums;

namespace API.DTOs
{
    public record ScriptResponse(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("nombre")] string Nombre,
        [property: JsonPropertyName("scriptPath")] string ScriptPath,
        [property: JsonPropertyName("arguments")] string Arguments,
        [property: JsonPropertyName("tipo")] string Tipo);
}

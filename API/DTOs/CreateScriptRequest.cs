using System.Text.Json.Serialization;

namespace API.DTOs;

public record CreateScriptRequest(
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("scriptPath")] string ScriptPath,
    [property: JsonPropertyName("arguments")] string Arguments,
    [property: JsonPropertyName("tipo")] string Tipo);

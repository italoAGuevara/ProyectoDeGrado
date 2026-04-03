using System.Text.Json.Serialization;

namespace API.DTOs;

public record RutaValidaResponse([property: JsonPropertyName("ruta")] string RutaNormalizada);

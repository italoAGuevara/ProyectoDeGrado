using System.Text.Json.Serialization;

namespace API.DTOs;

public record RutaOrigenRequest([property: JsonPropertyName("ruta")] string Ruta);

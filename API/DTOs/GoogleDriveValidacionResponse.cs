using System.Text.Json.Serialization;

namespace API.DTOs;

public record GoogleDriveValidacionResponse(
    [property: JsonPropertyName("mensaje")] string Mensaje,
    [property: JsonPropertyName("nombreCarpeta")] string? NombreCarpeta
);

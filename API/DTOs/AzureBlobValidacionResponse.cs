using System.Text.Json.Serialization;

namespace API.DTOs;

public record AzureBlobValidacionResponse(
    [property: JsonPropertyName("mensaje")] string Mensaje,
    [property: JsonPropertyName("containerName")] string? ContainerName = null);

using System.Text.Json.Serialization;

namespace API.DTOs;

public record S3ValidacionResponse(
    [property: JsonPropertyName("mensaje")] string Mensaje,
    [property: JsonPropertyName("bucket")] string? Bucket = null,
    [property: JsonPropertyName("identityArn")] string? IdentityArn = null);

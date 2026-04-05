using System.Text.Json.Serialization;

namespace API.DTOs;

public record ValidarAzureBlobRequest(
    [property: JsonPropertyName("azureBlobContainerName")] string? AzureBlobContainerName = null,
    [property: JsonPropertyName("azureBlobConnectionString")] string? AzureBlobConnectionString = null);

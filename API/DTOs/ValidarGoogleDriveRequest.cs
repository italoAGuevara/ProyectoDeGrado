using System.Text.Json.Serialization;

namespace API.DTOs;

public record ValidarGoogleDriveRequest(
    [property: JsonPropertyName("idCarpeta")] string IdCarpeta,
    [property: JsonPropertyName("serviceAccountEmail")] string ServiceAccountEmail,
    [property: JsonPropertyName("privateKey")] string PrivateKey
);

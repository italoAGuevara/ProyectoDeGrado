using System.Text.Json.Serialization;

namespace API.Features.Settings
{
    public record RequireAuthResponse([property: JsonPropertyName("requireAuth")] bool RequireAuth);
}

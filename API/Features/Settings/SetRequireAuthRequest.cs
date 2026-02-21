using System.Text.Json.Serialization;

namespace API.Features.Settings
{
    public record SetRequireAuthRequest([property: JsonPropertyName("requireAuth")] bool RequireAuth);
}

using System.Text.Json.Serialization;

namespace API.Features.Login
{
    public record ChangePasswordRequest(
        [property: JsonPropertyName("currentPassword")] string CurrentPassword,
        [property: JsonPropertyName("newPassword")] string NewPassword);
}

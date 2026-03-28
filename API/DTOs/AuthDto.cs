using System.Text.Json.Serialization;

namespace API.DTOs
{
    public record class LoginRequest(string password);

    public record ChangePasswordRequest(
        [property: JsonPropertyName("currentPassword")] string CurrentPassword,
        [property: JsonPropertyName("newPassword")] string NewPassword);

}

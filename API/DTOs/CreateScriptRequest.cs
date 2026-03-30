using System.Text.Json.Serialization;
using HostedService.Enums;

namespace API.DTOs
{

    public record CreateScriptRequest(string Nombre, string ScriptPath, string Arguments, string Tipo);
}

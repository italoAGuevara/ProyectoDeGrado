using System.Text.Json.Serialization;

namespace HostedService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    /// <summary>Tipos permitidos de script: .ps1, .bat, .js</summary>
    public enum ScriptType
    {
        Ps1,
        Bat,
        Js
    }
}

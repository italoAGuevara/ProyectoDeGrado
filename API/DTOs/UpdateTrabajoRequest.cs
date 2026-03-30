using System.Text.Json.Serialization;

namespace API.DTOs;

public record UpdateTrabajoRequest(
    [property: JsonPropertyName("nombre")] string? Nombre,
    [property: JsonPropertyName("descripcion")] string? Descripcion,
    [property: JsonPropertyName("origenId")] int? OrigenId,
    [property: JsonPropertyName("destinoId")] int? DestinoId,
    [property: JsonPropertyName("scriptPreId")] int? ScriptPreId,
    [property: JsonPropertyName("scriptPostId")] int? ScriptPostId,
    [property: JsonPropertyName("preDetenerEnFallo")] bool? PreDetenerEnFallo,
    [property: JsonPropertyName("postDetenerEnFallo")] bool? PostDetenerEnFallo,
    [property: JsonPropertyName("cronExpression")] string? CronExpression,
    [property: JsonPropertyName("activo")] bool? Activo,
    [property: JsonPropertyName("procesando")] bool? Procesando,
    [property: JsonPropertyName("estatusPrevio")] string? EstatusPrevio
);

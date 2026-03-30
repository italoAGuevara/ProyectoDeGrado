using System.Text.Json.Serialization;

namespace API.DTOs;

public record LogAccionesUsuarioResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("fechaAccion")] DateTime FechaAccion,
    [property: JsonPropertyName("valorAnterior")] string ValorAnterior,
    [property: JsonPropertyName("valorNuevo")] string ValorNuevo,
    [property: JsonPropertyName("accion")] string Accion,
    [property: JsonPropertyName("tablaAfectada")] string TablaAfectada
);

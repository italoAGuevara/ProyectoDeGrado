using System.Text.Json.Serialization;

namespace API.DTOs;

public record CreateOrigenRequest(
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("ruta")] string Ruta,
    [property: JsonPropertyName("descripcion")] string Descripcion,
    [property: JsonPropertyName("tamanoMaximo")] string TamanoMaximo,
    [property: JsonPropertyName("filtrosExclusiones")] string FiltrosExclusiones
    );

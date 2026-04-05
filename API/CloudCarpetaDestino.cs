using API.Exceptions;

namespace API;

/// <summary>
/// Prefijo de “carpeta” lógica dentro de un bucket S3 o contenedor Azure (segmentos separados por /).
/// </summary>
public static class CloudCarpetaDestino
{
    /// <summary>
    /// Normaliza a prefijo de clave: sin barra inicial, con barra final si hay segmentos.
    /// Vacío si no hay segmentos útiles.
    /// </summary>
    public static string NormalizePrefijo(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var s = input.Trim().Replace('\\', '/');
        var parts = s.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return string.Empty;

        foreach (var p in parts)
        {
            if (p is "." or "..")
                throw new BadRequestException("carpetaDestino no puede contener «.» ni «..».");
        }

        return string.Join('/', parts) + "/";
    }
}

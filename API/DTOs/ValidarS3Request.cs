using System.Text.Json.Serialization;

namespace API.DTOs;

/// <summary>
/// Validación S3: <c>bucketName</c> y <c>region</c> obligatorios.
/// Sin <c>accessKeyId</c>/<c>secretAccessKey</c> usa la cadena de credenciales por defecto del servidor (IAM/entorno).
/// Con ambas claves valida explícitamente esas credenciales contra el bucket.
/// </summary>
public record ValidarS3Request(
    [property: JsonPropertyName("bucketName")] string? BucketName = null,
    [property: JsonPropertyName("region")] string? Region = null,
    [property: JsonPropertyName("accessKeyId")] string? AccessKeyId = null,
    [property: JsonPropertyName("secretAccessKey")] string? SecretAccessKey = null);


namespace HostedService.Entities
{
    public class Destino
    {
        public int Id { get; set; }
        public string Nombre  { get; set; } = string.Empty;
        public string TipoDeDestino { get; set; } = string.Empty;
        public string Credenciales { get; set; } = string.Empty;
        /// <summary>ID de carpeta en Google Drive; vacío para S3.</summary>
        public string IdCarpeta { get; set; } = string.Empty;
        /// <summary>Access Key ID de AWS cuando el destino S3 usa claves; vacío para IAM/bucket u otros tipos.</summary>
        public string AccessKeyId { get; set; } = string.Empty;
        /// <summary>Secret Access Key de S3 protegido con Data Protection (no texto plano).</summary>
        public string SecretAccessKey { get; set; } = string.Empty;
        /// <summary>Nombre del bucket S3 (IAM o claves de acceso).</summary>
        public string BucketName { get; set; } = string.Empty;
        /// <summary>Región AWS del bucket (IAM o claves de acceso).</summary>
        public string S3Region { get; set; } = string.Empty;
        /// <summary>Correo de la cuenta de servicio de Google (solo Google Drive).</summary>
        public string GoogleServiceAccountEmail { get; set; } = string.Empty;
        /// <summary>Clave privada de la cuenta de servicio, protegida con Data Protection (solo Google Drive).</summary>
        public string GooglePrivateKey { get; set; } = string.Empty;
        /// <summary>Nombre del contenedor de Azure Blob Storage.</summary>
        public string AzureBlobContainerName { get; set; } = string.Empty;
        /// <summary>Cadena de conexión de Azure Storage, protegida con Data Protection (solo Azure Blob).</summary>
        public string AzureBlobConnectionString { get; set; } = string.Empty;
        /// <summary>Prefijo de carpeta lógica dentro del bucket S3 o del contenedor Azure (p. ej. respaldos/app/). Vacío en Google Drive.</summary>
        public string CarpetaDestino { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    }
}

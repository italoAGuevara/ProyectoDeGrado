using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Audit;
using API.DTOs;
using API.Exceptions;
using API.Services.Interfaces;
using HostedService.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using File = Google.Apis.Drive.v3.Data.File;

namespace API.Services.Services;

public class DestinoService : IDestinoService
{
    private readonly AppDbContext _context;
    private readonly IDestinoCredentialProtector _credentialProtector;
    private readonly ILogAccionesUsuarioWriter _logAcciones;

    public DestinoService(
        AppDbContext context,
        IDestinoCredentialProtector credentialProtector,
        ILogAccionesUsuarioWriter logAcciones)
    {
        _context = context;
        _credentialProtector = credentialProtector;
        _logAcciones = logAcciones;
    }

    public async Task<IEnumerable<DestinoResponse>> GetAll()
    {
        var list = await _context.Destinos
            .AsNoTracking()
            .OrderBy(d => d.Nombre)
            .ToListAsync();
        return list.Select(MapToResponse).OrderBy(x => x.Id);
    }

    public async Task<DestinoResponse?> GetById(int id)
    {
        var entity = await _context.Destinos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

        if (entity is null)
            throw new NotFoundException($"Destino con Id '{id}' no existe");

        return MapToResponse(entity);
    }

    public async Task<DestinoResponse> Create(CreateDestinoRequest request)
    {
        ValidateRequired(request.Nombre, nameof(request.Nombre));
        var tipo = NormalizeAndValidateTipo(request.Tipo);

        var nombre = request.Nombre.Trim();
        await EnsureNombreUnicoAsync(nombre, excludeId: null);

        var idCarpeta = tipo == DestinoTipos.GoogleDrive
            ? ValidateIdCarpetaGoogleDrive(request.IdCarpeta)
            : string.Empty;

        var blobPlain = "{}";
        var accessKeyId = string.Empty;
        var secretEnc = string.Empty;
        var bucketName = string.Empty;
        var s3Region = string.Empty;
        var googleServiceEmail = string.Empty;
        var googlePrivateEnc = string.Empty;

        if (tipo == DestinoTipos.S3)
        {
            var ak = request.AccessKeyId?.Trim() ?? string.Empty;
            var hasKeys = !string.IsNullOrWhiteSpace(ak);
            if (hasKeys)
            {
                if (string.IsNullOrWhiteSpace(request.SecretAccessKey))
                    throw new BadRequestException("secretAccessKey es obligatorio para S3 con claves de acceso.");
                accessKeyId = ak;
                secretEnc = _credentialProtector.Protect(request.SecretAccessKey.Trim());
                blobPlain = BuildS3KeysCredencialesJson(accessKeyId);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.BucketName) || string.IsNullOrWhiteSpace(request.Region))
                    throw new BadRequestException("bucketName y region son obligatorios para S3 con IAM (Bucket y región).");
                bucketName = request.BucketName.Trim();
                s3Region = request.Region.Trim();
            }
        }
        else if (tipo == DestinoTipos.GoogleDrive)
        {
            if (string.IsNullOrWhiteSpace(request.ServiceAccountEmail))
                throw new BadRequestException("serviceAccountEmail es obligatorio para Google Drive.");
            if (string.IsNullOrWhiteSpace(request.PrivateKey))
                throw new BadRequestException("privateKey es obligatorio para Google Drive.");
            await ValidarGoogleDriveCredencialesAsync(
                idCarpeta,
                request.ServiceAccountEmail.Trim(),
                request.PrivateKey,
                CancellationToken.None);
            googleServiceEmail = request.ServiceAccountEmail.Trim();
            googlePrivateEnc = _credentialProtector.Protect(request.PrivateKey.Trim());
        }

        var entity = new Destino
        {
            Nombre = nombre,
            TipoDeDestino = tipo,
            Credenciales = _credentialProtector.Protect(blobPlain),
            IdCarpeta = idCarpeta,
            AccessKeyId = accessKeyId,
            SecretAccessKey = secretEnc,
            BucketName = bucketName,
            S3Region = s3Region,
            GoogleServiceAccountEmail = googleServiceEmail,
            GooglePrivateKey = googlePrivateEnc
        };
        _context.Destinos.Add(entity);
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Destino, AccionLog.Create, null, SnapshotDestino(entity));
        return MapToResponse(entity);
    }

    public async Task<DestinoResponse?> Update(int id, UpdateDestinoRequest request)
    {
        var entity = await _context.Destinos.FirstOrDefaultAsync(d => d.Id == id);
        if (entity is null) return null;

        var antes = SnapshotDestino(entity);

        if (request.Nombre is not null)
        {
            ValidateRequired(request.Nombre, nameof(request.Nombre));
            var nombre = request.Nombre.Trim();
            await EnsureNombreUnicoAsync(nombre, excludeId: id);
            entity.Nombre = nombre;
        }

        if (request.Tipo is not null)
            entity.TipoDeDestino = NormalizeAndValidateTipo(request.Tipo);

        if (HasS3CredentialPatch(request) && entity.TipoDeDestino == DestinoTipos.S3)
            ApplyS3CredentialsFromPatch(entity, request);
        else if (HasGoogleCredentialPatch(request) && entity.TipoDeDestino == DestinoTipos.GoogleDrive)
            ApplyGoogleCredentialsFromPatch(entity, request);

        if (request.IdCarpeta is not null)
            entity.IdCarpeta = request.IdCarpeta.Trim();

        if (entity.TipoDeDestino == DestinoTipos.S3)
        {
            entity.IdCarpeta = string.Empty;
            entity.GoogleServiceAccountEmail = string.Empty;
            entity.GooglePrivateKey = string.Empty;
        }
        else
        {
            entity.AccessKeyId = string.Empty;
            entity.SecretAccessKey = string.Empty;
            entity.BucketName = string.Empty;
            entity.S3Region = string.Empty;
            if (string.IsNullOrWhiteSpace(entity.IdCarpeta))
                throw new BadRequestException("idCarpeta es obligatorio para Google Drive.");
        }

        if (entity.TipoDeDestino == DestinoTipos.GoogleDrive)
        {
            if (string.IsNullOrWhiteSpace(entity.GoogleServiceAccountEmail))
                throw new BadRequestException("serviceAccountEmail es obligatorio para Google Drive.");
            var plainKey = GetGooglePrivateKeyPlaintextOrThrow(entity);
            await ValidarGoogleDriveCredencialesAsync(
                entity.IdCarpeta,
                entity.GoogleServiceAccountEmail,
                plainKey,
                CancellationToken.None);
        }

        entity.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Destino, AccionLog.Update, antes, SnapshotDestino(entity));
        return MapToResponse(entity);
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await _context.Destinos.FirstOrDefaultAsync(d => d.Id == id);
        if (entity is null) return false;

        var enUso = await _context.Trabajos.AnyAsync(t => t.TrabajosOrigenDestino.DestinoId == id);
        if (enUso)
            throw new ConflictException("El destino está asociado a uno o más trabajos.");

        var vinculosHuerfanos = await _context.TrabajosOrigenDestinos
            .Where(l => l.DestinoId == id)
            .ToListAsync();
        if (vinculosHuerfanos.Count > 0)
        {
            _context.TrabajosOrigenDestinos.RemoveRange(vinculosHuerfanos);
            await _context.SaveChangesAsync();
        }

        var antes = SnapshotDestino(entity);
        _context.Destinos.Remove(entity);
        await _context.SaveChangesAsync();
        await _logAcciones.RegistrarAsync(TablasAfectadas.Destino, AccionLog.Delete, antes, null);
        return true;
    }

    public async Task<GoogleDriveValidacionResponse> ValidarConexionGoogleDriveAsync(
        ValidarGoogleDriveRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequired(request.IdCarpeta, nameof(request.IdCarpeta));
        ValidateRequired(request.ServiceAccountEmail, nameof(request.ServiceAccountEmail));
        ValidateRequired(request.PrivateKey, nameof(request.PrivateKey));
        var idCarpeta = ValidateIdCarpetaGoogleDrive(request.IdCarpeta);
        return await ValidarGoogleDriveCredencialesAsync(
            idCarpeta,
            request.ServiceAccountEmail.Trim(),
            request.PrivateKey,
            cancellationToken);
    }

    /// <summary>Comprueba credenciales y que <paramref name="idCarpeta"/> sea una carpeta accesible.</summary>
    private async Task<GoogleDriveValidacionResponse> ValidarGoogleDriveCredencialesAsync(
        string idCarpeta,
        string serviceAccountEmail,
        string privateKeyOrJson,
        CancellationToken cancellationToken)
    {
        var folderId = idCarpeta.Trim();
        var email = serviceAccountEmail.Trim();
        var pk = NormalizeGooglePrivateKeyInput(privateKeyOrJson);
        if (string.IsNullOrWhiteSpace(pk))
            throw new BadRequestException("privateKey es obligatorio.");
                
        var serviceAccountJson = BuildMinimalServiceAccountJson(email, pk);
        GoogleCredential credential;
        try
        {
            await using var ms = new MemoryStream(Encoding.UTF8.GetBytes(serviceAccountJson));
            credential = await GoogleCredential.FromStreamAsync(ms, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"Credenciales de cuenta de servicio no válidas: {ex.Message}");
        }

        credential = credential.CreateScoped(DriveService.Scope.DriveReadonly);

        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "evocative-lodge-415320",
        });

        File meta;
        try
        {
            var req = service.Files.Get(folderId);
            req.Fields = "id,name,mimeType";
            req.SupportsAllDrives = true;
            meta = await req.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Google.GoogleApiException ex)
        {
            var detail = ex.Error?.Message ?? ex.Message;
            throw new BadRequestException(
                $"Google Drive rechazó la operación. Comprueba el ID de carpeta, que la carpeta esté compartida con la cuenta de servicio y el detalle: {detail}");
        }

        if (!string.Equals(meta.MimeType, "application/vnd.google-apps.folder", StringComparison.Ordinal))
            throw new BadRequestException("El idCarpeta no corresponde a una carpeta de Google Drive.");

        var nombre = meta.Name ?? folderId;
        return new GoogleDriveValidacionResponse(
            $"Conexión correcta. Carpeta: «{nombre}».",
            nombre);
    }

    private static string NormalizeGooglePrivateKeyInput(string key)
    {
        var k = key.Trim();
        if (!k.StartsWith("{", StringComparison.Ordinal))
        {
            return k.Replace("\\r\\n", "\n", StringComparison.Ordinal)
                .Replace("\\n", "\n", StringComparison.Ordinal);
        }

        try
        {
            using var doc = JsonDocument.Parse(k);
            if (!doc.RootElement.TryGetProperty("private_key", out var pkEl))
                throw new BadRequestException("El JSON de la cuenta de servicio no contiene private_key.");
            var extracted = pkEl.GetString();
            if (string.IsNullOrWhiteSpace(extracted))
                throw new BadRequestException("private_key vacío en el JSON.");
            return extracted;
        }
        catch (JsonException)
        {
            throw new BadRequestException("privateKey debe ser el PEM de la clave privada o el JSON completo de la cuenta de servicio.");
        }
    }

    private static string BuildMinimalServiceAccountJson(string clientEmail, string privateKeyPem)
    {
        var payload = new Dictionary<string, string>
        {
            ["type"] = "service_account",
            ["project_id"] = "cloudkeep-connection-test",
            ["private_key_id"] = "connection-test",
            ["private_key"] = privateKeyPem,
            ["client_email"] = clientEmail,
            ["client_id"] = "0",
            ["auth_uri"] = "https://accounts.google.com/o/oauth2/auth",
            ["token_uri"] = "https://oauth2.googleapis.com/token",
            ["auth_provider_x509_cert_url"] = "https://www.googleapis.com/oauth2/v1/certs"
        };
        return JsonSerializer.Serialize(payload);
    }

    /// <summary>No persiste credenciales; solo indica si había valor almacenado.</summary>
    private static object SnapshotDestino(Destino d) => new
    {
        d.Id,
        d.Nombre,
        d.TipoDeDestino,
        d.IdCarpeta,
        d.AccessKeyId,
        secretAccessKeyAlmacenada = !string.IsNullOrEmpty(d.SecretAccessKey),
        d.BucketName,
        d.S3Region,
        d.GoogleServiceAccountEmail,
        privateKeyGoogleAlmacenada = !string.IsNullOrEmpty(d.GooglePrivateKey),
        d.FechaCreacion,
        d.FechaModificacion
    };

    private static DestinoResponse MapToResponse(Destino d) => new(
        d.Id,
        d.Nombre,
        d.TipoDeDestino,
        d.IdCarpeta,
        d.AccessKeyId,
        !string.IsNullOrEmpty(d.SecretAccessKey),
        d.BucketName,
        d.S3Region,
        d.GoogleServiceAccountEmail,
        !string.IsNullOrEmpty(d.GooglePrivateKey),
        d.FechaCreacion,
        d.FechaModificacion
    );

    private async Task EnsureNombreUnicoAsync(string nombre, int? excludeId)
    {
        var exists = excludeId is null
            ? await _context.Destinos.AnyAsync(d => d.Nombre == nombre)
            : await _context.Destinos.AnyAsync(d => d.Nombre == nombre && d.Id != excludeId);

        if (exists)
            throw new ConflictException($"Ya existe un destino con el nombre '{nombre}'.");
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BadRequestException($"{fieldName} es obligatorio.");
    }

    private static string NormalizeAndValidateTipo(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new BadRequestException("tipo es obligatorio.");

        var t = tipo.Trim();
        if (!DestinoTipos.Allowed.Contains(t))
            throw new BadRequestException($"tipo debe ser '{DestinoTipos.S3}' o '{DestinoTipos.GoogleDrive}'.");

        return t;
    }

    private static string ValidateIdCarpetaGoogleDrive(string? idCarpeta)
    {
        if (string.IsNullOrWhiteSpace(idCarpeta))
            throw new BadRequestException("idCarpeta es obligatorio para Google Drive.");
        return idCarpeta.Trim();
    }

    private static bool HasS3CredentialPatch(UpdateDestinoRequest r) =>
        r.BucketName != null || r.Region != null || r.AccessKeyId != null || r.SecretAccessKey != null;

    private static bool HasGoogleCredentialPatch(UpdateDestinoRequest r) =>
        r.ServiceAccountEmail != null || r.PrivateKey != null;

    private void ApplyS3CredentialsFromPatch(Destino entity, UpdateDestinoRequest r)
    {
        entity.GoogleServiceAccountEmail = string.Empty;
        entity.GooglePrivateKey = string.Empty;

        var bucketTouched = r.BucketName != null;
        var regionTouched = r.Region != null;
        if (bucketTouched != regionTouched)
            throw new BadRequestException("bucketName y region deben enviarse juntos para S3 IAM.");

        var akRaw = r.AccessKeyId?.Trim() ?? string.Empty;
        var hasAk = !string.IsNullOrWhiteSpace(akRaw);

        if (hasAk)
        {
            entity.AccessKeyId = akRaw;
            entity.BucketName = string.Empty;
            entity.S3Region = string.Empty;
            if (r.SecretAccessKey != null)
            {
                entity.SecretAccessKey = string.IsNullOrWhiteSpace(r.SecretAccessKey)
                    ? string.Empty
                    : _credentialProtector.Protect(r.SecretAccessKey.Trim());
            }

            entity.Credenciales = _credentialProtector.Protect(BuildS3KeysCredencialesJson(entity.AccessKeyId));
            return;
        }

        if (bucketTouched && r.BucketName != null && r.Region != null)
        {
            if (string.IsNullOrWhiteSpace(r.BucketName) || string.IsNullOrWhiteSpace(r.Region))
                throw new BadRequestException("bucketName y region son obligatorios para S3 con IAM (Bucket y región).");
            entity.AccessKeyId = string.Empty;
            entity.SecretAccessKey = string.Empty;
            entity.BucketName = r.BucketName.Trim();
            entity.S3Region = r.Region.Trim();
            entity.Credenciales = _credentialProtector.Protect("{}");
            return;
        }

        if (r.SecretAccessKey != null && !string.IsNullOrWhiteSpace(entity.AccessKeyId))
        {
            entity.SecretAccessKey = string.IsNullOrWhiteSpace(r.SecretAccessKey)
                ? string.Empty
                : _credentialProtector.Protect(r.SecretAccessKey.Trim());
        }
    }

    private string GetGooglePrivateKeyPlaintextOrThrow(Destino entity)
    {
        if (string.IsNullOrWhiteSpace(entity.GooglePrivateKey))
            throw new BadRequestException(
                "privateKey es obligatoria para Google Drive (vuelve a pegar la clave si editaste el destino).");
        try
        {
            return _credentialProtector.Unprotect(entity.GooglePrivateKey);
        }
        catch (CryptographicException)
        {
            throw new BadRequestException(
                "No se pudo descifrar la clave privada almacenada. Guarda de nuevo la clave de la cuenta de servicio.");
        }
    }

    private void ApplyGoogleCredentialsFromPatch(Destino entity, UpdateDestinoRequest r)
    {
        entity.AccessKeyId = string.Empty;
        entity.SecretAccessKey = string.Empty;
        entity.BucketName = string.Empty;
        entity.S3Region = string.Empty;

        if (r.ServiceAccountEmail != null)
            entity.GoogleServiceAccountEmail = r.ServiceAccountEmail.Trim();
        if (r.PrivateKey != null)
        {
            entity.GooglePrivateKey = string.IsNullOrWhiteSpace(r.PrivateKey)
                ? string.Empty
                : _credentialProtector.Protect(r.PrivateKey.Trim());
        }

        entity.Credenciales = _credentialProtector.Protect("{}");
    }

    private static string BuildS3KeysCredencialesJson(string accessKeyId) =>
        JsonSerializer.Serialize(new Dictionary<string, string> { ["AccessKeyId"] = accessKeyId });
}

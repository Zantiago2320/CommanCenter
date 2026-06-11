using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CommanCenter.Portal.Services;

/// <summary>
/// Servicio para subir imágenes. Usa Azure Blob Storage si está configurado
/// (sección "AzureBlobStorage"); si no, cae a almacenamiento local en wwwroot/uploads.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Sube una imagen y devuelve la URL pública (o ruta relativa si es local).
    /// </summary>
    /// <param name="archivo">Stream del archivo.</param>
    /// <param name="nombreArchivo">Nombre único del archivo (con extensión).</param>
    /// <param name="subcarpeta">Subcarpeta lógica: "consultores" o "celulas".</param>
    /// <param name="contentType">Tipo MIME del archivo.</param>
    Task<string> SubirAsync(Stream archivo, string nombreArchivo, string subcarpeta, string contentType);
}

public class ImageStorageService : IImageStorageService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageStorageService> _logger;

    public ImageStorageService(IConfiguration config, IWebHostEnvironment env, ILogger<ImageStorageService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    public async Task<string> SubirAsync(Stream archivo, string nombreArchivo, string subcarpeta, string contentType)
    {
        var connectionString = _config["AzureBlobStorage:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTION_STRING");

        // Si hay conexión a Blob, se usa Azure; si no, almacenamiento local.
        if (!string.IsNullOrWhiteSpace(connectionString))
            return await SubirABlobAsync(connectionString, archivo, nombreArchivo, subcarpeta, contentType);

        _logger.LogWarning("AzureBlobStorage no configurado: la imagen se guarda localmente (efímero en Azure).");
        return await GuardarLocalAsync(archivo, nombreArchivo, subcarpeta);
    }

    private async Task<string> SubirABlobAsync(string connectionString, Stream archivo,
        string nombreArchivo, string subcarpeta, string contentType)
    {
        var containerName = _config["AzureBlobStorage:ContainerName"] ?? "imagenes";

        var containerClient = new BlobContainerClient(connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobName = $"{subcarpeta}/{nombreArchivo}";
        var blobClient = containerClient.GetBlobClient(blobName);

        archivo.Position = 0;
        await blobClient.UploadAsync(archivo, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        _logger.LogInformation("Imagen subida a Blob Storage: {Url}", blobClient.Uri);
        return blobClient.Uri.ToString();
    }

    private async Task<string> GuardarLocalAsync(Stream archivo, string nombreArchivo, string subcarpeta)
    {
        var carpeta = Path.Combine(_env.WebRootPath, "uploads", subcarpeta);
        Directory.CreateDirectory(carpeta);

        var rutaFisica = Path.Combine(carpeta, nombreArchivo);
        await using (var stream = new FileStream(rutaFisica, FileMode.Create))
        {
            archivo.Position = 0;
            await archivo.CopyToAsync(stream);
        }

        return $"/uploads/{subcarpeta}/{nombreArchivo}";
    }
}

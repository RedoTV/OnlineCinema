namespace OnlineCinema.Backend.Services;

/// <summary>
/// Абстракция над объектным хранилищем. За S3-совместимым API прячем MinIO,
/// чтобы потом можно было переключиться на AWS S3 / Azure Blob не трогая бизнес-логику.
/// </summary>
public interface IStorageService
{
    Task<string> SaveAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, long Length)> GetAsync(string objectKey, CancellationToken ct = default);
    Task DeleteAsync(string objectKey, CancellationToken ct = default);
    /// <summary>
    /// Генерирует presigned URL для прямой отдачи/загрузки браузером в обход бэкенда.
    /// </summary>
    string GetPresignedUrl(string objectKey, int expirySeconds = 3600);
    /// <summary>Возвращает same-origin URL, который браузер откроет через nginx.</summary>
    string BuildBrowserMediaUrl(string objectKey, int expirySeconds = 3600);
    bool TryParseObjectKey(string? urlOrKey, out string objectKey);
}

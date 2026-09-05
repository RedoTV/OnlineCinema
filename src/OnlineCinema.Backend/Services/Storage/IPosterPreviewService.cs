namespace OnlineCinema.Backend.Services.Storage;

/// <summary>
/// Превью постеров для регулярного просмотра (каталог/сериалы):
/// возвращает уменьшенную копию оригинала, кэширует в хранилище.
/// </summary>
public interface IPosterPreviewService
{
    Task<(Stream Stream, string ContentType, long Length)> GetPreviewAsync(
        string objectKey, CancellationToken ct = default);
}
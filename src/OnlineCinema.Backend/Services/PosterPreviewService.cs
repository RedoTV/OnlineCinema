using OnlineCinema.Backend.Services.Helpers;

namespace OnlineCinema.Backend.Services;

public class PosterPreviewService : IPosterPreviewService
{
    private readonly IStorageService _storage;
    private readonly ILogger<PosterPreviewService> _logger;

    public PosterPreviewService(IStorageService storage, ILogger<PosterPreviewService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<(Stream Stream, string ContentType, long Length)> GetPreviewAsync(
        string objectKey, CancellationToken ct = default)
    {
        var previewKey = $"previews/{objectKey}";

        // Превью уже закэшировано — отдаём сразу, без ресайза.
        try
        {
            var cached = await _storage.GetAsync(previewKey, ct);
            _logger.LogDebug("Preview cache hit {Key}", previewKey);
            return cached;
        }
        catch
        {
            // Кэша нет — строим ниже.
        }

        var (stream, contentType, _) = await _storage.GetAsync(objectKey, ct);
        await using (stream)
        {
            var (resized, previewType) = await ImageResizer.ResizeToPreviewAsync(stream, contentType, ct);
            await using (resized)
            {
                var ms = new MemoryStream();
                await resized.CopyToAsync(ms, ct);
                var bytes = ms.ToArray();

                // Кэшируем для следующих просмотров; гонка двух запросов
                // безопасна — последний записавший побеждает тем же контентом.
                try
                {
                    await using var upload = new MemoryStream(bytes, writable: false);
                    await _storage.SaveAsync(upload, previewKey, previewType, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Preview cache write failed {Key}", previewKey);
                }

                var output = new MemoryStream(bytes, writable: false);
                return ((Stream)output, previewType, bytes.LongLength);
            }
        }
    }
}
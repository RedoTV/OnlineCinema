using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace OnlineCinema.Backend.Services.Helpers;

/// <summary>
/// Уменьшает изображение постера для предпросмотра в каталоге.
/// Регулярный просмотр сетки не требует полного разрешения — ширина 320px
/// (формат 2:3 -> 320x480) и JPEG q60 режут трафик в ~5-10 раз.
/// SVG (сиды/заглушки) и неизвестные форматы возвращаются как есть.
/// </summary>
public static class ImageResizer
{
    public const int PreviewWidth = 320;
    public const int PreviewQuality = 60;

    public static bool IsResizable(string? contentType) =>
        contentType is "image/jpeg" or "image/png" or "image/webp";

    public static string PreviewContentType(string contentType) =>
        IsResizable(contentType) ? "image/jpeg" : contentType;

    public static async Task<(Stream Stream, string ContentType)> ResizeToPreviewAsync(
        Stream source, string contentType, CancellationToken ct = default)
    {
        // Не умеем/не хотим ресайзить — отдаём оригинал без изменений.
        if (!IsResizable(contentType))
        {
            var copy = new MemoryStream();
            await source.CopyToAsync(copy, ct);
            copy.Position = 0;
            return (copy, contentType);
        }

        using var image = await Image.LoadAsync(source, ct);

        // Маленький оригинал апскейлить не надо — лишний вес без пользы.
        if (image.Width <= PreviewWidth)
        {
            source.Position = 0;
            var passthrough = new MemoryStream();
            await source.CopyToAsync(passthrough, ct);
            passthrough.Position = 0;
            return (passthrough, contentType);
        }

        var ratio = (double)PreviewWidth / image.Width;
        var height = Math.Max(1, (int)Math.Round(image.Height * ratio));
        image.Mutate(x => x.Resize(PreviewWidth, height));

        var output = new MemoryStream();
        await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = PreviewQuality }, ct);
        output.Position = 0;
        return (output, "image/jpeg");
    }
}
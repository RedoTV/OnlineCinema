using OnlineCinema.Backend.Services.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace OnlineCinema.Backend.Tests;

public class ImageResizerTests
{
    private static MemoryStream MakeImage(int width, int height)
    {
        var ms = new MemoryStream();
        using (var image = new Image<Rgba32>(width, height))
        {
            image.SaveAsJpeg(ms);
        }
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task ResizeToPreview_Reduces_Large_Jpeg_To_Preview_Width()
    {
        await using var source = MakeImage(600, 900);
        var (stream, contentType) = await ImageResizer.ResizeToPreviewAsync(source, "image/jpeg");
        await using (stream)
        {
            using var result = await Image.LoadAsync(stream);
            Assert.Equal(ImageResizer.PreviewWidth, result.Width);
            Assert.Equal("image/jpeg", contentType);
            Assert.True(stream.Length < source.Length);
        }
    }

    [Fact]
    public async Task ResizeToPreview_Passes_Through_Small_Original()
    {
        await using var source = MakeImage(200, 300);
        var (stream, _) = await ImageResizer.ResizeToPreviewAsync(source, "image/jpeg");
        await using (stream)
        {
            using var result = await Image.LoadAsync(stream);
            Assert.Equal(200, result.Width);
        }
    }

    [Fact]
    public async Task ResizeToPreview_Passes_Through_Svg_Unchanged()
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>"u8.ToArray();
        await using var source = new MemoryStream(svg);
        var (stream, contentType) = await ImageResizer.ResizeToPreviewAsync(source, "image/svg+xml");
        await using (stream)
        {
            Assert.Equal("image/svg+xml", contentType);
            Assert.Equal(svg.Length, stream.Length);
        }
    }
}
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Tests for rendering HTML to JPEG format.
/// </summary>
[Collection("Rendering")]
public class RenderToJpegTests
{
    private readonly RenderingFixture _fixture;

    public RenderToJpegTests(RenderingFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void RenderToJpeg_ReturnsValidJpegWithCorrectMagicBytes()
    {
        Assert.NotNull(_fixture.JpegBytes);
        Assert.True(_fixture.JpegBytes.Length > 0);
        // JPEG magic bytes: FF D8 FF
        Assert.Equal(0xFF, _fixture.JpegBytes[0]);
        Assert.Equal(0xD8, _fixture.JpegBytes[1]);
        Assert.Equal(0xFF, _fixture.JpegBytes[2]);
    }

    [Fact]
    public void RenderToJpeg_QualityAffectsFileSize()
    {
        Assert.True(_fixture.JpegHighQuality.Length > _fixture.JpegLowQuality.Length,
            $"High quality ({_fixture.JpegHighQuality.Length}) should be larger than low quality ({_fixture.JpegLowQuality.Length})");
    }
}

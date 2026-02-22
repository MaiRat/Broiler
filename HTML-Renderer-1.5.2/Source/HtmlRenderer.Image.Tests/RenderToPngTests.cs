using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Tests for rendering HTML to PNG format.
/// </summary>
[Collection("Rendering")]
public class RenderToPngTests
{
    private readonly RenderingFixture _fixture;

    public RenderToPngTests(RenderingFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void RenderToPng_SimpleHtml_ReturnsValidPngWithCorrectMagicBytes()
    {
        Assert.NotNull(_fixture.PngBytes);
        Assert.True(_fixture.PngBytes.Length > 100);
        // PNG magic bytes: 137 80 78 71 13 10 26 10
        Assert.Equal(0x89, _fixture.PngBytes[0]);
        Assert.Equal(0x50, _fixture.PngBytes[1]);
        Assert.Equal(0x4E, _fixture.PngBytes[2]);
        Assert.Equal(0x47, _fixture.PngBytes[3]);
    }

    [Fact]
    public void RenderToPng_StyledHtml_ReturnsValidPng()
    {
        Assert.NotNull(_fixture.PngStyledBytes);
        Assert.True(_fixture.PngStyledBytes.Length > 100);
    }

    [Fact]
    public void RenderToPng_EmptyHtml_ReturnsValidPng()
    {
        var emptyBytes = HtmlRender.RenderToPng("", 100, 100);
        Assert.NotNull(emptyBytes);
        Assert.True(emptyBytes.Length > 0);
    }
}

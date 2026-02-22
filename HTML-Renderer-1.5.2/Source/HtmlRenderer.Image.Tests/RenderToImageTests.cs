using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Tests for rendering HTML to SKBitmap images with various HTML samples.
/// </summary>
[Collection("Rendering")]
public class RenderToImageTests
{
    private readonly RenderingFixture _fixture;

    public RenderToImageTests(RenderingFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void RenderToImage_SimpleDiv_CreatesCorrectSize()
    {
        Assert.NotNull(_fixture.SimpleDiv);
        Assert.Equal(400, _fixture.SimpleDiv.Width);
        Assert.Equal(300, _fixture.SimpleDiv.Height);
    }

    [Fact]
    public void RenderToImage_EmptyHtmlWithDefaultBackground_IsWhite()
    {
        var pixel = _fixture.EmptyWhiteBackground.GetPixel(0, 0);
        Assert.Equal(255, pixel.Red);
        Assert.Equal(255, pixel.Green);
        Assert.Equal(255, pixel.Blue);
    }

    [Fact]
    public void RenderToImage_EmptyHtmlWithCustomBackground_IsApplied()
    {
        var pixel = _fixture.EmptyRedBackground.GetPixel(0, 0);
        Assert.Equal(255, pixel.Red);
        Assert.Equal(0, pixel.Green);
        Assert.Equal(0, pixel.Blue);
    }

    [Fact]
    public void RenderToImage_ColoredDivAndNestedElements_RenderCorrectly()
    {
        Assert.NotNull(_fixture.ColoredDiv);
        // Check that some pixels in the blue div area are non-white
        bool hasNonWhite = false;
        for (int y = 0; y < 20 && !hasNonWhite; y++)
            for (int x = 0; x < 20 && !hasNonWhite; x++)
            {
                var p = _fixture.ColoredDiv.GetPixel(x, y);
                if (p.Red != 255 || p.Green != 255 || p.Blue != 255)
                    hasNonWhite = true;
            }
        Assert.True(hasNonWhite, "Expected non-white pixels from blue background div");
    }

    [Fact]
    public void RenderToImage_TableAndCssStyles_DoNotThrow()
    {
        Assert.NotNull(_fixture.TableAndStyles);
        Assert.Equal(400, _fixture.TableAndStyles.Width);
    }

    [Fact]
    public void RenderToImage_LargeDocument_DoesNotThrow()
    {
        Assert.NotNull(_fixture.LargeDocument);
        Assert.Equal(800, _fixture.LargeDocument.Width);
    }

    [Fact]
    public void RenderToImageAutoSized_CreatesReasonableSize()
    {
        Assert.NotNull(_fixture.AutoSized);
        Assert.True(_fixture.AutoSized.Width > 0);
        Assert.True(_fixture.AutoSized.Height > 0);
        Assert.True(_fixture.AutoSized.Width <= 4096);
    }

    [Fact]
    public void RenderToImageAutoSized_EmptyHtml_ReturnsMinimalBitmap()
    {
        using var bitmap = HtmlRender.RenderToImageAutoSized("", maxWidth: 100);
        Assert.True(bitmap.Width >= 1);
        Assert.True(bitmap.Height >= 1);
    }

    [Fact]
    public void RenderToImageAutoSized_NullHtml_ReturnsMinimalBitmap()
    {
        using var bitmap = HtmlRender.RenderToImageAutoSized(null, maxWidth: 100);
        Assert.True(bitmap.Width >= 1);
        Assert.True(bitmap.Height >= 1);
    }
}

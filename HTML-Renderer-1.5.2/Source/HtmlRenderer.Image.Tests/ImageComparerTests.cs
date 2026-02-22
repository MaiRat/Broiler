using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Tests for the ImageComparer utility class.
/// </summary>
[Collection("Rendering")]
public class ImageComparerTests
{
    private readonly RenderingFixture _fixture;

    public ImageComparerTests(RenderingFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Compare_IdenticalImages_ReturnsOne()
    {
        using var bitmap1 = CreateSolidBitmap(10, 10, SKColors.Red);
        using var bitmap2 = CreateSolidBitmap(10, 10, SKColors.Red);
        Assert.Equal(1.0, ImageComparer.Compare(bitmap1, bitmap2));
    }

    [Fact]
    public void Compare_DifferentImages_ReturnsLessThanOne()
    {
        using var bitmap1 = CreateSolidBitmap(10, 10, SKColors.Red);
        using var bitmap2 = CreateSolidBitmap(10, 10, SKColors.Blue);
        Assert.True(ImageComparer.Compare(bitmap1, bitmap2) < 1.0);
    }

    [Fact]
    public void Compare_DifferentSizes_ReturnsZero()
    {
        using var bitmap1 = CreateSolidBitmap(10, 10, SKColors.Red);
        using var bitmap2 = CreateSolidBitmap(20, 20, SKColors.Red);
        Assert.Equal(0, ImageComparer.Compare(bitmap1, bitmap2));
    }

    [Fact]
    public void Compare_NullImage_ReturnsZero()
    {
        using var bitmap = CreateSolidBitmap(10, 10, SKColors.Red);
        Assert.Equal(0, ImageComparer.Compare(null, bitmap));
        Assert.Equal(0, ImageComparer.Compare(bitmap, null));
    }

    [Fact]
    public void AreIdentical_SameAndDifferent_WorksCorrectly()
    {
        using var bitmap1 = CreateSolidBitmap(10, 10, SKColors.Green);
        using var bitmap2 = CreateSolidBitmap(10, 10, SKColors.Green);
        using var bitmap3 = CreateSolidBitmap(10, 10, SKColors.Red);

        Assert.True(ImageComparer.AreIdentical(bitmap1, bitmap2));
        Assert.False(ImageComparer.AreIdentical(bitmap1, bitmap3));
    }

    [Fact]
    public void CompareWithTolerance_SlightDifference_ReturnsHighSimilarity()
    {
        using var bitmap1 = CreateSolidBitmap(10, 10, new SKColor(100, 100, 100));
        using var bitmap2 = CreateSolidBitmap(10, 10, new SKColor(103, 102, 101));

        var similarity = ImageComparer.CompareWithTolerance(bitmap1, bitmap2, colorTolerance: 5);
        Assert.Equal(1.0, similarity);
    }

    [Fact]
    public void CompareWithTolerance_VeryDifferent_ReturnsLowSimilarity()
    {
        using var bitmap1 = CreateSolidBitmap(10, 10, SKColors.Red);
        using var bitmap2 = CreateSolidBitmap(10, 10, SKColors.Blue);

        var similarity = ImageComparer.CompareWithTolerance(bitmap1, bitmap2, colorTolerance: 5);
        Assert.True(similarity < 0.5);
    }

    [Fact]
    public void AreSimilar_VariousScenarios_WorkCorrectly()
    {
        using var bitmap1 = CreateSolidBitmap(10, 10, SKColors.Yellow);
        using var bitmap2 = CreateSolidBitmap(10, 10, SKColors.Yellow);
        Assert.True(ImageComparer.AreSimilar(bitmap1, bitmap2));

        using var bitmap3 = CreateSolidBitmap(10, 10, new SKColor(200, 200, 200));
        using var bitmap4 = CreateSolidBitmap(10, 10, new SKColor(202, 198, 201));
        Assert.True(ImageComparer.AreSimilar(bitmap3, bitmap4, threshold: 0.95, colorTolerance: 5));

        using var bitmap5 = CreateSolidBitmap(10, 10, SKColors.White);
        using var bitmap6 = CreateSolidBitmap(10, 10, SKColors.Black);
        Assert.False(ImageComparer.AreSimilar(bitmap5, bitmap6));
    }

    [Fact]
    public void Compare_RenderedHtmlWithSyntheticBitmap_ProducesMeaningfulResult()
    {
        using var synthetic = CreateSolidBitmap(100, 100, SKColors.Blue);
        Assert.False(ImageComparer.AreIdentical(_fixture.RenderedForComparison, synthetic));
        Assert.True(ImageComparer.AreIdentical(_fixture.RenderedForComparison, _fixture.RenderedForComparison));
    }

    private static SKBitmap CreateSolidBitmap(int width, int height, SKColor color)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
        return bitmap;
    }
}

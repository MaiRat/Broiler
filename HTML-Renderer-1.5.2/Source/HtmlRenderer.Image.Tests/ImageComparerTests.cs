using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core.IR;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Tests for the ImageComparer utility class.
/// </summary>
[Collection("Rendering")]
public class ImageComparerTests(RenderingFixture fixture)
{
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
        Assert.False(ImageComparer.AreIdentical(fixture.RenderedForComparison, synthetic));
        Assert.True(ImageComparer.AreIdentical(fixture.RenderedForComparison, fixture.RenderedForComparison));
    }

    [Fact]
    public void PixelDiffCompare_DifferentImages_CollectsMismatches()
    {
        using var bitmap1 = CreateSolidBitmap(4, 4, SKColors.Red);
        using var bitmap2 = CreateSolidBitmap(4, 4, SKColors.Blue);

        // Set one pixel to match so not all pixels differ
        bitmap2.SetPixel(0, 0, SKColors.Red);

        var config = new DeterministicRenderConfig
        {
            ViewportWidth = 4,
            ViewportHeight = 4,
            PixelDiffThreshold = 0.0,
            ColorTolerance = 0
        };

        using var result = PixelDiffRunner.Compare(bitmap1, bitmap2, config);

        // 15 of 16 pixels differ (all except (0,0))
        Assert.Equal(15, result.DiffPixelCount);
        Assert.Equal(15, result.Mismatches.Count);

        // Verify mismatch data contains positions and colours
        var first = result.Mismatches[0];
        Assert.True(first.X >= 0 && first.X < 4);
        Assert.True(first.Y >= 0 && first.Y < 4);
        // Actual = Red (255,0,0), Baseline = Blue (0,0,255)
        Assert.Equal(255, first.ActualR);
        Assert.Equal(0, first.ActualG);
        Assert.Equal(0, first.ActualB);
        Assert.Equal(0, first.BaselineR);
        Assert.Equal(0, first.BaselineG);
        Assert.Equal(255, first.BaselineB);
    }

    [Fact]
    public void PixelDiffCompare_IdenticalImages_ReturnsEmptyMismatches()
    {
        using var bitmap1 = CreateSolidBitmap(4, 4, SKColors.Green);
        using var bitmap2 = CreateSolidBitmap(4, 4, SKColors.Green);

        using var result = PixelDiffRunner.Compare(bitmap1, bitmap2);

        Assert.Equal(0, result.DiffPixelCount);
        Assert.Empty(result.Mismatches);
    }

    private static SKBitmap CreateSolidBitmap(int width, int height, SKColor color)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
        return bitmap;
    }
}

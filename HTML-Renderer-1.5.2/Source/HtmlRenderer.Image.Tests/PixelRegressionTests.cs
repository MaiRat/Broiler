using System;
using System.IO;
using System.Runtime.CompilerServices;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core.IR;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Phase 5 pixel-regression tests. Each test renders HTML deterministically,
/// compares the result against a committed baseline PNG, and classifies any
/// failure as layout / paint / raster diff.
///
/// To re-baseline: delete the baseline PNG and run the test. A new baseline
/// is written and the test is marked as failed (re-run to validate).
/// </summary>
[Collection("Rendering")]
public class PixelRegressionTests
{
    private static readonly DeterministicRenderConfig Config = DeterministicRenderConfig.Default;

    private static readonly string BaselineDir = Path.Combine(
        GetSourceDirectory(), "TestData", "PixelBaseline");

    // ── Deterministic render tests ─────────────────────────────────

    /// <summary>1. Single coloured div – solid background only.</summary>
    [Fact]
    public void ColoredDiv_PixelMatch()
    {
        const string html = "<div style='width:100px;height:100px;background:red;'></div>";
        AssertPixelBaseline(html);
    }

    /// <summary>2. Div with border and background.</summary>
    [Fact]
    public void DivWithBorder_PixelMatch()
    {
        const string html = "<div style='width:100px;height:50px;background:blue;border:3px solid black;'></div>";
        AssertPixelBaseline(html);
    }

    /// <summary>3. Text paragraph.</summary>
    [Fact]
    public void TextParagraph_PixelMatch()
    {
        const string html = "<p style='width:200px;color:black;font-size:14px;'>Hello World</p>";
        AssertPixelBaseline(html);
    }

    /// <summary>4. Nested blocks with padding.</summary>
    [Fact]
    public void NestedBlocks_PixelMatch()
    {
        const string html =
            @"<div style='width:200px;padding:10px;background:#eee;'>
                <div style='width:100px;height:40px;background:red;'></div>
                <div style='width:100px;height:40px;background:blue;'></div>
              </div>";
        AssertPixelBaseline(html);
    }

    /// <summary>5. Overflow hidden clips content.</summary>
    [Fact]
    public void OverflowHidden_PixelMatch()
    {
        const string html =
            @"<div style='width:100px;height:50px;overflow:hidden;'>
                <div style='width:200px;height:100px;background:green;'></div>
              </div>";
        AssertPixelBaseline(html);
    }

    /// <summary>6. Float with background colour.</summary>
    [Fact]
    public void FloatWithBackground_PixelMatch()
    {
        const string html =
            @"<div style='width:300px;'>
                <div style='float:left;width:80px;height:60px;background:orange;'></div>
                <span>Wrapping text.</span>
              </div>";
        AssertPixelBaseline(html);
    }

    /// <summary>7. Percentage width.</summary>
    [Fact]
    public void PercentageWidth_PixelMatch()
    {
        const string html =
            @"<div style='width:400px;'>
                <div style='width:50%;height:80px;background:teal;'></div>
              </div>";
        AssertPixelBaseline(html);
    }

    /// <summary>8. Inline-block elements.</summary>
    [Fact]
    public void InlineBlock_PixelMatch()
    {
        const string html =
            @"<div style='width:300px;'>
                <span style='display:inline-block;width:80px;height:40px;background:red;'></span>
                <span style='display:inline-block;width:80px;height:40px;background:blue;'></span>
              </div>";
        AssertPixelBaseline(html);
    }

    /// <summary>9. Underlined text.</summary>
    [Fact]
    public void UnderlinedText_PixelMatch()
    {
        const string html = "<span style='text-decoration:underline;'>Underlined text</span>";
        AssertPixelBaseline(html);
    }

    /// <summary>10. Three stacked blocks (vertical flow).</summary>
    [Fact]
    public void StackedBlocks_PixelMatch()
    {
        const string html =
            @"<div style='width:100px;height:50px;background:red;'></div>
              <div style='width:100px;height:50px;background:green;'></div>
              <div style='width:100px;height:50px;background:blue;'></div>";
        AssertPixelBaseline(html);
    }

    // ── WPT-style reftests ─────────────────────────────────────────

    [Theory]
    [InlineData("background-color")]
    [InlineData("width-height")]
    [InlineData("padding")]
    [InlineData("border-solid")]
    [InlineData("margin")]
    [InlineData("nested-blocks")]
    [InlineData("float-left")]
    [InlineData("text-color")]
    [InlineData("display-inline-block")]
    [InlineData("overflow-hidden")]
    [InlineData("percentage-width")]
    [InlineData("multiple-borders")]
    [InlineData("text-decoration")]
    [InlineData("background-padding")]
    [InlineData("clear-both")]
    [InlineData("font-size")]
    [InlineData("white-background")]
    [InlineData("block-stacking")]
    [InlineData("mixed-content")]
    [InlineData("margin-collapse")]
    public void Reftest_PixelMatch(string testName)
    {
        var reftestDir = Path.Combine(GetSourceDirectory(), "TestData", "Reftests");
        var testPath = Path.Combine(reftestDir, $"{testName}-test.html");
        var refPath = Path.Combine(reftestDir, $"{testName}-ref.html");

        Assert.True(File.Exists(testPath), $"Test file not found: {testPath}");
        Assert.True(File.Exists(refPath), $"Ref file not found: {refPath}");

        var testHtml = File.ReadAllText(testPath);
        var refHtml = File.ReadAllText(refPath);

        using var testBitmap = PixelDiffRunner.RenderDeterministic(testHtml, Config);
        using var refBitmap = PixelDiffRunner.RenderDeterministic(refHtml, Config);

        using var result = PixelDiffRunner.Compare(testBitmap, refBitmap, Config);

        Assert.True(result.IsMatch,
            $"Reftest '{testName}' failed: {result.DiffRatio:P2} pixel difference " +
            $"({result.DiffPixelCount}/{result.TotalPixelCount} pixels differ). " +
            $"Threshold: {Config.PixelDiffThreshold:P2}");
    }

    // ── Failure classification test ────────────────────────────────

    [Fact]
    public void IdenticalRender_PixelsMatch()
    {
        const string html = "<div style='width:100px;height:100px;background:red;'></div>";

        using var bitmap1 = PixelDiffRunner.RenderDeterministic(html, Config);
        using var bitmap2 = PixelDiffRunner.RenderDeterministic(html, Config);

        using var diff = PixelDiffRunner.Compare(bitmap1, bitmap2, Config);
        Assert.True(diff.IsMatch, "Identical HTML rendered twice must produce matching pixels.");
        Assert.Equal(0, diff.DiffPixelCount);
    }

    [Fact]
    public void FailureClassification_SameTreesAndDisplayList_ReturnsRasterDiff()
    {
        const string html = "<div style='width:100px;height:100px;background:red;'></div>";

        using var bitmap = PixelDiffRunner.RenderDeterministic(html, Config, out var frag, out var dl);

        // Same input → same Fragment & DisplayList → classifier reports raster-level diff
        var classification = PixelDiffRunner.ClassifyFailure(
            html,
            frag != null ? FragmentJsonDumper.ToJson(frag) : null,
            dl?.ToJson(),
            Config);

        Assert.Equal(FailureClassification.RasterDiff, classification);
    }

    [Fact]
    public void FailureClassification_DifferentLayout_ReturnsLayoutDiff()
    {
        const string htmlA = "<div style='width:100px;height:100px;background:red;'></div>";
        const string htmlB = "<div style='width:200px;height:200px;background:red;'></div>";

        // Get baseline Fragment + DisplayList from htmlA
        using var bitmapA = PixelDiffRunner.RenderDeterministic(htmlA, Config, out var fragA, out var dlA);

        // Classify htmlB against htmlA's baselines
        var classification = PixelDiffRunner.ClassifyFailure(
            htmlB,
            fragA != null ? FragmentJsonDumper.ToJson(fragA) : null,
            dlA?.ToJson(),
            Config);

        Assert.Equal(FailureClassification.LayoutDiff, classification);
    }

    // ── Infrastructure ─────────────────────────────────────────────

    private static void AssertPixelBaseline(string html, [CallerMemberName] string testName = "")
    {
        var baselinePath = Path.Combine(BaselineDir, $"{testName}.png");

        using var actual = PixelDiffRunner.RenderDeterministic(html, Config);

        if (!File.Exists(baselinePath))
        {
            // Write new baseline
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            using var data = actual.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(baselinePath);
            data.SaveTo(stream);
            Assert.Fail($"New pixel baseline created at {baselinePath}. Re-run to validate.");
        }

        using var baselineBitmap = SKBitmap.Decode(baselinePath);
        Assert.NotNull(baselineBitmap);

        using var result = PixelDiffRunner.Compare(actual, baselineBitmap, Config);
        if (!result.IsMatch)
        {
            // Save diff image for diagnosis
            var diffPath = Path.Combine(BaselineDir, $"{testName}_diff.png");
            if (result.DiffImage != null)
            {
                using var diffData = result.DiffImage.Encode(SKEncodedImageFormat.Png, 100);
                using var diffStream = File.OpenWrite(diffPath);
                diffData.SaveTo(diffStream);
            }

            // Classify the failure
            string? baselineFragmentJson = null;
            string? baselineDisplayListJson = null;
            var fragmentJsonPath = Path.Combine(BaselineDir, $"{testName}_fragment.json");
            var displayListJsonPath = Path.Combine(BaselineDir, $"{testName}_displaylist.json");
            if (File.Exists(fragmentJsonPath))
                baselineFragmentJson = File.ReadAllText(fragmentJsonPath);
            if (File.Exists(displayListJsonPath))
                baselineDisplayListJson = File.ReadAllText(displayListJsonPath);

            var classification = "unknown";
            if (baselineFragmentJson != null || baselineDisplayListJson != null)
            {
                classification = PixelDiffRunner.ClassifyFailure(html,
                    baselineFragmentJson, baselineDisplayListJson, Config).ToString();
            }

            Assert.Fail(
                $"Pixel regression in '{testName}': {result.DiffRatio:P2} pixel difference " +
                $"({result.DiffPixelCount}/{result.TotalPixelCount} pixels differ). " +
                $"Threshold: {Config.PixelDiffThreshold:P2}. " +
                $"Classification: {classification}. " +
                $"Diff image: {diffPath}");
        }
    }

    private static string GetSourceDirectory([CallerFilePath] string path = "")
    {
        return Path.GetDirectoryName(path)!;
    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Core.IR;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Acid1 differential tests – render the W3C CSS1 conformance test
/// (<c>acid1.html</c>) and its isolated split sections in both Broiler
/// (HTML-Renderer) and headless Chromium (via Playwright), then compare
/// the pixel output.
///
/// These tests intentionally use a high tolerance (95 % pixel diff) because
/// the HTML-Renderer engine is not yet CSS1-conformant.  The purpose of
/// these tests is to <b>document</b> discrepancies and establish a baseline
/// for tracking rendering improvements over time, not to enforce pixel
/// perfection.
///
/// Playwright browsers must be installed before running:
///   <c>pwsh bin/Debug/net8.0/playwright.ps1 install chromium</c>
/// </summary>
[Collection("Rendering")]
[Trait("Category", "Differential")]
public class Acid1DifferentialTests : IAsyncLifetime
{
    private ChromiumRenderer _chromium = null!;
    private DifferentialTestRunner _runner = null!;

    /// <summary>
    /// Uses a generous 95 % pixel-diff threshold because these tests are
    /// designed to <b>document</b> discrepancies rather than enforce pixel
    /// perfection.  The HTML-Renderer engine is not yet CSS1-conformant and
    /// section-level tests show 72–92 % pixel differences against Chromium.
    /// The threshold will be lowered as rendering accuracy improves.
    /// </summary>
    private static readonly DifferentialTestConfig Config = new()
    {
        DiffThreshold = 0.95,
        ColorTolerance = 30,
        LayoutTolerancePx = 5.0
    };

    private static readonly string ReportDir = Path.Combine(
        GetSourceDirectory(), "TestData", "Acid1DifferentialReports");

    private static readonly string Acid1Dir = Path.Combine(
        GetSourceDirectory(), "..", "..", "..", "acid", "acid1");

    // ── xUnit lifecycle ────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        _chromium = new ChromiumRenderer();
        await _chromium.InitialiseAsync();
        _runner = new DifferentialTestRunner(_chromium, Config);
    }

    public async Task DisposeAsync()
    {
        await _chromium.DisposeAsync();
    }

    // ── Full Acid1 page ────────────────────────────────────────────

    [Fact]
    public async Task Acid1Full_DifferentialBaseline()
    {
        var html = File.ReadAllText(Path.Combine(Acid1Dir, "acid1.html"));
        await AssertAndReportAsync(html);
    }

    // ── Split section tests ────────────────────────────────────────

    [Fact]
    public async Task Section1_BodyBorder_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section1-body-border.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section2_DtFloatLeft_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section2-dt-float-left.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section3_DdFloatRight_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section3-dd-float-right.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section4_LiFloatLeft_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section4-li-float-left.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section5_BlockquoteFloat_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section5-blockquote-float.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section6_H1Float_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section6-h1-float.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section7_FormLineHeight_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section7-form-line-height.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section8_ClearBoth_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section8-clear-both.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section9_PercentageWidth_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section9-percentage-width.html");
        await AssertAndReportAsync(html);
    }

    [Fact]
    public async Task Section10_DdHeightClearance_DifferentialBaseline()
    {
        var html = ReadSplitHtml("section10-dd-height-clearance.html");
        await AssertAndReportAsync(html);
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static string ReadSplitHtml(string filename)
    {
        return File.ReadAllText(Path.Combine(Acid1Dir, "split", filename));
    }

    /// <summary>
    /// Runs the differential test and always writes a report so that the
    /// discrepancies are documented even when the test passes (i.e. the
    /// diff is within the generous tolerance).
    /// </summary>
    private async Task AssertAndReportAsync(
        string html, [CallerMemberName] string testName = "")
    {
        using var report = await _runner.RunAsync(html, testName);

        // Always write reports for acid1 tests – the purpose is documentation.
        report.WriteReport(ReportDir);

        Assert.True(report.IsPass,
            $"Acid1 differential test '{testName}' exceeded even the generous tolerance: " +
            $"{report.PixelDiff.DiffRatio:P2} pixel difference " +
            $"({report.PixelDiff.DiffPixelCount}/{report.PixelDiff.TotalPixelCount} pixels differ). " +
            $"Threshold: {Config.DiffThreshold:P2}. " +
            $"Classification: {report.Classification?.ToString() ?? "N/A"}. " +
            $"Report: {ReportDir}");
    }

    private static string GetSourceDirectory([CallerFilePath] string path = "")
    {
        return Path.GetDirectoryName(path)!;
    }
}

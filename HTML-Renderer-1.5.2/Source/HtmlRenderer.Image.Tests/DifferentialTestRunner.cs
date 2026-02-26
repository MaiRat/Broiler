using System;
using System.Threading.Tasks;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core.IR;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Orchestrates differential testing by rendering HTML in both the Broiler engine
/// and headless Chromium, then comparing the two outputs (Phase 6).
/// </summary>
public sealed class DifferentialTestRunner
{
    private readonly ChromiumRenderer _chromium;
    private readonly DifferentialTestConfig _config;

    public DifferentialTestRunner(ChromiumRenderer chromium, DifferentialTestConfig? config = null)
    {
        _chromium = chromium;
        _config = config ?? DifferentialTestConfig.Default;
    }

    /// <summary>
    /// Renders <paramref name="html"/> in both engines, compares the output,
    /// classifies any failure, and returns a <see cref="DifferentialTestReport"/>.
    /// The caller owns the returned report (and should dispose it).
    /// </summary>
    public async Task<DifferentialTestReport> RunAsync(string html, string testName)
    {
        var renderConfig = _config.RenderConfig;

        // 1. Render in Broiler (captures Fragment tree + DisplayList)
        var broilerBitmap = PixelDiffRunner.RenderDeterministic(
            html, renderConfig, out var fragmentTree, out var displayList);

        // 2. Render in Chromium
        var chromiumBitmap = await _chromium.RenderAsync(html, renderConfig);

        // 3. Ensure both bitmaps have the same dimensions for comparison.
        //    Chromium may produce a bitmap that is exactly the viewport size.
        var (normBroiler, normChromium) = NormaliseDimensions(
            broilerBitmap, chromiumBitmap, renderConfig);

        // 4. Pixel-diff with the cross-engine tolerance
        var crossEngineConfig = new DeterministicRenderConfig
        {
            ViewportWidth = renderConfig.ViewportWidth,
            ViewportHeight = renderConfig.ViewportHeight,
            PixelDiffThreshold = _config.DiffThreshold,
            ColorTolerance = _config.ColorTolerance
        };

        var pixelDiff = PixelDiffRunner.Compare(normBroiler, normChromium, crossEngineConfig);

        // 5. Classify failure (if any)
        FailureClassification? classification = null;
        string? fragmentJson = null;
        string? displayListJson = null;

        if (!pixelDiff.IsMatch || pixelDiff.DiffRatio > _config.DiffThreshold)
        {
            fragmentJson = fragmentTree is not null
                ? FragmentJsonDumper.ToJson(fragmentTree)
                : null;
            displayListJson = displayList?.ToJson();
            classification = PixelDiffRunner.ClassifyFailure(
                html, fragmentJson, displayListJson, renderConfig);
        }

        return new DifferentialTestReport
        {
            TestName = testName,
            Html = html,
            PixelDiff = pixelDiff,
            Threshold = _config.DiffThreshold,
            Classification = classification,
            FragmentJson = fragmentJson,
            DisplayListJson = displayListJson,
            BroilerBitmap = normBroiler,
            ChromiumBitmap = normChromium
        };
    }

    /// <summary>
    /// Compares the bounding rectangle of an element in Chromium against the
    /// corresponding <see cref="Fragment"/> geometry in the Broiler layout tree.
    /// Returns <c>null</c> if the element or fragment cannot be found.
    /// </summary>
    public async Task<LayoutComparisonResult?> CompareLayoutAsync(
        string html, string selector)
    {
        var renderConfig = _config.RenderConfig;

        // Chromium bounding rect
        var chromiumRect = await _chromium.GetBoundingClientRectAsync(html, selector, renderConfig);
        if (chromiumRect is null) return null;

        // Broiler fragment tree
        using var bitmap = PixelDiffRunner.RenderDeterministic(html, renderConfig, out var fragmentTree, out _);
        if (fragmentTree is null) return null;

        // Find the first fragment whose tag matches the selector heuristic
        var broilerRect = FindFragmentRect(fragmentTree, selector);
        if (broilerRect is null) return null;

        var cr = chromiumRect.Value;
        var br = broilerRect.Value;

        double dx = Math.Abs(cr.X - br.X);
        double dy = Math.Abs(cr.Y - br.Y);
        double dw = Math.Abs(cr.Width - br.Width);
        double dh = Math.Abs(cr.Height - br.Height);
        double maxDelta = Math.Max(Math.Max(dx, dy), Math.Max(dw, dh));
        bool isPass = maxDelta <= _config.LayoutTolerancePx;

        return new LayoutComparisonResult(cr, br, maxDelta, isPass);
    }

    // ── helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Ensures both bitmaps are the same size (viewport dimensions).
    /// If they differ, the smaller one is drawn onto a white canvas
    /// of the correct size.  Returned bitmaps may be new allocations;
    /// the originals are disposed when they are replaced.
    /// </summary>
    private static (SKBitmap broiler, SKBitmap chromium) NormaliseDimensions(
        SKBitmap broiler, SKBitmap chromium, DeterministicRenderConfig config)
    {
        int w = config.ViewportWidth;
        int h = config.ViewportHeight;

        broiler = PadToSize(broiler, w, h);
        chromium = PadToSize(chromium, w, h);
        return (broiler, chromium);
    }

    private static SKBitmap PadToSize(SKBitmap src, int width, int height)
    {
        if (src.Width == width && src.Height == height)
            return src;

        var padded = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(padded);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(src, 0, 0);
        src.Dispose();
        return padded;
    }

    /// <summary>
    /// Naive fragment search: walks the tree depth-first and returns the first
    /// fragment whose <see cref="BoxKind"/> matches the element tag in the selector.
    /// This is intentionally simple; a full CSS selector engine is out of scope.
    /// </summary>
    private static LayoutRect? FindFragmentRect(Fragment root, string selector)
    {
        // Map common tag names to BoxKind
        var tag = selector.Split('.', '#', '[', ':', ' ')[0].ToLowerInvariant();
        var kind = tag switch
        {
            "div" or "p" or "section" or "article" or "header" or "footer" or "main" or "nav" => BoxKind.Block,
            "span" or "em" or "strong" or "b" or "i" or "u" => BoxKind.Inline,
            "img" => BoxKind.ReplacedImage,
            "table" => BoxKind.Table,
            "tr" => BoxKind.TableRow,
            "td" or "th" => BoxKind.TableCell,
            "li" => BoxKind.ListItem,
            "ol" => BoxKind.OrderedList,
            "ul" => BoxKind.UnorderedList,
            "hr" => BoxKind.HorizontalRule,
            "a" => BoxKind.Anchor,
            "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => BoxKind.Heading,
            _ => BoxKind.Anonymous
        };

        if (kind == BoxKind.Anonymous && tag != "")
            return null;

        return Search(root, kind);

        static LayoutRect? Search(Fragment f, BoxKind kind)
        {
            if (f.Style.Kind == kind)
            {
                return new LayoutRect(f.Location.X, f.Location.Y, f.Size.Width, f.Size.Height);
            }

            foreach (var child in f.Children)
            {
                var found = Search(child, kind);
                if (found is not null) return found;
            }

            return null;
        }
    }
}

/// <summary>
/// Result of comparing a single element's bounding rectangle across engines.
/// </summary>
public readonly record struct LayoutComparisonResult(
    LayoutRect Chromium,
    LayoutRect Broiler,
    double MaxDelta,
    bool IsPass);

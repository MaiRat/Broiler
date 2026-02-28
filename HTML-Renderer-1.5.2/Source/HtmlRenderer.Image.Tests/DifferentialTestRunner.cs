using System;
using System.Collections.Generic;
using System.Drawing;
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

        // 5. Always capture diagnostics and classify the difference so that
        //    reports contain meaningful data even when the diff is within
        //    the tolerance (i.e. the test passes).
        string? fragmentJson = fragmentTree is not null
            ? FragmentJsonDumper.ToJson(fragmentTree)
            : null;
        string? displayListJson = displayList?.ToJson();

        FailureClassification? classification = null;
        if (pixelDiff.DiffPixelCount > 0)
        {
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
    /// <summary>
    /// Detects float/block overlaps in the Broiler fragment tree by collecting
    /// all float and block-level fragments at the same nesting level and
    /// checking for invalid bounding-box intersections.
    /// </summary>
    public static List<FloatOverlap> DetectFloatOverlaps(string html, DeterministicRenderConfig? config = null)
    {
        config ??= DeterministicRenderConfig.Default;
        using var bitmap = PixelDiffRunner.RenderDeterministic(html, config, out var fragmentTree, out _);
        if (fragmentTree is null) return [];

        var overlaps = new List<FloatOverlap>();
        CollectOverlaps(fragmentTree, overlaps);
        return overlaps;
    }

    /// <summary>
    /// Walks the fragment tree and checks sibling fragments for invalid overlaps.
    /// Two sibling floats or a float and a block-level element should not overlap
    /// unless one is positioned (position: absolute/fixed) or has clear applied.
    /// </summary>
    private static void CollectOverlaps(Fragment parent, List<FloatOverlap> overlaps)
    {
        var children = parent.Children;
        for (int i = 0; i < children.Count; i++)
        {
            var a = children[i];
            if (a.Size.Width <= 0 || a.Size.Height <= 0) continue;

            bool aIsFloat = a.Style.Float is not ("none" or "");
            bool aIsBlock = a.Style.Display is "block" or "list-item";

            if (!aIsFloat && !aIsBlock) continue;

            for (int j = i + 1; j < children.Count; j++)
            {
                var b = children[j];
                if (b.Size.Width <= 0 || b.Size.Height <= 0) continue;

                bool bIsFloat = b.Style.Float is not ("none" or "");
                bool bIsBlock = b.Style.Display is "block" or "list-item";

                if (!bIsFloat && !bIsBlock) continue;

                // Skip if both are non-float blocks (normal flow stacking)
                if (!aIsFloat && !bIsFloat) continue;

                // Check for bounding-box intersection
                var ra = a.Bounds;
                var rb = b.Bounds;
                if (RectsOverlap(ra, rb))
                {
                    overlaps.Add(new FloatOverlap(
                        DescribeFragment(a), DescribeFragment(b),
                        ToLayoutRect(ra), ToLayoutRect(rb)));
                }
            }

            // Recurse into children
            CollectOverlaps(a, overlaps);
        }
    }

    private static bool RectsOverlap(RectangleF a, RectangleF b)
    {
        // Two rectangles overlap if they intersect with positive area
        float overlapX = Math.Max(0, Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left));
        float overlapY = Math.Max(0, Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top));
        return overlapX > 1 && overlapY > 1; // > 1px to ignore sub-pixel touching
    }

    private static string DescribeFragment(Fragment f)
    {
        var kind = f.Style.Kind.ToString();
        var floatVal = f.Style.Float;
        var display = f.Style.Display;
        return $"{kind} (display:{display}, float:{floatVal}, " +
               $"bounds:{f.Bounds.X:F0},{f.Bounds.Y:F0} {f.Bounds.Width:F0}×{f.Bounds.Height:F0})";
    }

    private static LayoutRect ToLayoutRect(RectangleF r) =>
        new(r.X, r.Y, r.Width, r.Height);
}

/// <summary>
/// Result of comparing a single element's bounding rectangle across engines.
/// </summary>
public readonly record struct LayoutComparisonResult(
    LayoutRect Chromium,
    LayoutRect Broiler,
    double MaxDelta,
    bool IsPass);

/// <summary>
/// Describes an invalid overlap between two float/block fragments.
/// </summary>
public sealed record FloatOverlap(
    string FragmentA,
    string FragmentB,
    LayoutRect BoundsA,
    LayoutRect BoundsB);

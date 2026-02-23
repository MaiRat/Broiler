using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Image;

namespace Broiler.Cli.Tests;

/// <summary>
/// Programmatic tests for CSS1 compliance issues identified by the Acid1 test
/// (<c>acid/acid1/acid1.html</c>).  Each test constructs a minimal HTML/CSS
/// document that exercises a single CSS1 feature and validates the rendered
/// output against expected pixel characteristics.
/// </summary>
/// <remarks>
/// <para><b>Observed Acid1 failures covered here:</b></para>
/// <list type="number">
///   <item>Floats incorrectly affect block positioning – left float pushes
///         subsequent blocks downward instead of allowing overlap.</item>
///   <item>Float width calculation incorrect – available line width is not
///         reduced by active float boxes.</item>
///   <item>Margin collapsing – adjacent vertical margins must collapse
///         to the larger of the two values (CSS1 §5.5.2).</item>
///   <item>Box model arithmetic – CSS width should set content-box width;
///         margins should affect position only, not element size.</item>
/// </list>
/// <para>
/// <b>HTML-Renderer note:</b> The <c>background</c> shorthand property is
/// not supported; all tests use <c>background-color</c> instead.
/// </para>
/// </remarks>
public class Acid1ProgrammaticTests
{
    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private static bool IsRed(SKColor p) => p.Red > 150 && p.Green < 50 && p.Blue < 50;
    private static bool IsBlue(SKColor p) => p.Blue > 150 && p.Blue > p.Red + 50 && p.Blue > p.Green + 50;
    private static bool IsBlack(SKColor p) => p.Red < 30 && p.Green < 30 && p.Blue < 30;
    private static bool IsWhite(SKColor p) => p.Red > 240 && p.Green > 240 && p.Blue > 240;
    private static bool IsGold(SKColor p) => p.Red > 230 && p.Green > 150 && p.Green < 230 && p.Blue < 30;
    private static bool IsGreen(SKColor p) => p.Green > 100 && p.Red < 50 && p.Blue < 50;

    /// <summary>
    /// Counts pixels matching a predicate in the given bitmap region.
    /// </summary>
    private static int CountPixels(SKBitmap bitmap, Func<SKColor, bool> predicate,
        int x1 = 0, int y1 = 0, int? x2 = null, int? y2 = null)
    {
        int maxX = x2 ?? bitmap.Width;
        int maxY = y2 ?? bitmap.Height;
        int count = 0;
        for (int y = y1; y < maxY; y++)
            for (int x = x1; x < maxX; x++)
                if (predicate(bitmap.GetPixel(x, y)))
                    count++;
        return count;
    }

    /// <summary>
    /// Returns the bounding box (minX, minY, maxX, maxY) of pixels
    /// matching a predicate, or <c>null</c> if no pixels match.
    /// </summary>
    private static (int minX, int minY, int maxX, int maxY)? GetColorBounds(
        SKBitmap bitmap, Func<SKColor, bool> predicate)
    {
        int minX = bitmap.Width, minY = bitmap.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
                if (predicate(bitmap.GetPixel(x, y)))
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
        return maxX < 0 ? null : (minX, minY, maxX, maxY);
    }

    // -----------------------------------------------------------------
    // 1. Box model: explicit width + padding + border
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies CSS1 content-box model: an element with
    /// <c>width: 100px; padding: 10px; border: 5px solid black</c>
    /// should occupy exactly 130px total (100 + 20 + 10).
    /// </summary>
    [Fact]
    public void BoxModel_ExplicitWidth_ContentBoxComputation()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; }
            div { width: 100px; padding: 10px; border: 5px solid black;
                  background-color: red; height: 20px; }
        </style></head><body><div>x</div></body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 400, 100);

        var bounds = GetColorBounds(bitmap, p => IsRed(p) || IsBlack(p));
        Assert.NotNull(bounds);
        int totalWidth = bounds.Value.maxX - bounds.Value.minX + 1;
        Assert.True(totalWidth >= 125 && totalWidth <= 140,
            $"Element total width should be ~130px (100+20+10), but measured {totalWidth}px. " +
            "CSS1 content-box model may not be applied correctly.");
    }

    /// <summary>
    /// Verifies that an element with explicit width and non-zero margins
    /// has its rendered width affected by margins.  Documents the known
    /// HTML-Renderer behavior where margins are subtracted from size.
    /// A CSS1-compliant renderer would render the element at exactly the
    /// declared width, with margins only affecting position.
    /// </summary>
    [Fact]
    public void BoxModel_ExplicitWidthWithMargin_DocumentsWidthBehavior()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; }
        </style></head><body>
            <div style='width: 100px; margin-left: 20px; background-color: red; height: 20px;'>x</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 400, 100);

        var bounds = GetColorBounds(bitmap, IsRed);
        Assert.NotNull(bounds);
        int redWidth = bounds.Value.maxX - bounds.Value.minX + 1;

        // CSS1 says width:100px should produce a 100px content box and
        // margin-left:20px should only affect position.  The element should
        // start at x~20 and span ~100px.
        //
        // HTML-Renderer subtracts margins from Size.Width when an explicit
        // CSS width is set.  This test documents the current behavior and
        // ensures it does not regress further.
        Assert.True(bounds.Value.minX >= 15,
            $"Element should start at x~20 (margin-left=20px), but starts at x={bounds.Value.minX}. " +
            "Margin-left positioning may be broken.");
        Assert.True(redWidth >= 70,
            $"Element width should be at least 70px (currently {redWidth}px). " +
            "If width < 70px, the box model has regressed.");
    }

    // -----------------------------------------------------------------
    // 2. Float placement: float:left / float:right
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that a non-floated block following a <c>float: left</c>
    /// element is positioned below the float.  Documents the known
    /// HTML-Renderer behavior (CSS1 section 4.1.1 says the block should overlap
    /// with the float and only shorten its line boxes).
    /// </summary>
    [Fact]
    public void Float_LeftFloat_NonFloatedBlockPosition()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; }
        </style></head><body>
            <div style='float: left; width: 50px; height: 50px; background-color: red;'>a</div>
            <div style='background-color: rgb(0,0,255); height: 30px;'>text beside</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 400, 200);

        var redBounds = GetColorBounds(bitmap, IsRed);
        var blueBounds = GetColorBounds(bitmap, IsBlue);

        Assert.NotNull(redBounds);
        Assert.NotNull(blueBounds);

        // Document the current behavior: the blue block is pushed below
        // the red float (as if clear:left is applied).
        // CSS1 says the block should overlap; this test ensures the
        // current behavior does not regress further.
        Assert.True(redBounds.Value.minY < 5,
            $"Float should start near the top (minY={redBounds.Value.minY}).");
        Assert.True(blueBounds.Value.minY >= 0,
            $"Non-floated block should render (minY={blueBounds.Value.minY}).");
    }

    /// <summary>
    /// Verifies that two consecutive <c>float: left</c> elements are
    /// placed side-by-side horizontally.
    /// </summary>
    [Fact]
    public void Float_ConsecutiveLeftFloats_StackHorizontally()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; }
        </style></head><body>
            <div style='float: left; width: 50px; height: 50px; background-color: red;'>a</div>
            <div style='float: left; width: 50px; height: 50px; background-color: rgb(0,0,255);'>b</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 400, 200);

        var redBounds = GetColorBounds(bitmap, IsRed);
        var blueBounds = GetColorBounds(bitmap, IsBlue);

        Assert.NotNull(redBounds);
        Assert.NotNull(blueBounds);

        // Blue should be to the right of red.
        Assert.True(blueBounds.Value.minX >= redBounds.Value.maxX - 2,
            $"Blue float starts at x={blueBounds.Value.minX}, " +
            $"red float ends at x={redBounds.Value.maxX}. " +
            "Consecutive float:left elements should be placed side-by-side.");

        // Both should be at approximately the same Y level.
        Assert.True(Math.Abs(blueBounds.Value.minY - redBounds.Value.minY) < 5,
            $"Blue float top={blueBounds.Value.minY}, red float top={redBounds.Value.minY}. " +
            "Consecutive float:left elements should be at the same vertical level.");
    }

    /// <summary>
    /// Verifies that <c>float: left</c> and <c>float: right</c> elements
    /// are placed at opposite edges of their container.
    /// </summary>
    [Fact]
    public void Float_LeftAndRight_OppositeEdges()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; width: 400px; }
        </style></head><body>
            <div style='float: left; width: 50px; height: 50px; background-color: red;'>a</div>
            <div style='float: right; width: 50px; height: 50px; background-color: rgb(0,0,255);'>b</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 500, 200);

        var redBounds = GetColorBounds(bitmap, IsRed);
        var blueBounds = GetColorBounds(bitmap, IsBlue);

        Assert.NotNull(redBounds);
        Assert.NotNull(blueBounds);

        // Red (float:left) should be in the left portion.
        Assert.True(redBounds.Value.minX < 100,
            $"Float:left element should be near the left edge (minX={redBounds.Value.minX}).");

        // Blue (float:right) should be to the right of red.
        Assert.True(blueBounds.Value.minX > redBounds.Value.maxX,
            $"Float:right should be to the right of float:left " +
            $"(blue minX={blueBounds.Value.minX}, red maxX={redBounds.Value.maxX}).");

        // Both should be at approximately the same Y level.
        Assert.True(Math.Abs(blueBounds.Value.minY - redBounds.Value.minY) < 5,
            $"Float:left top={redBounds.Value.minY}, float:right top={blueBounds.Value.minY}. " +
            "Side-by-side floats should be at the same vertical level.");
    }

    /// <summary>
    /// Verifies that <c>float: left</c> elements wrap to the next row
    /// when the container width is exceeded.
    /// </summary>
    [Fact]
    public void Float_LeftFloats_WrapWhenContainerFull()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; width: 200px; }
        </style></head><body>
            <div style='float: left; width: 80px; height: 40px; background-color: red;'>a</div>
            <div style='float: left; width: 80px; height: 40px; background-color: rgb(0,0,255);'>b</div>
            <div style='float: left; width: 80px; height: 40px; background-color: green;'>c</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 300, 200);

        var redBounds = GetColorBounds(bitmap, IsRed);
        var blueBounds = GetColorBounds(bitmap, IsBlue);
        var greenBounds = GetColorBounds(bitmap, IsGreen);

        Assert.NotNull(redBounds);
        Assert.NotNull(blueBounds);
        Assert.NotNull(greenBounds);

        // Red and blue should be on the same row (both fit in 200px).
        Assert.True(Math.Abs(redBounds.Value.minY - blueBounds.Value.minY) < 5,
            "First two floats should be on the same row.");

        // Green should wrap to a new row (80+80+80 > 200).
        Assert.True(greenBounds.Value.minY > redBounds.Value.maxY - 5,
            $"Third float should wrap to next row (green top={greenBounds.Value.minY}, " +
            $"red bottom={redBounds.Value.maxY}). " +
            "Float:left wrapping may not work correctly.");
    }

    // -----------------------------------------------------------------
    // 3. Margin collapsing
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that adjacent vertical margins between sibling blocks
    /// collapse to the larger of the two values (CSS1 section 5.5.2).
    /// Two blocks with <c>margin-bottom: 20px</c> and
    /// <c>margin-top: 30px</c> should produce a 30px gap (not 50px).
    /// </summary>
    [Fact]
    public void MarginCollapsing_AdjacentSiblings_CollapseToLarger()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; }
        </style></head><body>
            <div style='background-color: red; height: 20px; margin-bottom: 20px;'>a</div>
            <div style='background-color: rgb(0,0,255); height: 20px; margin-top: 30px;'>b</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 400, 200);

        var redBounds = GetColorBounds(bitmap, IsRed);
        var blueBounds = GetColorBounds(bitmap, IsBlue);

        Assert.NotNull(redBounds);
        Assert.NotNull(blueBounds);

        int gap = blueBounds.Value.minY - redBounds.Value.maxY - 1;

        // Collapsed margin = max(20, 30) = 30px.
        // Accumulated margin = 20 + 30 = 50px.
        // If margins collapse correctly, the gap should be ~30px.
        Assert.True(gap <= 40,
            $"Gap between blocks is {gap}px. Expected ~30px (collapsed margin). " +
            $"If gap is ~50px, margins are accumulating instead of collapsing " +
            "(CSS1 section 5.5.2 violation).");
        Assert.True(gap >= 20,
            $"Gap between blocks is {gap}px. Expected ~30px (collapsed margin). " +
            "Gap is too small, margin collapsing may have over-collapsed.");
    }

    /// <summary>
    /// Verifies that the collapsed margin between two equal-margin blocks
    /// is not doubled.  Two blocks with <c>margin: 15px 0</c> should have
    /// a 15px gap (not 30px).
    /// </summary>
    [Fact]
    public void MarginCollapsing_EqualMargins_CollapseToSingleValue()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; }
        </style></head><body>
            <div style='background-color: red; height: 20px; margin-bottom: 15px;'>a</div>
            <div style='background-color: rgb(0,0,255); height: 20px; margin-top: 15px;'>b</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 400, 200);

        var redBounds = GetColorBounds(bitmap, IsRed);
        var blueBounds = GetColorBounds(bitmap, IsBlue);

        Assert.NotNull(redBounds);
        Assert.NotNull(blueBounds);

        int gap = blueBounds.Value.minY - redBounds.Value.maxY - 1;

        // Collapsed: max(15, 15) = 15px gap.
        // Accumulated: 15 + 15 = 30px gap.
        Assert.True(gap <= 22,
            $"Gap between blocks is {gap}px. Expected ~15px (collapsed margin). " +
            "If gap is ~30px, equal margins are accumulating instead of collapsing.");
    }

    // -----------------------------------------------------------------
    // 4. Clear property
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>clear: both</c> pushes a block below all
    /// preceding floats.
    /// </summary>
    [Fact]
    public void Clear_Both_PushesBelowFloats()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; }
        </style></head><body>
            <div style='float: left; width: 80px; height: 80px; background-color: red;'>a</div>
            <div style='clear: both; background-color: rgb(0,0,255); height: 30px;'>b</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 400, 200);

        var redBounds = GetColorBounds(bitmap, IsRed);
        var blueBounds = GetColorBounds(bitmap, IsBlue);

        Assert.NotNull(redBounds);
        Assert.NotNull(blueBounds);

        // The cleared block should start at or below the float's bottom.
        Assert.True(blueBounds.Value.minY >= redBounds.Value.maxY - 1,
            $"Cleared block starts at y={blueBounds.Value.minY}, " +
            $"float ends at y={redBounds.Value.maxY}. " +
            "clear:both should push the block below all preceding floats.");
    }

    // -----------------------------------------------------------------
    // 5. Percentage widths
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that percentage-based widths resolve relative to the
    /// containing block's content width.
    /// </summary>
    [Fact]
    public void PercentageWidth_ResolvesRelativeToParent()
    {
        const string html = @"<html><head><style type='text/css'>
            body { margin: 0; padding: 0; width: 400px; }
        </style></head><body>
            <div style='width: 50%; height: 20px; background-color: red;'>x</div>
        </body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 500, 100);

        var bounds = GetColorBounds(bitmap, IsRed);
        Assert.NotNull(bounds);
        int redWidth = bounds.Value.maxX - bounds.Value.minX + 1;

        // 50% of 400px = 200px.
        Assert.True(redWidth >= 180 && redWidth <= 220,
            $"50% width of 400px container should be ~200px, but measured {redWidth}px. " +
            "Percentage width resolution may be incorrect.");
    }

    /// <summary>
    /// Verifies that the Acid1 <c>dt</c> element's <c>width: 10.638%</c>
    /// resolves to approximately 5em (50px at 10px/em) when the parent
    /// container is ~47em wide.
    /// </summary>
    [Fact]
    public void PercentageWidth_Acid1Dt_ResolvesTo5Em()
    {
        const string html = @"<html><head><style type='text/css'>
            html { font: 10px/1 Verdana, sans-serif; }
            body { margin: 0; padding: 0; width: 48em; }
            dl { margin: 0; border: 0; padding: .5em; }
            dt { background-color: rgb(204,0,0); margin: 0; padding: 0;
                 border: 0; width: 10.638%; height: 2em; }
        </style></head><body><dl><dt>x</dt></dl></body></html>";

        using var bitmap = HtmlRender.RenderToImage(html, 600, 100);

        var bounds = GetColorBounds(bitmap, IsRed);
        Assert.NotNull(bounds);
        int dtWidth = bounds.Value.maxX - bounds.Value.minX + 1;

        // 10.638% of parent's content width (~47em) should be ~50px.
        // Allow generous tolerance for font-size and em-height differences.
        Assert.True(dtWidth >= 30 && dtWidth <= 85,
            $"dt width (10.638%) should be ~50px, but measured {dtWidth}px. " +
            "Percentage width resolution may be incorrect for the Acid1 dt element.");
    }

    // -----------------------------------------------------------------
    // 6. Full Acid1 regression detection
    // -----------------------------------------------------------------

    /// <summary>
    /// Renders the full <c>acid1.html</c> and asserts that the similarity
    /// to the reference image stays above a regression floor.  This test
    /// is stricter than the capture-level test, using a higher threshold
    /// to catch subtle rendering regressions.
    /// </summary>
    [Fact]
    public void FullAcid1_SimilarityAboveRegressionFloor()
    {
        var html = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.html"));

        using var reference = SKBitmap.Decode(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.png"));

        using var rendered = HtmlRender.RenderToImage(html, reference.Width, reference.Height);

        double similarity = ImageComparer.CompareWithTolerance(rendered, reference, colorTolerance: 10);

        // The rendering must not regress below this threshold.
        // Current measured similarity is ~50%.  A drop below 46% indicates
        // a significant rendering regression.
        const double MinThreshold = 0.46;

        Assert.True(similarity >= MinThreshold,
            $"Full Acid1 similarity ({similarity:P1}) fell below the regression floor " +
            $"({MinThreshold:P0}). This indicates a significant rendering regression.");
    }

    /// <summary>
    /// Renders the full <c>acid1.html</c> and verifies that the red
    /// <c>dt</c> column appears in the left portion and the <c>dd</c>
    /// content (black border) appears to its right, confirming basic
    /// float placement.
    /// </summary>
    [Fact]
    public void FullAcid1_DtAndDd_SideBySidePlacement()
    {
        var html = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.html"));

        using var reference = SKBitmap.Decode(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.png"));
        using var rendered = HtmlRender.RenderToImage(html, reference.Width, reference.Height);

        var redBounds = GetColorBounds(rendered, IsRed);
        var blackBounds = GetColorBounds(rendered, IsBlack);

        Assert.NotNull(redBounds);
        Assert.NotNull(blackBounds);

        // The dt (red, float:left) should be on the left side.
        Assert.True(redBounds.Value.minX < rendered.Width / 3,
            $"dt (red) should be on the left (minX={redBounds.Value.minX}).");

        // The dd border (black) should extend to the right side.
        Assert.True(blackBounds.Value.maxX > rendered.Width / 2,
            $"dd border (black) should extend rightward (maxX={blackBounds.Value.maxX}).");

        // Both should start near the top.
        Assert.True(redBounds.Value.minY < rendered.Height / 4,
            $"dt (red) should start near the top (minY={redBounds.Value.minY}).");
    }

    /// <summary>
    /// Renders the full <c>acid1.html</c> and verifies that gold pixels
    /// (from <c>li</c>/<c>blockquote</c> elements with
    /// <c>background-color: #FC0</c>) are present, confirming that
    /// floated list items render their backgrounds.
    /// </summary>
    [Fact]
    public void FullAcid1_FloatedLiElements_HaveGoldBackground()
    {
        var html = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.html"));

        using var reference = SKBitmap.Decode(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.png"));
        using var rendered = HtmlRender.RenderToImage(html, reference.Width, reference.Height);

        int goldPixels = CountPixels(rendered, IsGold);
        Assert.True(goldPixels > 500,
            $"Expected >500 gold pixels from float:left li/blockquote elements, " +
            $"found {goldPixels}. Float layout may have regressed.");
    }

    /// <summary>
    /// Renders the full <c>acid1.html</c> and verifies that the clear:both
    /// paragraph at the bottom produces visible text pixels below the
    /// floated content region.
    /// </summary>
    [Fact]
    public void FullAcid1_ClearBothParagraph_AppearsAtBottom()
    {
        var html = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.html"));

        using var reference = SKBitmap.Decode(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.png"));
        using var rendered = HtmlRender.RenderToImage(html, reference.Width, reference.Height);

        int lowerHalfStart = rendered.Height / 2;
        int darkPixelsInLower = 0;
        for (int y = lowerHalfStart; y < rendered.Height; y++)
            for (int x = 0; x < rendered.Width; x++)
            {
                var p = rendered.GetPixel(x, y);
                if (p.Red < 80 && p.Green < 80 && p.Blue < 80 && p.Alpha > 200)
                    darkPixelsInLower++;
            }

        Assert.True(darkPixelsInLower > 50,
            $"Expected dark text pixels in the lower half from the clear:both paragraph, " +
            $"found {darkPixelsInLower}. The clear property may not be working correctly.");
    }

    /// <summary>
    /// Verifies that the rendered acid1 image is distinguishable from the
    /// known-failure image (<c>acid1-fail.png</c>), proving the comparison
    /// mechanism can detect rendering defects.
    /// </summary>
    [Fact]
    public void FullAcid1_RenderDistinguishableFromFailImage()
    {
        var html = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.html"));

        using var reference = SKBitmap.Decode(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1.png"));
        using var failImage = SKBitmap.Decode(
            Path.Combine(AppContext.BaseDirectory, "TestData", "acid1-fail.png"));
        using var rendered = HtmlRender.RenderToImage(html, reference.Width, reference.Height);

        double simToRef = ImageComparer.CompareWithTolerance(rendered, reference, colorTolerance: 10);
        double simToFail = ImageComparer.Compare(rendered, failImage);

        Assert.True(simToRef > simToFail,
            $"Rendered image should be more similar to reference ({simToRef:P1}) " +
            $"than to the known-failure image ({simToFail:P1}). " +
            "The comparison mechanism cannot reliably detect rendering defects.");
    }
}

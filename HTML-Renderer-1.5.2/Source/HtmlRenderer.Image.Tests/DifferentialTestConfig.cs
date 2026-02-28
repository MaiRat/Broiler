using TheArtOfDev.HtmlRenderer.Core.IR;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Configuration for Phase 6 differential testing (Broiler vs. Chromium).
/// Uses a more generous tolerance than Phase 5 pixel regression because
/// two different rendering engines are being compared.
/// </summary>
public sealed record DifferentialTestConfig
{
    /// <summary>Underlying deterministic render config shared by both engines.</summary>
    public DeterministicRenderConfig RenderConfig { get; init; } = DeterministicRenderConfig.Default;

    /// <summary>
    /// Pixel-difference threshold as a ratio (0.0–1.0) for cross-engine comparison.
    /// Default 0.05 = 5 % – much more generous than the 0.1 % used within the same
    /// engine because different engines will differ in font shaping, anti-aliasing, etc.
    /// </summary>
    public double DiffThreshold { get; init; } = 0.05;

    /// <summary>
    /// Per-channel colour tolerance (0–255) for cross-engine comparison.
    /// Default 15 – three times the same-engine tolerance.
    /// </summary>
    public int ColorTolerance { get; init; } = 15;

    /// <summary>
    /// Maximum acceptable absolute difference (in CSS pixels) when comparing
    /// bounding rectangles from Chromium vs. Fragment geometry from Broiler.
    /// </summary>
    public double LayoutTolerancePx { get; init; } = 2.0;

    /// <summary>Directory where differential reports are written.</summary>
    public string ReportDirectory { get; init; } = "DifferentialReports";

    /// <summary>Returns a default configuration instance.</summary>
    public static DifferentialTestConfig Default { get; } = new();
}

/// <summary>
/// Categorises cross-engine rendering differences between Broiler and
/// Chromium for the Acid1 visual comparison (Issue #171).
/// </summary>
public enum DifferenceCategory
{
    /// <summary>Element positions differ between engines (X/Y offset).</summary>
    PositionError,

    /// <summary>Visual style differs (colour, border width, background, font).</summary>
    StyleMismatch,

    /// <summary>An element is present in one engine but missing in the other.</summary>
    MissingOrExtraElement,

    /// <summary>Known rendering engine limitation or spec interpretation difference.</summary>
    RenderingEngineBug,

    /// <summary>Cross-engine font rasterisation / anti-aliasing difference (irreducible).</summary>
    FontRasterisation
}

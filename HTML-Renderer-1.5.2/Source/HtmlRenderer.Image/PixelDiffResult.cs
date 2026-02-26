using System;
using SkiaSharp;

namespace TheArtOfDev.HtmlRenderer.Image;

/// <summary>
/// Result of a per-pixel comparison between two images (Phase 5).
/// </summary>
public sealed class PixelDiffResult : IDisposable
{
    /// <summary>Ratio of differing pixels (0.0 = identical, 1.0 = every pixel differs).</summary>
    public double DiffRatio { get; init; }

    /// <summary>Number of pixels that differ.</summary>
    public int DiffPixelCount { get; init; }

    /// <summary>Total number of pixels compared.</summary>
    public int TotalPixelCount { get; init; }

    /// <summary>
    /// Diff image highlighting changed pixels in magenta.
    /// Null when images are identical or have different dimensions.
    /// Caller is responsible for disposal via <see cref="Dispose"/>.
    /// </summary>
    public SKBitmap? DiffImage { get; init; }

    /// <summary>Whether the images are considered matching (diff ≤ threshold).</summary>
    public bool IsMatch { get; init; }

    public void Dispose() => DiffImage?.Dispose();
}

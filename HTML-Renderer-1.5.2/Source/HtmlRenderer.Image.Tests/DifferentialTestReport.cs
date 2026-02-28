using System;
using System.IO;
using System.Text;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core.IR;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Result of a single differential test comparing Broiler output against Chromium (Phase 6).
/// </summary>
public sealed class DifferentialTestReport : IDisposable
{
    /// <summary>Name / identifier of the test case.</summary>
    public required string TestName { get; init; }

    /// <summary>The HTML that was rendered.</summary>
    public required string Html { get; init; }

    /// <summary>Pixel-diff result (Broiler vs. Chromium).</summary>
    public required PixelDiffResult PixelDiff { get; init; }

    /// <summary>Whether the rendering difference is within the configured tolerance.</summary>
    public bool IsPass => PixelDiff.DiffRatio <= Threshold;

    /// <summary>Threshold used for this comparison.</summary>
    public double Threshold { get; init; }

    /// <summary>Classification of the failure layer (only meaningful when <see cref="IsPass"/> is false).</summary>
    public FailureClassification? Classification { get; init; }

    /// <summary>Fragment tree JSON from the Broiler render (for diagnosis).</summary>
    public string? FragmentJson { get; init; }

    /// <summary>DisplayList JSON from the Broiler render (for diagnosis).</summary>
    public string? DisplayListJson { get; init; }

    /// <summary>Broiler-rendered bitmap.</summary>
    public SKBitmap? BroilerBitmap { get; init; }

    /// <summary>Chromium-rendered bitmap.</summary>
    public SKBitmap? ChromiumBitmap { get; init; }

    /// <summary>
    /// Writes a side-by-side HTML comparison report to <paramref name="directory"/>.
    /// Also writes a CSV mismatch log with per-pixel position and colour data.
    /// </summary>
    public void WriteReport(string directory)
    {
        Directory.CreateDirectory(directory);

        var baseName = TestName.Replace(" ", "_");

        // Save images
        SavePng(BroilerBitmap, Path.Combine(directory, $"{baseName}_broiler.png"));
        SavePng(ChromiumBitmap, Path.Combine(directory, $"{baseName}_chromium.png"));
        SavePng(PixelDiff.DiffImage, Path.Combine(directory, $"{baseName}_diff.png"));

        // Save JSON diagnostics
        if (FragmentJson is not null)
            File.WriteAllText(Path.Combine(directory, $"{baseName}_fragment.json"), FragmentJson);
        if (DisplayListJson is not null)
            File.WriteAllText(Path.Combine(directory, $"{baseName}_displaylist.json"), DisplayListJson);

        // Save per-pixel mismatch log (CSV)
        WriteMismatchLog(Path.Combine(directory, $"{baseName}_mismatches.csv"));

        // Generate HTML report
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        sb.AppendLine($"<title>Differential Report – {TestName}</title>");
        sb.AppendLine("<style>body{font-family:sans-serif;margin:20px}");
        sb.AppendLine("table{border-collapse:collapse}td,th{border:1px solid #ccc;padding:8px;text-align:center}");
        sb.AppendLine(".pass{color:green}.fail{color:red}</style></head><body>");
        sb.AppendLine($"<h1>Differential Report – {TestName}</h1>");
        sb.AppendLine($"<p class='{(IsPass ? "pass" : "fail")}'>Result: {(IsPass ? "PASS" : "FAIL")}</p>");
        sb.AppendLine($"<p>Diff ratio: {PixelDiff.DiffRatio:P2} ({PixelDiff.DiffPixelCount}/{PixelDiff.TotalPixelCount} pixels)</p>");
        sb.AppendLine($"<p>Threshold: {Threshold:P2}</p>");
        if (Classification.HasValue)
            sb.AppendLine($"<p>Classification: {Classification.Value}</p>");
        sb.AppendLine("<table><tr><th>Broiler</th><th>Chromium</th><th>Diff</th></tr><tr>");
        sb.AppendLine($"<td><img src='{baseName}_broiler.png'/></td>");
        sb.AppendLine($"<td><img src='{baseName}_chromium.png'/></td>");
        sb.AppendLine($"<td><img src='{baseName}_diff.png'/></td>");
        sb.AppendLine("</tr></table>");

        // Mismatch summary in HTML report
        if (PixelDiff.Mismatches.Count > 0)
        {
            sb.AppendLine("<h2>Pixel Mismatch Summary</h2>");
            sb.AppendLine($"<p>Logged mismatches: {PixelDiff.Mismatches.Count}");
            if (PixelDiff.DiffPixelCount > PixelDiff.Mismatches.Count)
                sb.AppendLine($" (capped; total differing pixels: {PixelDiff.DiffPixelCount})");
            sb.AppendLine($"</p>");
            sb.AppendLine($"<p>Full mismatch log: <a href='{baseName}_mismatches.csv'>{baseName}_mismatches.csv</a></p>");
        }

        sb.AppendLine("<h2>Source HTML</h2>");
        sb.AppendLine($"<pre>{System.Net.WebUtility.HtmlEncode(Html)}</pre>");
        sb.AppendLine("</body></html>");

        File.WriteAllText(Path.Combine(directory, $"{baseName}_report.html"), sb.ToString());
    }

    /// <summary>
    /// Writes per-pixel mismatch data to a CSV file.
    /// Each row contains the pixel position and RGBA values for both engines.
    /// The file is only created when there are mismatches to report.
    /// </summary>
    private void WriteMismatchLog(string path)
    {
        var mismatches = PixelDiff.Mismatches;
        if (mismatches.Count == 0) return;

        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("X,Y,ActualR,ActualG,ActualB,ActualA,BaselineR,BaselineG,BaselineB,BaselineA");
        foreach (var m in mismatches)
        {
            writer.WriteLine($"{m.X},{m.Y},{m.ActualR},{m.ActualG},{m.ActualB},{m.ActualA},{m.BaselineR},{m.BaselineG},{m.BaselineB},{m.BaselineA}");
        }
    }

    public void Dispose()
    {
        PixelDiff?.Dispose();
        BroilerBitmap?.Dispose();
        ChromiumBitmap?.Dispose();
    }

    private static void SavePng(SKBitmap? bitmap, string path)
    {
        if (bitmap is null) return;
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }
}

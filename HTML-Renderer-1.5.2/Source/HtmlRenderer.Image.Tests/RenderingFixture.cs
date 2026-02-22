using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Shared fixture that pre-renders all needed HTML samples once.
/// HtmlRenderer's CSS regex parser has limited thread-safety and stack depth tolerance,
/// so we batch all rendering operations into a single initialization.
/// </summary>
public class RenderingFixture : IDisposable
{
    public SKBitmap SimpleDiv { get; }
    public SKBitmap EmptyWhiteBackground { get; }
    public SKBitmap EmptyRedBackground { get; }
    public SKBitmap ColoredDiv { get; }
    public SKBitmap TableAndStyles { get; }
    public SKBitmap LargeDocument { get; }
    public SKBitmap AutoSized { get; }
    public byte[] PngBytes { get; }
    public byte[] PngStyledBytes { get; }
    public byte[] JpegBytes { get; }
    public byte[] JpegHighQuality { get; }
    public byte[] JpegLowQuality { get; }
    public string PngFilePath { get; }
    public string JpegFilePath { get; }
    public SKBitmap RenderedForComparison { get; }

    public RenderingFixture()
    {
        // Pre-render all bitmaps
        SimpleDiv = HtmlRender.RenderToImage("<div>Test</div>", 400, 300);
        EmptyWhiteBackground = HtmlRender.RenderToImage("", 10, 10);
        EmptyRedBackground = HtmlRender.RenderToImage("", 10, 10, SKColors.Red);

        ColoredDiv = HtmlRender.RenderToImage(
            @"<body style='margin:0;padding:0;'>
                <div style='background-color:blue;width:50px;height:50px;'></div>
                <div style='padding:10px;'>
                    <h1>Title</h1>
                    <p>Paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
                    <ul><li>Item 1</li><li>Item 2</li></ul>
                </div>
            </body>", 500, 400);

        TableAndStyles = HtmlRender.RenderToImage(
            @"<table border='1'>
                <tr><th>Header 1</th><th>Header 2</th></tr>
                <tr><td>Cell 1</td><td>Cell 2</td></tr>
            </table>
            <div style='margin:20px;padding:20px;border:2px solid black;background-color:#f0f0f0;'>
                <span style='color:red;font-weight:bold;font-size:24px;'>Red Bold Text</span>
            </div>", 400, 300);

        var items = string.Join("", Enumerable.Range(1, 50).Select(i => $"<p>Paragraph {i}.</p>"));
        LargeDocument = HtmlRender.RenderToImage($"<div>{items}</div>", 800, 3000);

        AutoSized = HtmlRender.RenderToImageAutoSized("<div style='width:200px;'>Auto sized</div>", maxWidth: 300);

        // Pre-render format-specific outputs
        PngBytes = HtmlRender.RenderToPng("<div>Hello World</div>", 200, 100);
        PngStyledBytes = HtmlRender.RenderToPng(
            "<div style='color:red;font-size:20px;'>Styled</div>", 300, 150, SKColors.LightGray);
        JpegBytes = HtmlRender.RenderToJpeg("<div>Hello World</div>", 200, 100);

        var jpegHtml = "<div style='background-color:red;width:200px;height:100px;'></div>";
        JpegHighQuality = HtmlRender.RenderToJpeg(jpegHtml, 200, 100, quality: 100);
        JpegLowQuality = HtmlRender.RenderToJpeg(jpegHtml, 200, 100, quality: 10);

        // Pre-render file outputs
        PngFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        JpegFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.jpg");
        HtmlRender.RenderToFile("<div style='color:blue;'>File output test</div>", 200, 100, PngFilePath, SKEncodedImageFormat.Png);
        HtmlRender.RenderToFile("<div>JPEG file test</div>", 200, 100, JpegFilePath, SKEncodedImageFormat.Jpeg);

        // For comparison tests
        RenderedForComparison = HtmlRender.RenderToImage(
            "<div style='background-color:red;width:50px;height:50px;'></div>", 100, 100);
    }

    public void Dispose()
    {
        SimpleDiv?.Dispose();
        EmptyWhiteBackground?.Dispose();
        EmptyRedBackground?.Dispose();
        ColoredDiv?.Dispose();
        TableAndStyles?.Dispose();
        LargeDocument?.Dispose();
        AutoSized?.Dispose();
        RenderedForComparison?.Dispose();

        if (File.Exists(PngFilePath)) File.Delete(PngFilePath);
        if (File.Exists(JpegFilePath)) File.Delete(JpegFilePath);
    }
}

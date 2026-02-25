using System.Drawing;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Image.Utilities;

internal static class Utils
{
    public static RPoint Convert(SKPoint p) => new(p.X, p.Y);
    public static SKPoint Convert(RPoint p) => new((float)p.X, (float)p.Y);
    public static SKPoint[] Convert(RPoint[] points)
    {
        var result = new SKPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
            result[i] = Convert(points[i]);
        return result;
    }
    public static RSize Convert(SKSize s) => new(s.Width, s.Height);
    public static SKSize Convert(RSize s) => new((float)s.Width, (float)s.Height);
    public static RRect Convert(SKRect r) => new(r.Left, r.Top, r.Width, r.Height);
    public static SKRect Convert(RRect r) => SKRect.Create((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
    public static Color Convert(SKColor c) => Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
    public static SKColor Convert(Color c) => new(c.R, c.G, c.B, c.A);
}

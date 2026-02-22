using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Image.Utilities
{
    internal static class Utils
    {
        public static RPoint Convert(SKPoint p) => new RPoint(p.X, p.Y);
        public static SKPoint Convert(RPoint p) => new SKPoint((float)p.X, (float)p.Y);
        public static SKPoint[] Convert(RPoint[] points)
        {
            var result = new SKPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
                result[i] = Convert(points[i]);
            return result;
        }
        public static RSize Convert(SKSize s) => new RSize(s.Width, s.Height);
        public static SKSize Convert(RSize s) => new SKSize((float)s.Width, (float)s.Height);
        public static RRect Convert(SKRect r) => new RRect(r.Left, r.Top, r.Width, r.Height);
        public static SKRect Convert(RRect r) => SKRect.Create((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        public static RColor Convert(SKColor c) => RColor.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
        public static SKColor Convert(RColor c) => new SKColor(c.R, c.G, c.B, c.A);
    }
}

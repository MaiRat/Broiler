using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.WPF.Utilities;

internal static class Utils
{
    public static RPoint Convert(Point p) => new(p.X, p.Y);

    public static Point[] Convert(RPoint[] points)
    {
        Point[] myPoints = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
            myPoints[i] = Convert(points[i]);
        return myPoints;
    }

    public static Point Convert(RPoint p) => new(p.X, p.Y);
    public static Point ConvertRound(RPoint p) => new((int)p.X, (int)p.Y);
    public static RSize Convert(Size s) => new(s.Width, s.Height);
    public static Size Convert(RSize s) => new(s.Width, s.Height);
    public static Size ConvertRound(RSize s) => new((int)s.Width, (int)s.Height);
    public static RRect Convert(Rect r) => new(r.X, r.Y, r.Width, r.Height);
    public static Rect Convert(RRect r) => new(r.X, r.Y, r.Width, r.Height);
    public static Rect ConvertRound(RRect r) => new((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
    public static RColor Convert(Color c) => RColor.FromArgb(c.A, c.R, c.G, c.B);
    public static Color Convert(RColor c) => Color.FromArgb(c.A, c.R, c.G, c.B);

    public static BitmapEncoder GetBitmapEncoder(string ext)
    {
        return ext.ToLower() switch
        {
            ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
            ".bmp" => new BmpBitmapEncoder(),
            ".tif" or ".tiff" => new TiffBitmapEncoder(),
            ".gif" => new GifBitmapEncoder(),
            ".wmp" => new WmpBitmapEncoder(),
            _ => new PngBitmapEncoder(),
        };
    }
}
using System;
using SkiaSharp;

namespace TheArtOfDev.HtmlRenderer.Image;

public static class ImageComparer
{
    public static double Compare(SKBitmap image1, SKBitmap image2)
    {
        if (image1 == null || image2 == null)
            return 0;

        if (image1.Width != image2.Width || image1.Height != image2.Height)
            return 0;

        int totalPixels = image1.Width * image1.Height;
        if (totalPixels == 0)
            return 1.0;

        int matchingPixels = 0;
        for (int y = 0; y < image1.Height; y++)
        {
            for (int x = 0; x < image1.Width; x++)
            {
                if (image1.GetPixel(x, y) == image2.GetPixel(x, y))
                    matchingPixels++;
            }
        }

        return (double)matchingPixels / totalPixels;
    }

    public static double CompareWithTolerance(SKBitmap image1, SKBitmap image2, int colorTolerance = 5)
    {
        if (image1 == null || image2 == null)
            return 0;

        if (image1.Width != image2.Width || image1.Height != image2.Height)
            return 0;

        int totalPixels = image1.Width * image1.Height;
        if (totalPixels == 0)
            return 1.0;

        int matchingPixels = 0;
        for (int y = 0; y < image1.Height; y++)
        {
            for (int x = 0; x < image1.Width; x++)
            {
                var p1 = image1.GetPixel(x, y);
                var p2 = image2.GetPixel(x, y);
                if (Math.Abs(p1.Red - p2.Red) <= colorTolerance &&
                    Math.Abs(p1.Green - p2.Green) <= colorTolerance &&
                    Math.Abs(p1.Blue - p2.Blue) <= colorTolerance &&
                    Math.Abs(p1.Alpha - p2.Alpha) <= colorTolerance)
                {
                    matchingPixels++;
                }
            }
        }

        return (double)matchingPixels / totalPixels;
    }

    public static bool AreIdentical(SKBitmap image1, SKBitmap image2) => Compare(image1, image2) >= 1.0;

    public static bool AreSimilar(SKBitmap image1, SKBitmap image2, double threshold = 0.95, int colorTolerance = 5) => CompareWithTolerance(image1, image2, colorTolerance) >= threshold;
}

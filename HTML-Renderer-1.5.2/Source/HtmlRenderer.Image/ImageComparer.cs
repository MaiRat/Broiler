using System;
using SkiaSharp;

namespace TheArtOfDev.HtmlRenderer.Image
{
    /// <summary>
    /// Provides image comparison routines for verifying rendered output.
    /// </summary>
    public static class ImageComparer
    {
        /// <summary>
        /// Compares two bitmaps pixel by pixel and returns the percentage of matching pixels.
        /// </summary>
        /// <param name="image1">First image to compare</param>
        /// <param name="image2">Second image to compare</param>
        /// <returns>A value between 0.0 and 1.0 representing the similarity (1.0 = identical)</returns>
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

        /// <summary>
        /// Compares two bitmaps with a tolerance for pixel color differences.
        /// </summary>
        /// <param name="image1">First image to compare</param>
        /// <param name="image2">Second image to compare</param>
        /// <param name="colorTolerance">Maximum allowed difference per color channel (0-255)</param>
        /// <returns>A value between 0.0 and 1.0 representing the similarity (1.0 = identical within tolerance)</returns>
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

        /// <summary>
        /// Checks if two images are identical.
        /// </summary>
        public static bool AreIdentical(SKBitmap image1, SKBitmap image2)
        {
            return Compare(image1, image2) >= 1.0;
        }

        /// <summary>
        /// Checks if two images are similar within a given threshold.
        /// </summary>
        /// <param name="image1">First image</param>
        /// <param name="image2">Second image</param>
        /// <param name="threshold">Minimum similarity ratio (0.0-1.0, default 0.95)</param>
        /// <param name="colorTolerance">Maximum allowed per-channel color difference (0-255)</param>
        /// <returns>true if images are similar within the threshold</returns>
        public static bool AreSimilar(SKBitmap image1, SKBitmap image2, double threshold = 0.95, int colorTolerance = 5)
        {
            return CompareWithTolerance(image1, image2, colorTolerance) >= threshold;
        }
    }
}

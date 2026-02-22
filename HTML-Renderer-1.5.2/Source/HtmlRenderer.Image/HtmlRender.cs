using System;
using System.IO;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.Image.Adapters;

namespace TheArtOfDev.HtmlRenderer.Image
{
    /// <summary>
    /// Standalone static class for simple and direct HTML rendering to images.<br/>
    /// Supports rendering HTML to PNG, JPEG, and other image formats using SkiaSharp.<br/>
    /// Cross-platform support (Windows, Linux, macOS).
    /// </summary>
    public static class HtmlRender
    {
        /// <summary>
        /// Renders the specified HTML into a new image of the requested size.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="width">The width of the image to render into</param>
        /// <param name="height">The height of the image to render into</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated bitmap of the html</returns>
        public static SKBitmap RenderToImage(string html, int width, int height,
            SKColor backgroundColor = default,
            CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null,
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);

            var bgColor = backgroundColor == default ? SKColors.White : backgroundColor;
            canvas.Clear(bgColor);

            if (!string.IsNullOrEmpty(html))
            {
                RenderHtml(canvas, html, new RPoint(0, 0), new RSize(width, height), cssData, stylesheetLoad, imageLoad);
            }

            return bitmap;
        }

        /// <summary>
        /// Renders the specified HTML into a new image with auto-sized dimensions.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="maxWidth">optional: the max width of the rendered html (0 for unlimited)</param>
        /// <param name="maxHeight">optional: the max height of the rendered html (0 for unlimited)</param>
        /// <param name="backgroundColor">optional: the color to fill the image with (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>the generated bitmap of the html</returns>
        public static SKBitmap RenderToImageAutoSized(string html, int maxWidth = 0, int maxHeight = 0,
            SKColor backgroundColor = default,
            CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null,
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            if (string.IsNullOrEmpty(html))
                return new SKBitmap(1, 1);

            var bgColor = backgroundColor == default ? SKColors.White : backgroundColor;

            using var container = new HtmlContainer();
            container.AvoidAsyncImagesLoading = true;
            container.AvoidImagesLateLoading = true;

            if (stylesheetLoad != null)
                container.StylesheetLoad += stylesheetLoad;
            if (imageLoad != null)
                container.ImageLoad += imageLoad;

            container.SetHtml(html, cssData);

            var minSize = new RSize(0, 0);
            var maxSize = new RSize(maxWidth, maxHeight);
            var finalSize = MeasureHtml(container, minSize, maxSize);

            // Ensure minimum dimensions
            int w = Math.Max(1, (int)Math.Ceiling(finalSize.Width));
            int h = Math.Max(1, (int)Math.Ceiling(finalSize.Height));

            // Apply max width limit
            if (maxWidth < 1 && w > 4096)
                w = 4096;

            container.MaxSize = new RSize(w, h);

            var bitmap = new SKBitmap(w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(bgColor);

            var clip = new RRect(0, 0, w, h);
            container.PerformPaint(canvas, clip);

            return bitmap;
        }

        /// <summary>
        /// Renders the specified HTML to a PNG byte array.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="backgroundColor">optional: the background color (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>PNG image bytes</returns>
        public static byte[] RenderToPng(string html, int width, int height,
            SKColor backgroundColor = default,
            CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null,
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            using var bitmap = RenderToImage(html, width, height, backgroundColor, cssData, stylesheetLoad, imageLoad);
            using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        /// <summary>
        /// Renders the specified HTML to a JPEG byte array.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="quality">JPEG quality (1-100, default 90)</param>
        /// <param name="backgroundColor">optional: the background color (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        /// <returns>JPEG image bytes</returns>
        public static byte[] RenderToJpeg(string html, int width, int height,
            int quality = 90,
            SKColor backgroundColor = default,
            CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null,
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            using var bitmap = RenderToImage(html, width, height, backgroundColor, cssData, stylesheetLoad, imageLoad);
            using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, quality);
            return data.ToArray();
        }

        /// <summary>
        /// Renders the specified HTML to a file in the specified format.
        /// </summary>
        /// <param name="html">HTML source to render</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="filePath">Path to save the image to</param>
        /// <param name="format">Image format (default PNG)</param>
        /// <param name="quality">Image quality for lossy formats (1-100, default 90)</param>
        /// <param name="backgroundColor">optional: the background color (default - white)</param>
        /// <param name="cssData">optional: the style to use for html rendering</param>
        /// <param name="stylesheetLoad">optional: can be used to overwrite stylesheet resolution logic</param>
        /// <param name="imageLoad">optional: can be used to overwrite image resolution logic</param>
        public static void RenderToFile(string html, int width, int height, string filePath,
            SKEncodedImageFormat format = SKEncodedImageFormat.Png,
            int quality = 90,
            SKColor backgroundColor = default,
            CssData cssData = null,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad = null,
            EventHandler<HtmlImageLoadEventArgs> imageLoad = null)
        {
            ArgChecker.AssertArgNotNullOrEmpty(filePath, nameof(filePath));

            using var bitmap = RenderToImage(html, width, height, backgroundColor, cssData, stylesheetLoad, imageLoad);
            using var data = bitmap.Encode(format, quality);
            using var stream = File.OpenWrite(filePath);
            data.SaveTo(stream);
        }

        /// <summary>
        /// Measure html layout using a temporary bitmap surface.
        /// </summary>
        private static RSize MeasureHtml(HtmlContainer container, RSize minSize, RSize maxSize)
        {
            // Create a small temporary surface for measurement
            using var measureBitmap = new SKBitmap(1, 1);
            using var measureCanvas = new SKCanvas(measureBitmap);
            var clip = new RRect(0, 0, 99999, 99999);

            using var g = new GraphicsAdapter(measureCanvas, clip);
            return HtmlRendererUtils.MeasureHtmlByRestrictions(g, container.HtmlContainerInt, minSize, maxSize);
        }

        /// <summary>
        /// Render HTML onto the given canvas.
        /// </summary>
        private static RSize RenderHtml(SKCanvas canvas, string html, RPoint location, RSize maxSize,
            CssData cssData,
            EventHandler<HtmlStylesheetLoadEventArgs> stylesheetLoad,
            EventHandler<HtmlImageLoadEventArgs> imageLoad)
        {
            RSize actualSize = new RSize(0, 0);

            if (!string.IsNullOrEmpty(html))
            {
                using var container = new HtmlContainer();
                container.Location = location;
                container.MaxSize = maxSize;
                container.AvoidAsyncImagesLoading = true;
                container.AvoidImagesLateLoading = true;

                if (stylesheetLoad != null)
                    container.StylesheetLoad += stylesheetLoad;
                if (imageLoad != null)
                    container.ImageLoad += imageLoad;

                container.SetHtml(html, cssData);

                var clip = new RRect(location.X, location.Y, maxSize.Width, maxSize.Height);
                container.PerformLayout(canvas, clip);
                container.PerformPaint(canvas, clip);

                actualSize = container.ActualSize;
            }

            return actualSize;
        }
    }
}

using System;
using System.Collections.Generic;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Image.Adapters;
using TheArtOfDev.HtmlRenderer.Image.Utilities;

namespace TheArtOfDev.HtmlRenderer.Image
{
    /// <summary>
    /// Low level handling of Html Renderer logic for image rendering.
    /// </summary>
    /// <seealso cref="HtmlContainerInt"/>
    public sealed class HtmlContainer : IDisposable
    {
        private readonly HtmlContainerInt _htmlContainerInt;

        /// <summary>
        /// Init.
        /// </summary>
        public HtmlContainer()
        {
            _htmlContainerInt = new HtmlContainerInt(SkiaImageAdapter.Instance);
            _htmlContainerInt.SetMargins(0);
            _htmlContainerInt.PageSize = new RSize(99999, 99999);
        }

        /// <summary>
        /// Raised when the set html document has been fully loaded.
        /// </summary>
        public event EventHandler LoadComplete
        {
            add { _htmlContainerInt.LoadComplete += value; }
            remove { _htmlContainerInt.LoadComplete -= value; }
        }

        /// <summary>
        /// Raised when an error occurred during html rendering.
        /// </summary>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError
        {
            add { _htmlContainerInt.RenderError += value; }
            remove { _htmlContainerInt.RenderError -= value; }
        }

        /// <summary>
        /// Raised when a stylesheet is about to be loaded.
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad
        {
            add { _htmlContainerInt.StylesheetLoad += value; }
            remove { _htmlContainerInt.StylesheetLoad -= value; }
        }

        /// <summary>
        /// Raised when an image is about to be loaded.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad
        {
            add { _htmlContainerInt.ImageLoad += value; }
            remove { _htmlContainerInt.ImageLoad -= value; }
        }

        /// <summary>
        /// The internal core html container.
        /// </summary>
        internal HtmlContainerInt HtmlContainerInt => _htmlContainerInt;

        /// <summary>
        /// the parsed stylesheet data used for handling the html.
        /// </summary>
        public CssData CssData => _htmlContainerInt.CssData;

        /// <summary>
        /// Gets or sets a value indicating if image asynchronous loading should be avoided.
        /// </summary>
        public bool AvoidAsyncImagesLoading
        {
            get => _htmlContainerInt.AvoidAsyncImagesLoading;
            set => _htmlContainerInt.AvoidAsyncImagesLoading = value;
        }

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided.
        /// </summary>
        public bool AvoidImagesLateLoading
        {
            get => _htmlContainerInt.AvoidImagesLateLoading;
            set => _htmlContainerInt.AvoidImagesLateLoading = value;
        }

        /// <summary>
        /// The max width and height of the rendered html.
        /// </summary>
        public RSize MaxSize
        {
            get => _htmlContainerInt.MaxSize;
            set => _htmlContainerInt.MaxSize = value;
        }

        /// <summary>
        /// The actual size of the rendered html (after layout).
        /// </summary>
        public RSize ActualSize
        {
            get => _htmlContainerInt.ActualSize;
            internal set => _htmlContainerInt.ActualSize = value;
        }

        /// <summary>
        /// The top-left most location of the rendered html.
        /// </summary>
        public RPoint Location
        {
            get => _htmlContainerInt.Location;
            set => _htmlContainerInt.Location = value;
        }

        /// <summary>
        /// Init with optional document and stylesheet.
        /// </summary>
        public void SetHtml(string htmlSource, CssData baseCssData = null)
        {
            _htmlContainerInt.SetHtml(htmlSource, baseCssData);
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.
        /// </summary>
        public void PerformLayout(SKCanvas canvas, RRect clip)
        {
            using var g = new GraphicsAdapter(canvas, clip);
            _htmlContainerInt.PerformLayout(g);
        }

        /// <summary>
        /// Render the html using the given device.
        /// </summary>
        public void PerformPaint(SKCanvas canvas, RRect clip)
        {
            using var g = new GraphicsAdapter(canvas, clip);
            _htmlContainerInt.PerformPaint(g);
        }

        public void Dispose()
        {
            _htmlContainerInt.Dispose();
        }
    }
}

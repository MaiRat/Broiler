using System;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Image.Adapters;

namespace TheArtOfDev.HtmlRenderer.Image;

public sealed class HtmlContainer : IDisposable
{
    public HtmlContainer()
    {
        HtmlContainerInt = new HtmlContainerInt(SkiaImageAdapter.Instance);
        HtmlContainerInt.SetMargins(0);
        HtmlContainerInt.PageSize = new RSize(99999, 99999);
    }

    public event EventHandler LoadComplete
    {
        add { HtmlContainerInt.LoadComplete += value; }
        remove { HtmlContainerInt.LoadComplete -= value; }
    }

    public event EventHandler<HtmlRenderErrorEventArgs> RenderError
    {
        add { HtmlContainerInt.RenderError += value; }
        remove { HtmlContainerInt.RenderError -= value; }
    }

    public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad
    {
        add { HtmlContainerInt.StylesheetLoad += value; }
        remove { HtmlContainerInt.StylesheetLoad -= value; }
    }

    public event EventHandler<HtmlImageLoadEventArgs> ImageLoad
    {
        add { HtmlContainerInt.ImageLoad += value; }
        remove { HtmlContainerInt.ImageLoad -= value; }
    }

    internal HtmlContainerInt HtmlContainerInt { get; }

    public CssData CssData => HtmlContainerInt.CssData;

    public bool AvoidAsyncImagesLoading
    {
        get => HtmlContainerInt.AvoidAsyncImagesLoading;
        set => HtmlContainerInt.AvoidAsyncImagesLoading = value;
    }

    public bool AvoidImagesLateLoading
    {
        get => HtmlContainerInt.AvoidImagesLateLoading;
        set => HtmlContainerInt.AvoidImagesLateLoading = value;
    }

    public RSize MaxSize
    {
        get => HtmlContainerInt.MaxSize;
        set => HtmlContainerInt.MaxSize = value;
    }

    public RSize ActualSize
    {
        get => HtmlContainerInt.ActualSize;
        internal set => HtmlContainerInt.ActualSize = value;
    }

    public RPoint Location
    {
        get => HtmlContainerInt.Location;
        set => HtmlContainerInt.Location = value;
    }

    public void SetHtml(string htmlSource, CssData baseCssData = null) => HtmlContainerInt.SetHtml(htmlSource, baseCssData);

    public void PerformLayout(SKCanvas canvas, RRect clip)
    {
        using var g = new GraphicsAdapter(canvas, clip);
        HtmlContainerInt.PerformLayout(g);
    }

    public void PerformPaint(SKCanvas canvas, RRect clip)
    {
        using var g = new GraphicsAdapter(canvas, clip);
        HtmlContainerInt.PerformPaint(g);
    }

    public void Dispose() => HtmlContainerInt.Dispose();
}

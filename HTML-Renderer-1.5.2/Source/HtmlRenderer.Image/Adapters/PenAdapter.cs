using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Image.Adapters;

internal sealed class PenAdapter(SKPaint paint) : RPen
{
    public SKPaint Paint { get; } = paint;

    public override double Width
    {
        get => Paint.StrokeWidth;
        set => Paint.StrokeWidth = (float)value;
    }

    public override RDashStyle DashStyle
    {
        set
        {
            Paint.PathEffect = value switch
            {
                RDashStyle.Solid => null,
                RDashStyle.Dash => Width < 2
                                        ? SKPathEffect.CreateDash([4f, 4f], 0)
                                        : SKPathEffect.CreateDash([4f * (float)Width, 2f * (float)Width], 0),
                RDashStyle.Dot => SKPathEffect.CreateDash([(float)Width, (float)Width], 0),
                RDashStyle.DashDot => SKPathEffect.CreateDash([4f * (float)Width, 2f * (float)Width, (float)Width, 2f * (float)Width], 0),
                RDashStyle.DashDotDot => SKPathEffect.CreateDash([4f * (float)Width, 2f * (float)Width, (float)Width, 2f * (float)Width, (float)Width, 2f * (float)Width], 0),
                _ => null,
            };
        }
    }
}

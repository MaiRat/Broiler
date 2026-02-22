using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Image.Adapters
{
    internal sealed class PenAdapter : RPen
    {
        private readonly SKPaint _paint;

        public PenAdapter(SKPaint paint)
        {
            _paint = paint;
        }

        public SKPaint Paint => _paint;

        public override double Width
        {
            get => _paint.StrokeWidth;
            set => _paint.StrokeWidth = (float)value;
        }

        public override RDashStyle DashStyle
        {
            set
            {
                switch (value)
                {
                    case RDashStyle.Solid:
                        _paint.PathEffect = null;
                        break;
                    case RDashStyle.Dash:
                        _paint.PathEffect = Width < 2
                            ? SKPathEffect.CreateDash(new[] { 4f, 4f }, 0)
                            : SKPathEffect.CreateDash(new[] { 4f * (float)Width, 2f * (float)Width }, 0);
                        break;
                    case RDashStyle.Dot:
                        _paint.PathEffect = SKPathEffect.CreateDash(new[] { (float)Width, (float)Width }, 0);
                        break;
                    case RDashStyle.DashDot:
                        _paint.PathEffect = SKPathEffect.CreateDash(new[] { 4f * (float)Width, 2f * (float)Width, (float)Width, 2f * (float)Width }, 0);
                        break;
                    case RDashStyle.DashDotDot:
                        _paint.PathEffect = SKPathEffect.CreateDash(new[] { 4f * (float)Width, 2f * (float)Width, (float)Width, 2f * (float)Width, (float)Width, 2f * (float)Width }, 0);
                        break;
                    default:
                        _paint.PathEffect = null;
                        break;
                }
            }
        }
    }
}

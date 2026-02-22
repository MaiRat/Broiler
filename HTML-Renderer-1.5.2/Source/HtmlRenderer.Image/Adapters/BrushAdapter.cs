using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Image.Adapters
{
    internal sealed class BrushAdapter : RBrush
    {
        private readonly SKPaint _paint;
        private readonly bool _dispose;

        public BrushAdapter(SKPaint paint, bool dispose)
        {
            _paint = paint;
            _dispose = dispose;
        }

        public SKPaint Paint => _paint;

        public override void Dispose()
        {
            if (_dispose)
            {
                _paint.Dispose();
            }
        }
    }
}

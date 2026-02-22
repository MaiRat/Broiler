using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Image.Adapters
{
    internal sealed class FontAdapter : RFont
    {
        private readonly SKFont _font;
        private readonly SKTypeface _typeface;
        private readonly double _size;
        private readonly RFontStyle _style;
        private double _height = -1;
        private double _underlineOffset = -1;
        private double _whitespaceWidth = -1;

        public FontAdapter(SKTypeface typeface, double size, RFontStyle style)
        {
            _typeface = typeface;
            _size = size;
            _style = style;
            _font = new SKFont(typeface, (float)size);
            _font.Edging = SKFontEdging.SubpixelAntialias;

            // Calculate metrics
            var metrics = _font.Metrics;
            _height = metrics.Descent - metrics.Ascent;
            _underlineOffset = -metrics.Ascent + metrics.UnderlinePosition.GetValueOrDefault(metrics.Descent - metrics.Ascent * 0.87f);
        }

        public SKFont Font => _font;
        public SKTypeface Typeface => _typeface;

        public override double Size => _size;
        public override double Height => _height;
        public override double UnderlineOffset => _underlineOffset;
        public override double LeftPadding => _height / 6.0;

        public override double GetWhitespaceWidth(RGraphics graphics)
        {
            if (_whitespaceWidth < 0)
            {
                _whitespaceWidth = graphics.MeasureString(" ", this).Width;
            }
            return _whitespaceWidth;
        }

        internal void SetMetrics(double height, double underlineOffset)
        {
            _height = height;
            _underlineOffset = underlineOffset;
        }
    }
}

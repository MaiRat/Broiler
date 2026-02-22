using System;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Image.Adapters
{
    internal sealed class GraphicsPathAdapter : RGraphicsPath
    {
        private readonly SKPath _path = new SKPath();
        private RPoint _lastPoint;

        public SKPath Path => _path;

        public override void Start(double x, double y)
        {
            _lastPoint = new RPoint(x, y);
            _path.MoveTo((float)x, (float)y);
        }

        public override void LineTo(double x, double y)
        {
            _path.LineTo((float)x, (float)y);
            _lastPoint = new RPoint(x, y);
        }

        public override void ArcTo(double x, double y, double size, Corner corner)
        {
            float left = (float)(Math.Min(x, _lastPoint.X) - (corner == Corner.TopRight || corner == Corner.BottomRight ? size : 0));
            float top = (float)(Math.Min(y, _lastPoint.Y) - (corner == Corner.BottomLeft || corner == Corner.BottomRight ? size : 0));
            var rect = SKRect.Create(left, top, (float)size * 2, (float)size * 2);
            _path.ArcTo(rect, GetStartAngle(corner), 90, false);
            _lastPoint = new RPoint(x, y);
        }

        public override void Dispose()
        {
            _path.Dispose();
        }

        private static float GetStartAngle(Corner corner)
        {
            switch (corner)
            {
                case Corner.TopLeft: return 180;
                case Corner.TopRight: return 270;
                case Corner.BottomLeft: return 90;
                case Corner.BottomRight: return 0;
                default: throw new ArgumentOutOfRangeException(nameof(corner));
            }
        }
    }
}

using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Image.Adapters
{
    internal sealed class ImageAdapter : RImage
    {
        private readonly SKBitmap _bitmap;

        public ImageAdapter(SKBitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public SKBitmap Bitmap => _bitmap;

        public override double Width => _bitmap.Width;
        public override double Height => _bitmap.Height;

        public override void Dispose()
        {
            _bitmap.Dispose();
        }
    }
}

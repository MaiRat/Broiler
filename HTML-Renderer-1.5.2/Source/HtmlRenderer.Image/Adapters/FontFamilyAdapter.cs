using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.Image.Adapters
{
    internal sealed class FontFamilyAdapter : RFontFamily
    {
        private readonly string _familyName;

        public FontFamilyAdapter(string familyName)
        {
            _familyName = familyName;
        }

        public override string Name => _familyName;
    }
}

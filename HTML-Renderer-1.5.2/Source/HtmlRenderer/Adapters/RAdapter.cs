using System;
using System.Collections.Generic;
using System.IO;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Adapters;

public abstract class RAdapter
{
    private readonly Dictionary<RColor, RBrush> _brushesCache = [];
    private readonly Dictionary<RColor, RPen> _penCache = [];
    private readonly FontsHandler _fontsHandler;

    private CssData _defaultCssData;
    private RImage _loadImage;
    private RImage _errorImage;

    protected RAdapter() => _fontsHandler = new FontsHandler(this);

    public CssData DefaultCssData => _defaultCssData ??= CssData.Parse(this, CssDefaults.DefaultStyleSheet, false);

    public RColor GetColor(string colorName)
    {
        ArgChecker.AssertArgNotNullOrEmpty(colorName, "colorName");
        return GetColorInt(colorName);
    }

    public RPen GetPen(RColor color)
    {
        if (!_penCache.TryGetValue(color, out RPen pen))
            _penCache[color] = pen = CreatePen(color);

        return pen;
    }

    public RBrush GetSolidBrush(RColor color)
    {
        if (!_brushesCache.TryGetValue(color, out RBrush brush))
            _brushesCache[color] = brush = CreateSolidBrush(color);

        return brush;
    }

    public RBrush GetLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle) => CreateLinearGradientBrush(rect, color1, color2, angle);

    public RImage ConvertImage(object image) =>
        // TODO:a remove this by creating better API.
        ConvertImageInt(image);

    public RImage ImageFromStream(Stream memoryStream) => ImageFromStreamInt(memoryStream);

    public bool IsFontExists(string font) => _fontsHandler.IsFontExists(font);

    public void AddFontFamily(RFontFamily fontFamily) => _fontsHandler.AddFontFamily(fontFamily);

    public void AddFontFamilyMapping(string fromFamily, string toFamily) => _fontsHandler.AddFontFamilyMapping(fromFamily, toFamily);

    public RFont GetFont(string family, double size, RFontStyle style) => _fontsHandler.GetCachedFont(family, size, style);

    public RImage GetLoadingImage()
    {
        if (_loadImage == null)
        {
            var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Core.Utils.ImageLoad.png");

            if (stream != null)
                _loadImage = ImageFromStream(stream);
        }

        return _loadImage;
    }

    public RImage GetLoadingFailedImage()
    {
        if (_errorImage == null)
        {
            var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("TheArtOfDev.HtmlRenderer.Core.Utils.ImageError.png");

            if (stream != null)
                _errorImage = ImageFromStream(stream);
        }

        return _errorImage;
    }

    public object GetClipboardDataObject(string html, string plainText) => GetClipboardDataObjectInt(html, plainText);

    public void SetToClipboard(string text) => SetToClipboardInt(text);

    public void SetToClipboard(string html, string plainText) => SetToClipboardInt(html, plainText);

    public void SetToClipboard(RImage image) => SetToClipboardInt(image);

    public RContextMenu GetContextMenu() => CreateContextMenuInt();

    public void SaveToFile(RImage image, string name, string extension, RControl control = null) => SaveToFileInt(image, name, extension, control);

    internal RFont CreateFont(string family, double size, RFontStyle style) => CreateFontInt(family, size, style);

    internal RFont CreateFont(RFontFamily family, double size, RFontStyle style) => CreateFontInt(family, size, style);

    protected abstract RColor GetColorInt(string colorName);

    protected abstract RPen CreatePen(RColor color);

    protected abstract RBrush CreateSolidBrush(RColor color);

    protected abstract RBrush CreateLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle);

    protected abstract RImage ConvertImageInt(object image);

    protected abstract RImage ImageFromStreamInt(Stream memoryStream);

    protected abstract RFont CreateFontInt(string family, double size, RFontStyle style);

    protected abstract RFont CreateFontInt(RFontFamily family, double size, RFontStyle style);

    protected virtual object GetClipboardDataObjectInt(string html, string plainText) => throw new NotImplementedException();

    protected virtual void SetToClipboardInt(string text) => throw new NotImplementedException();

    protected virtual void SetToClipboardInt(string html, string plainText) => throw new NotImplementedException();

    protected virtual void SetToClipboardInt(RImage image) => throw new NotImplementedException();

    protected virtual RContextMenu CreateContextMenuInt() => throw new NotImplementedException();

    protected virtual void SaveToFileInt(RImage image, string name, string extension, RControl control = null) => throw new NotImplementedException();
}
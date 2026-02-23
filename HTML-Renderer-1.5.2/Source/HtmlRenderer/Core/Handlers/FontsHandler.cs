using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers;

internal sealed class FontsHandler
{
    private readonly RAdapter _adapter;
    private readonly Dictionary<string, string> _fontsMapping = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, RFontFamily> _existingFontFamilies = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, Dictionary<double, Dictionary<RFontStyle, RFont>>> _fontsCache = new(StringComparer.InvariantCultureIgnoreCase);

    public FontsHandler(RAdapter adapter)
    {
        ArgChecker.AssertArgNotNull(adapter, "global");

        _adapter = adapter;
    }

    public bool IsFontExists(string family)
    {
        bool exists = _existingFontFamilies.ContainsKey(family);

        if (!exists)
        {
            if (_fontsMapping.TryGetValue(family, out string mappedFamily))
                exists = _existingFontFamilies.ContainsKey(mappedFamily);
        }

        return exists;
    }

    public void AddFontFamily(RFontFamily fontFamily)
    {
        ArgChecker.AssertArgNotNull(fontFamily, "family");

        _existingFontFamilies[fontFamily.Name] = fontFamily;
    }

    public void AddFontFamilyMapping(string fromFamily, string toFamily)
    {
        ArgChecker.AssertArgNotNullOrEmpty(fromFamily, "fromFamily");
        ArgChecker.AssertArgNotNullOrEmpty(toFamily, "toFamily");

        _fontsMapping[fromFamily] = toFamily;
    }

    public RFont GetCachedFont(string family, double size, RFontStyle style)
    {
        var font = TryGetFont(family, size, style);

        if (font != null)
            return font;

        if (!_existingFontFamilies.ContainsKey(family))
        {
            if (_fontsMapping.TryGetValue(family, out string mappedFamily))
            {
                font = TryGetFont(mappedFamily, size, style);
                if (font == null)
                {
                    font = CreateFont(mappedFamily, size, style);
                    _fontsCache[mappedFamily][size][style] = font;
                }
            }
        }

        font ??= CreateFont(family, size, style);
        _fontsCache[family][size][style] = font;

        return font;
    }

    private RFont TryGetFont(string family, double size, RFontStyle style)
    {
        RFont font = null;

        if (_fontsCache.TryGetValue(family, out Dictionary<double, Dictionary<RFontStyle, RFont>> a))
        {
            if (a.TryGetValue(size, out Dictionary<RFontStyle, RFont> b))
            {
                if (b.TryGetValue(style, out RFont value))
                    font = value;
            }
            else
            {
                _fontsCache[family][size] = [];
            }
        }
        else
        {
            _fontsCache[family] = new Dictionary<double, Dictionary<RFontStyle, RFont>> { [size] = [] };
        }

        return font;
    }

    private RFont CreateFont(string family, double size, RFontStyle style)
    {
        RFontFamily fontFamily;

        try
        {
            return _existingFontFamilies.TryGetValue(family, out fontFamily)
                ? _adapter.CreateFont(fontFamily, size, style)
                : _adapter.CreateFont(family, size, style);
        }
        catch (Exception ex)
        {
            // handle possibility of no requested style exists for the font, use regular then
            System.Diagnostics.Debug.WriteLine($"[HtmlRenderer] FontsHandler.GetCachedFont style fallback for '{family}': {ex.Message}");
            return _existingFontFamilies.TryGetValue(family, out fontFamily)
                ? _adapter.CreateFont(fontFamily, size, RFontStyle.Regular)
                : _adapter.CreateFont(family, size, RFontStyle.Regular);
        }
    }
}
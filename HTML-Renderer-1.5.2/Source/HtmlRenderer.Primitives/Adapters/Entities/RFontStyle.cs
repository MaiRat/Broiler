using System;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities;

[Flags]
public enum RFontStyle
{
    Regular = 0,
    Bold = 1,
    Italic = 2,
    Underline = 4,
    Strikeout = 8
}
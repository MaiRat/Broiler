using System;

namespace TheArtOfDev.HtmlRenderer.Core.Entities;

public readonly struct CssBlockSelectorItem
{
    public CssBlockSelectorItem(string @class, bool directParent)
    {
        ArgumentException.ThrowIfNullOrEmpty(@class);

        Class = @class;
        DirectParent = directParent;
    }

    public readonly string Class { get; }
    public readonly bool DirectParent { get; }
    public override readonly string ToString() => Class + (DirectParent ? " > " : string.Empty);
}
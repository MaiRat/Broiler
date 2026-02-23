using System;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Core.Entities;

public sealed class HtmlScrollEventArgs(RPoint location) : EventArgs
{
    public double X => location.X;
    public double Y => location.Y;

    public override string ToString() => $"Location: {location}";
}
using System.Collections.Generic;
using System.Drawing;

namespace TheArtOfDev.HtmlRenderer.Core.IR;

/// <summary>
/// Flat, ordered list of drawing primitives.
/// Produced by paint; consumed by raster. No DOM/style references.
/// </summary>
/// <remarks>
/// Phase 1: Type definitions only. Not yet populated by the rendering pipeline.
/// </remarks>
public sealed class DisplayList
{
    public IReadOnlyList<DisplayItem> Items { get; init; } = [];
}

/// <summary>
/// Base class for all display list drawing primitives.
/// </summary>
public abstract class DisplayItem
{
    public RectangleF Bounds { get; init; }
}

/// <summary>Fills a rectangle with a solid color.</summary>
public sealed class FillRectItem : DisplayItem
{
    public Color Color { get; init; }
}

/// <summary>Draws a border around a rectangle.</summary>
public sealed class DrawBorderItem : DisplayItem
{
    public BoxEdges Widths { get; init; } = BoxEdges.Zero;
    public Color TopColor { get; init; }
    public Color RightColor { get; init; }
    public Color BottomColor { get; init; }
    public Color LeftColor { get; init; }
    public string Style { get; init; } = "solid";
}

/// <summary>Draws a text string at a given origin.</summary>
public sealed class DrawTextItem : DisplayItem
{
    public string Text { get; init; } = string.Empty;
    public string FontFamily { get; init; } = string.Empty;
    public float FontSize { get; init; }
    public string FontWeight { get; init; } = "normal";
    public Color Color { get; init; }
    public PointF Origin { get; init; }
}

/// <summary>Draws an image into a destination rectangle.</summary>
public sealed class DrawImageItem : DisplayItem
{
    public object? ImageHandle { get; init; }
    public RectangleF SourceRect { get; init; }
    public RectangleF DestRect { get; init; }
}

/// <summary>Pushes a clip rectangle onto the clip stack.</summary>
public sealed class ClipItem : DisplayItem
{
    public RectangleF ClipRect { get; init; }
}

/// <summary>Pops the most recent clip from the clip stack.</summary>
public sealed class RestoreItem : DisplayItem { }

/// <summary>Applies an opacity value to subsequent items until restored.</summary>
public sealed class OpacityItem : DisplayItem
{
    public float Opacity { get; init; }
}

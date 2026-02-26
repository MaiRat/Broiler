using System.Collections.Generic;
using System.Drawing;

namespace TheArtOfDev.HtmlRenderer.Core.IR;

/// <summary>
/// Immutable layout result for a single box/fragment.
/// Produced by layout; consumed by paint.
/// </summary>
/// <remarks>
/// Phase 1: This is a read-only snapshot built after layout completes by walking the CssBox tree.
/// No existing code paths consume this yet.
/// </remarks>
public sealed class Fragment
{
    /// <summary>Location in absolute coordinates.</summary>
    public PointF Location { get; init; }

    /// <summary>Size of the fragment's border-box.</summary>
    public SizeF Size { get; init; }

    /// <summary>Bounding rectangle (Location + Size).</summary>
    public RectangleF Bounds => new(Location, Size);

    /// <summary>Resolved margin edges.</summary>
    public BoxEdges Margin { get; init; } = BoxEdges.Zero;

    /// <summary>Resolved border-width edges.</summary>
    public BoxEdges Border { get; init; } = BoxEdges.Zero;

    /// <summary>Resolved padding edges.</summary>
    public BoxEdges Padding { get; init; } = BoxEdges.Zero;

    /// <summary>Inline line fragments (for boxes that generate line boxes).</summary>
    public IReadOnlyList<LineFragment>? Lines { get; init; }

    /// <summary>Child fragments.</summary>
    public IReadOnlyList<Fragment> Children { get; init; } = [];

    /// <summary>Back-reference to the computed style (for paint to pick colors etc.).</summary>
    public ComputedStyle Style { get; init; } = new();

    /// <summary>Whether this fragment creates a new stacking context.</summary>
    public bool CreatesStackingContext { get; init; }

    /// <summary>Stack level (z-index or implicit order).</summary>
    public int StackLevel { get; init; }
}

/// <summary>
/// A single line-box within a block container.
/// </summary>
public sealed class LineFragment
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Width { get; init; }
    public float Height { get; init; }
    public float Baseline { get; init; }
    public IReadOnlyList<InlineFragment> Inlines { get; init; } = [];
}

/// <summary>
/// An inline-level fragment within a line box (text run or inline box).
/// </summary>
public sealed class InlineFragment
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Width { get; init; }
    public float Height { get; init; }
    public string? Text { get; init; }
    public ComputedStyle Style { get; init; } = new();

    /// <summary>Platform-specific font handle resolved during layout (Phase 3).</summary>
    public object? FontHandle { get; init; }
}

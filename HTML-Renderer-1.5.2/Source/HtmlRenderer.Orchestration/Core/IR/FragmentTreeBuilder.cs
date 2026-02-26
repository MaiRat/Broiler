using System.Collections.Generic;
using System.Drawing;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.IR;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core;

/// <summary>
/// Walks a <see cref="CssBox"/> tree after layout and builds a read-only
/// <see cref="Fragment"/> tree that snapshots the layout geometry.
/// </summary>
/// <remarks>
/// Phase 1: Shadow data — the fragment tree is built in parallel with existing code
/// but is not consumed by any rendering path yet.
/// </remarks>
internal static class FragmentTreeBuilder
{
    /// <summary>
    /// Builds a <see cref="Fragment"/> tree from the given root <see cref="CssBox"/>.
    /// Should be called after <c>PerformLayout</c> has completed.
    /// </summary>
    public static Fragment Build(CssBox root)
    {
        return BuildFragment(root);
    }

    private static Fragment BuildFragment(CssBox box)
    {
        var style = ComputedStyleBuilder.FromBox(box);

        var children = new List<Fragment>(box.Boxes.Count);
        foreach (var child in box.Boxes)
        {
            children.Add(BuildFragment(child));
        }

        List<LineFragment>? lines = null;
        if (box.LineBoxes.Count > 0)
        {
            lines = new List<LineFragment>(box.LineBoxes.Count);
            foreach (var lineBox in box.LineBoxes)
            {
                lines.Add(BuildLineFragment(lineBox));
            }
        }

        return new Fragment
        {
            Location = box.Location,
            Size = box.Size,
            Margin = style.Margin,
            Border = style.Border,
            Padding = style.Padding,
            Lines = lines,
            Children = children,
            Style = style,
            CreatesStackingContext = IsStackingContext(box),
            StackLevel = 0,
        };
    }

    private static LineFragment BuildLineFragment(CssLineBox lineBox)
    {
        var inlines = new List<InlineFragment>();

        foreach (var word in lineBox.Words)
        {
            var ownerStyle = ComputedStyleBuilder.FromBox(word.OwnerBox);
            inlines.Add(new InlineFragment
            {
                X = (float)word.Left,
                Y = (float)word.Top,
                Width = (float)word.Width,
                Height = (float)word.Height,
                Text = word.IsSpaces ? " " : word.Text,
                Style = ownerStyle,
            });
        }

        // Compute line bounds from all rectangles in this line box
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxR = float.MinValue, maxB = float.MinValue;

        foreach (var rect in lineBox.Rectangles.Values)
        {
            if (rect.X < minX) minX = rect.X;
            if (rect.Y < minY) minY = rect.Y;
            if (rect.Right > maxR) maxR = rect.Right;
            if (rect.Bottom > maxB) maxB = rect.Bottom;
        }

        if (lineBox.Rectangles.Count == 0)
        {
            minX = minY = maxR = maxB = 0;
        }

        return new LineFragment
        {
            X = minX,
            Y = minY,
            Width = maxR - minX,
            Height = maxB - minY,
            Baseline = 0,
            Inlines = inlines,
        };
    }

    private static bool IsStackingContext(CssBox box)
    {
        // A box creates a stacking context if it is positioned with a z-index,
        // or has opacity < 1, or is a fixed/absolute-positioned element.
        if (box.Position == CssConstants.Absolute || box.Position == CssConstants.Fixed)
            return true;

        if (double.TryParse(box.Opacity, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var opacity) && opacity < 1.0)
            return true;

        return false;
    }
}

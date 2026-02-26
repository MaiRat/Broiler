using System;
using System.Collections.Generic;
using System.Drawing;
using TheArtOfDev.HtmlRenderer.Core.IR;

namespace TheArtOfDev.HtmlRenderer.Core;

/// <summary>
/// Walks a <see cref="Fragment"/> tree and produces a flat <see cref="DisplayList"/>
/// of drawing primitives. This decouples paint from the DOM (<see cref="Dom.CssBox"/>).
/// </summary>
/// <remarks>
/// Phase 3: Standalone paint walker that reads only from <see cref="Fragment"/> and
/// <see cref="ComputedStyle"/>. Replaces <c>CssBox.PaintImp()</c> for the new rendering path.
/// </remarks>
internal static class PaintWalker
{
    /// <summary>Default selection highlight color (semi-transparent blue).</summary>
    private static readonly Color SelectionHighlightColor = Color.FromArgb(0x69, 0x33, 0x99, 0xFF);

    /// <summary>
    /// Sentinel value indicating that a selection offset is not constrained
    /// (i.e. the entire inline is selected on that side). Matches the convention
    /// used by <c>CssRect.SelectedStartOffset</c> / <c>SelectedEndOffset</c>.
    /// </summary>
    private const double FullSelectionOffset = -1;

    /// <summary>
    /// Paints the given <see cref="Fragment"/> tree and returns a flat <see cref="DisplayList"/>.
    /// </summary>
    public static DisplayList Paint(Fragment root)
    {
        var items = new List<DisplayItem>();
        PaintFragment(root, items);
        return new DisplayList { Items = items };
    }

    private static void PaintFragment(Fragment fragment, List<DisplayItem> items)
    {
        var style = fragment.Style;

        // Skip invisible fragments
        if (style.Display == "none")
            return;
        if (style.Visibility != "visible")
        {
            // Even if not visible, children may be visible (CSS spec)
            PaintChildren(fragment, items);
            return;
        }

        var bounds = fragment.Bounds;

        // Skip empty-cells table cells
        if (style.Display == "table-cell" && style.EmptyCells == "hide")
        {
            bool hasContent = fragment.Lines != null && fragment.Lines.Count > 0;
            if (!hasContent && fragment.Children.Count == 0)
                return;
        }

        // Overflow clipping
        bool clipped = false;
        if (style.Overflow == "hidden")
        {
            items.Add(new ClipItem { Bounds = bounds, ClipRect = bounds });
            clipped = true;
        }

        // Background color
        EmitBackground(fragment, items);

        // Background image
        EmitBackgroundImage(fragment, items);

        // Borders
        EmitBorders(fragment, items);

        // Replaced image (e.g. <img> elements)
        EmitReplacedImage(fragment, items);

        // Selection highlights (before text so highlight is behind text)
        EmitSelection(fragment, items);

        // Text (inline fragments from line boxes)
        EmitText(fragment, items);

        // Text decoration
        EmitTextDecoration(fragment, items);

        // Child fragments (stacking-context sorted)
        PaintChildren(fragment, items);

        // Restore clip
        if (clipped)
            items.Add(new RestoreItem { Bounds = bounds });
    }

    private static void EmitBackground(Fragment fragment, List<DisplayItem> items)
    {
        var style = fragment.Style;

        // Determine the set of rectangles to paint: per-line rects for inline elements,
        // or the single fragment bounds for block elements.
        var rects = GetPaintRects(fragment);

        foreach (var rect in rects)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                continue;

            // Background gradient
            if (style.ActualBackgroundGradient.A > 0 &&
                style.ActualBackgroundGradient != style.ActualBackgroundColor)
            {
                // Emit primary color; gradient rendering deferred to raster backend
                items.Add(new FillRectItem { Bounds = rect, Color = style.ActualBackgroundColor });
            }
            else if (style.ActualBackgroundColor.A > 0)
            {
                items.Add(new FillRectItem { Bounds = rect, Color = style.ActualBackgroundColor });
            }
        }
    }

    private static void EmitBackgroundImage(Fragment fragment, List<DisplayItem> items)
    {
        if (fragment.BackgroundImageHandle == null)
            return;

        var bounds = fragment.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        // Background image covers the padding box (inside borders)
        var border = fragment.Border;
        var imgRect = new RectangleF(
            bounds.X + (float)border.Left,
            bounds.Y + (float)border.Top,
            bounds.Width - (float)(border.Left + border.Right),
            bounds.Height - (float)(border.Top + border.Bottom));

        if (imgRect.Width > 0 && imgRect.Height > 0)
        {
            items.Add(new DrawImageItem
            {
                Bounds = imgRect,
                ImageHandle = fragment.BackgroundImageHandle,
                SourceRect = RectangleF.Empty,
                DestRect = imgRect,
            });
        }
    }

    private static void EmitReplacedImage(Fragment fragment, List<DisplayItem> items)
    {
        if (fragment.ImageHandle == null)
            return;

        var bounds = fragment.Bounds;
        var border = fragment.Border;
        var padding = fragment.Padding;

        // Image dest rect: inside border + padding (matching CssBoxImage.PaintImp)
        var r = new RectangleF(
            (float)Math.Floor(bounds.X + border.Left + padding.Left),
            (float)Math.Floor(bounds.Y + border.Top + padding.Top),
            bounds.Width - (float)(border.Left + border.Right + padding.Left + padding.Right),
            bounds.Height - (float)(border.Top + border.Bottom + padding.Top + padding.Bottom));

        if (r.Width > 0 && r.Height > 0)
        {
            items.Add(new DrawImageItem
            {
                Bounds = r,
                ImageHandle = fragment.ImageHandle,
                SourceRect = fragment.ImageSourceRect,
                DestRect = r,
            });
        }
    }

    private static void EmitSelection(Fragment fragment, List<DisplayItem> items)
    {
        if (fragment.Lines == null || fragment.Lines.Count == 0)
            return;

        foreach (var line in fragment.Lines)
        {
            foreach (var inline in line.Inlines)
            {
                if (!inline.Selected)
                    continue;

                // Selection highlight rectangle
                var left = inline.SelectedStartOffset > FullSelectionOffset ? (float)inline.SelectedStartOffset : 0f;
                var width = inline.SelectedEndOffset > FullSelectionOffset ? (float)inline.SelectedEndOffset - left : inline.Width - left;

                if (width <= 0)
                    continue;

                items.Add(new FillRectItem
                {
                    Bounds = new RectangleF(inline.X + left, inline.Y, width, line.Height),
                    Color = SelectionHighlightColor,
                });
            }
        }
    }

    private static void EmitBorders(Fragment fragment, List<DisplayItem> items)
    {
        var style = fragment.Style;
        var border = fragment.Border;

        bool hasTop = HasBorder(style.BorderTopStyle, border.Top);
        bool hasRight = HasBorder(style.BorderRightStyle, border.Right);
        bool hasBottom = HasBorder(style.BorderBottomStyle, border.Bottom);
        bool hasLeft = HasBorder(style.BorderLeftStyle, border.Left);

        if (!hasTop && !hasRight && !hasBottom && !hasLeft)
            return;

        var rects = GetPaintRects(fragment);

        for (int i = 0; i < rects.Count; i++)
        {
            var rect = rects[i];
            if (rect.Width <= 0 || rect.Height <= 0)
                continue;

            bool isFirst = i == 0;
            bool isLast = i == rects.Count - 1;

            items.Add(new DrawBorderItem
            {
                Bounds = rect,
                Widths = border,
                TopColor = hasTop ? style.ActualBorderTopColor : Color.Empty,
                RightColor = (hasRight && isLast) ? style.ActualBorderRightColor : Color.Empty,
                BottomColor = hasBottom ? style.ActualBorderBottomColor : Color.Empty,
                LeftColor = (hasLeft && isFirst) ? style.ActualBorderLeftColor : Color.Empty,
                // Style kept for Phase 1 backward compat; per-side styles are authoritative
                Style = style.BorderTopStyle ?? "solid",
                TopStyle = style.BorderTopStyle ?? "none",
                RightStyle = (isLast) ? (style.BorderRightStyle ?? "none") : "none",
                BottomStyle = style.BorderBottomStyle ?? "none",
                LeftStyle = (isFirst) ? (style.BorderLeftStyle ?? "none") : "none",
                CornerNw = style.ActualCornerNw,
                CornerNe = style.ActualCornerNe,
                CornerSe = style.ActualCornerSe,
                CornerSw = style.ActualCornerSw,
            });
        }
    }

    private static void EmitText(Fragment fragment, List<DisplayItem> items)
    {
        if (fragment.Lines == null || fragment.Lines.Count == 0)
            return;

        var style = fragment.Style;
        bool isRtl = style.Direction == "rtl";

        foreach (var line in fragment.Lines)
        {
            foreach (var inline in line.Inlines)
            {
                if (string.IsNullOrEmpty(inline.Text))
                    continue;

                // Skip line-break placeholders (CssRect uses "\n" for <br> elements)
                if (inline.Text == "\n")
                    continue;

                var inlineStyle = inline.Style;

                items.Add(new DrawTextItem
                {
                    Bounds = new RectangleF(inline.X, inline.Y, inline.Width, inline.Height),
                    Text = inline.Text,
                    FontFamily = inlineStyle.FontFamily,
                    FontSize = (float)ParseFontSize(inlineStyle.FontSize),
                    FontWeight = inlineStyle.FontWeight,
                    Color = inlineStyle.ActualColor,
                    Origin = new PointF(inline.X, inline.Y),
                    FontHandle = inline.FontHandle,
                    IsRtl = isRtl,
                });
            }
        }
    }

    private static void EmitTextDecoration(Fragment fragment, List<DisplayItem> items)
    {
        if (fragment.Lines == null || fragment.Lines.Count == 0)
            return;

        // Check text-decoration on the fragment itself and on its inline children.
        // In the box tree, text-decoration may be on the block or on anonymous inline children.
        string decoration = fragment.Style.TextDecoration;

        // If the block fragment doesn't have decoration, check children and inlines.
        // First child with a decoration wins (consistent with old CssBox.PaintDecoration
        // which only supported a single TextDecoration per box).
        if (string.IsNullOrEmpty(decoration) || decoration == "none")
        {
            // Check if any child fragment has text-decoration
            foreach (var child in fragment.Children)
            {
                if (!string.IsNullOrEmpty(child.Style.TextDecoration) && child.Style.TextDecoration != "none")
                {
                    decoration = child.Style.TextDecoration;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(decoration) || decoration == "none")
            return;

        var rects = GetPaintRects(fragment);

        foreach (var rect in rects)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                continue;

            var border = fragment.Border;
            var padding = fragment.Padding;

            float x1 = rect.X + (float)padding.Left + (float)border.Left;
            float x2 = rect.Right - (float)padding.Right - (float)border.Right;

            foreach (var line in fragment.Lines)
            {
                float y;
                if (decoration == "underline")
                    y = line.Y + line.Height * 0.85f; // approximate underline offset (~85% of line height)
                else if (decoration == "line-through")
                    y = line.Y + line.Height / 2f; // center of line
                else if (decoration == "overline")
                    y = line.Y; // top of line
                else
                    continue;

                items.Add(new DrawLineItem
                {
                    Bounds = new RectangleF(x1, y, x2 - x1, 1),
                    Start = new PointF(x1, y),
                    End = new PointF(x2, y),
                    Color = fragment.Style.ActualColor,
                    Width = 1,
                    DashStyle = "solid",
                });
            }
        }
    }

    private static void PaintChildren(Fragment fragment, List<DisplayItem> items)
    {
        if (fragment.Children.Count == 0)
            return;

        // Separate children into non-positioned and positioned
        // Paint order: non-positioned (tree order), then positioned (sorted by StackLevel)
        List<Fragment>? positioned = null;

        foreach (var child in fragment.Children)
        {
            if (child.CreatesStackingContext)
            {
                positioned ??= new List<Fragment>();
                positioned.Add(child);
            }
            else
            {
                PaintFragment(child, items);
            }
        }

        if (positioned != null)
        {
            positioned.Sort((a, b) => a.StackLevel.CompareTo(b.StackLevel));
            foreach (var child in positioned)
            {
                PaintFragment(child, items);
            }
        }
    }

    private static bool HasBorder(string? borderStyle, double width)
    {
        if (width <= 0)
            return false;
        if (string.IsNullOrEmpty(borderStyle))
            return false;
        if (borderStyle == "none" || borderStyle == "hidden")
            return false;
        return true;
    }

    private static double ParseFontSize(string fontSize)
    {
        if (string.IsNullOrEmpty(fontSize))
            return 11; // default: matches CssConstants.FontSize (11pt)

        // CSS 2.1 §15.7 named absolute sizes mapped to pt values
        // (relative to CssConstants.FontSize = 11)
        return fontSize switch
        {
            "medium" => 11,
            "xx-small" => 7,
            "x-small" => 8,
            "small" => 9,
            "large" => 13,
            "x-large" => 14,
            "xx-large" => 15,
            _ => TryParseNumeric(fontSize, 11),
        };
    }

    private static double TryParseNumeric(string value, double fallback)
    {
        // Strip common CSS units
        var numeric = value;
        if (numeric.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
            numeric = numeric[..^2];
        else if (numeric.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            numeric = numeric[..^2];
        else if (numeric.EndsWith("em", StringComparison.OrdinalIgnoreCase))
            numeric = numeric[..^2];

        return double.TryParse(numeric, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : fallback;
    }

    /// <summary>
    /// Returns the list of rectangles to paint for a fragment. For inline elements
    /// that have per-line-box rectangles, returns those; otherwise returns
    /// the single <see cref="Fragment.Bounds"/> rectangle.
    /// </summary>
    private static IReadOnlyList<RectangleF> GetPaintRects(Fragment fragment)
    {
        if (fragment.InlineRects != null && fragment.InlineRects.Count > 0)
            return fragment.InlineRects;
        return [fragment.Bounds];
    }
}

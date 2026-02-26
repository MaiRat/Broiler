using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.IR;

namespace TheArtOfDev.HtmlRenderer.Core;

/// <summary>
/// Creates a <see cref="ComputedStyle"/> snapshot from a <see cref="CssBoxProperties"/> instance.
/// This factory captures the current lazy-parsed computed values.
/// </summary>
/// <remarks>
/// Phase 1: Shadow data — existing code paths are unchanged.
/// </remarks>
internal static class ComputedStyleBuilder
{
    /// <summary>
    /// Snapshots the computed style of a CssBox, capturing all resolved actual values.
    /// </summary>
    public static ComputedStyle FromBox(CssBoxProperties box)
    {
        return new ComputedStyle
        {
            // Box model
            Display = box.Display,
            Position = box.Position,
            Float = box.Float,
            Clear = box.Clear,
            Overflow = box.Overflow,
            Visibility = box.Visibility,
            Direction = box.Direction,

            // Dimensions (raw)
            Width = box.Width,
            Height = box.Height,
            MaxWidth = box.MaxWidth,

            // Computed dimensions
            ActualWidth = box.ActualWidth,
            ActualHeight = box.ActualHeight,

            // Spacing
            Margin = new BoxEdges(
                box.ActualMarginTop,
                box.ActualMarginRight,
                box.ActualMarginBottom,
                box.ActualMarginLeft),
            Border = new BoxEdges(
                box.ActualBorderTopWidth,
                box.ActualBorderRightWidth,
                box.ActualBorderBottomWidth,
                box.ActualBorderLeftWidth),
            Padding = new BoxEdges(
                box.ActualPaddingTop,
                box.ActualPaddingRight,
                box.ActualPaddingBottom,
                box.ActualPaddingLeft),

            // Corner radii
            ActualCornerNw = box.ActualCornerNw,
            ActualCornerNe = box.ActualCornerNe,
            ActualCornerSe = box.ActualCornerSe,
            ActualCornerSw = box.ActualCornerSw,

            // Typography
            FontFamily = box.FontFamily ?? string.Empty,
            FontSize = box.FontSize ?? "medium",
            FontStyle = box.FontStyle,
            FontVariant = box.FontVariant,
            FontWeight = box.FontWeight,
            TextAlign = box.TextAlign,
            TextDecoration = box.TextDecoration,
            WhiteSpace = box.WhiteSpace,
            WordBreak = box.WordBreak,
            VerticalAlign = box.VerticalAlign,
            ActualLineHeight = box.ActualLineHeight,
            ActualTextIndent = box.ActualTextIndent,
            ActualWordSpacing = box.ActualWordSpacing,

            // Colors
            ActualColor = box.ActualColor,
            ActualBackgroundColor = box.ActualBackgroundColor,
            ActualBackgroundGradient = box.ActualBackgroundGradient,
            ActualBackgroundGradientAngle = box.ActualBackgroundGradientAngle,

            // Border colors
            ActualBorderTopColor = box.ActualBorderTopColor,
            ActualBorderRightColor = box.ActualBorderRightColor,
            ActualBorderBottomColor = box.ActualBorderBottomColor,
            ActualBorderLeftColor = box.ActualBorderLeftColor,

            // Border styles
            BorderTopStyle = box.BorderTopStyle,
            BorderRightStyle = box.BorderRightStyle,
            BorderBottomStyle = box.BorderBottomStyle,
            BorderLeftStyle = box.BorderLeftStyle,

            // Background
            BackgroundImage = box.BackgroundImage,
            BackgroundPosition = box.BackgroundPosition,
            BackgroundRepeat = box.BackgroundRepeat,
            BackgroundSize = box.BackgroundSize,

            // List
            ListStyleType = box.ListStyleType,
            ListStylePosition = box.ListStylePosition,
            ListStyleImage = box.ListStyleImage,
            ListStyle = box.ListStyle,

            // Opacity
            Opacity = box.Opacity,

            // Flex
            FlexDirection = box.FlexDirection,
            JustifyContent = box.JustifyContent,
            AlignItems = box.AlignItems,

            // Table
            BorderSpacing = box.BorderSpacing,
            BorderCollapse = box.BorderCollapse,
            EmptyCells = box.EmptyCells,
            ActualBorderSpacingHorizontal = box.ActualBorderSpacingHorizontal,
            ActualBorderSpacingVertical = box.ActualBorderSpacingVertical,

            // Box shadow
            BoxShadow = box.BoxShadow,

            // Positioning
            Left = box.Left,
            Top = box.Top,

            // Content
            Content = box.Content,

            // Page
            PageBreakInside = box.PageBreakInside,
        };
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Broiler.App.Rendering
{
    /// <summary>CSS display property values.</summary>
    public enum CssDisplay { Block, Inline, InlineBlock, None }

    /// <summary>CSS position property values.</summary>
    public enum CssPosition { Static, Relative, Absolute, Fixed }

    /// <summary>CSS float property values.</summary>
    public enum CssFloat { None, Left, Right }

    /// <summary>CSS clear property values.</summary>
    public enum CssClear { None, Left, Right, Both }

    /// <summary>A rectangle defined by position and size.</summary>
    public struct Rect
    {
        /// <summary>Horizontal position.</summary>
        public float X;
        /// <summary>Vertical position.</summary>
        public float Y;
        /// <summary>Width.</summary>
        public float Width;
        /// <summary>Height.</summary>
        public float Height;

        /// <summary>Initializes a new <see cref="Rect"/>.</summary>
        public Rect(float x, float y, float width, float height)
        { X = x; Y = y; Width = width; Height = height; }
    }

    /// <summary>Edge sizes for margin, border, or padding.</summary>
    public class BoxEdges
    {
        /// <summary>Top edge.</summary>
        public float Top;
        /// <summary>Right edge.</summary>
        public float Right;
        /// <summary>Bottom edge.</summary>
        public float Bottom;
        /// <summary>Left edge.</summary>
        public float Left;
    }

    /// <summary>Dimensions of a CSS box including content area and surrounding edges.</summary>
    public class BoxDimensions
    {
        /// <summary>Content box X position.</summary>
        public float X;
        /// <summary>Content box Y position.</summary>
        public float Y;
        /// <summary>Content box width.</summary>
        public float Width;
        /// <summary>Content box height.</summary>
        public float Height;
        /// <summary>Margin edges.</summary>
        public BoxEdges Margin = new BoxEdges();
        /// <summary>Border edges.</summary>
        public BoxEdges Border = new BoxEdges();
        /// <summary>Padding edges.</summary>
        public BoxEdges Padding = new BoxEdges();

        /// <summary>Returns the padding box rectangle.</summary>
        public Rect PaddingBox() => new Rect(
            X - Padding.Left, Y - Padding.Top,
            Width + Padding.Left + Padding.Right, Height + Padding.Top + Padding.Bottom);

        /// <summary>Returns the border box rectangle.</summary>
        public Rect BorderBox()
        {
            var p = PaddingBox();
            return new Rect(p.X - Border.Left, p.Y - Border.Top,
                p.Width + Border.Left + Border.Right, p.Height + Border.Top + Border.Bottom);
        }

        /// <summary>Returns the margin box rectangle.</summary>
        public Rect MarginBox()
        {
            var b = BorderBox();
            return new Rect(b.X - Margin.Left, b.Y - Margin.Top,
                b.Width + Margin.Left + Margin.Right, b.Height + Margin.Top + Margin.Bottom);
        }
    }

    /// <summary>A box in the layout tree with computed dimensions and CSS properties.</summary>
    public class LayoutBox
    {
        /// <summary>The DOM element this box represents.</summary>
        public DomElement Element;
        /// <summary>Computed box dimensions.</summary>
        public BoxDimensions Dimensions = new BoxDimensions();
        /// <summary>Resolved display value.</summary>
        public CssDisplay Display;
        /// <summary>Resolved position value.</summary>
        public CssPosition Position;
        /// <summary>Resolved float value.</summary>
        public CssFloat Float;
        /// <summary>Resolved clear value.</summary>
        public CssClear Clear;
        /// <summary>Child layout boxes.</summary>
        public List<LayoutBox> Children = new List<LayoutBox>();

        /// <summary>Initializes a new <see cref="LayoutBox"/> for the given element.</summary>
        public LayoutBox(DomElement element) { Element = element; }
    }

    /// <summary>
    /// Implements CSS Box Model Level 3 layout with block, inline, and inline-block
    /// formatting contexts, float/clear, and positioning.
    /// </summary>
    public class CssBoxModel
    {
        private const float DefaultFontSize = 16f;
        private static readonly HashSet<string> BlockTags = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        { "div", "p", "h1", "h2", "h3", "h4", "h5", "h6",
          "ul", "ol", "li", "section", "article", "header",
          "footer", "main", "nav", "aside", "blockquote", "form", "table" };

        /// <summary>Builds a layout tree from a DOM tree and performs layout.</summary>
        /// <param name="root">The root DOM element.</param>
        /// <param name="containerWidth">Available width for the root block.</param>
        /// <returns>The root <see cref="LayoutBox"/> with computed dimensions.</returns>
        public LayoutBox BuildLayoutTree(DomElement root, float containerWidth)
        {
            var box = CreateLayoutBox(root);
            box.Dimensions.Width = containerWidth;
            LayoutBlock(box, containerWidth);
            return box;
        }

        private LayoutBox CreateLayoutBox(DomElement element)
        {
            var box = new LayoutBox(element)
            {
                Display = ResolveDisplay(element),
                Position = ResolvePosition(element),
                Float = ResolveFloat(element),
                Clear = ResolveClear(element)
            };
            if (box.Display == CssDisplay.None) return box;

            foreach (var child in element.Children)
            {
                var childBox = CreateLayoutBox(child);
                if (childBox.Display != CssDisplay.None)
                    box.Children.Add(childBox);
            }
            return box;
        }

        private void LayoutBlock(LayoutBox box, float containerWidth)
        {
            ResolveEdges(box, containerWidth);
            box.Dimensions.Width = containerWidth
                - box.Dimensions.Margin.Left - box.Dimensions.Margin.Right
                - box.Dimensions.Border.Left - box.Dimensions.Border.Right
                - box.Dimensions.Padding.Left - box.Dimensions.Padding.Right;

            float contentWidth = box.Dimensions.Width;
            float cursorY = 0f;
            float lineX = 0f;
            float lineHeight = 0f;
            var floatLeftX = 0f;
            var floatRightX = contentWidth;

            foreach (var child in box.Children)
            {
                if (child.Position == CssPosition.Absolute || child.Position == CssPosition.Fixed)
                {
                    LayoutOutOfFlow(child, contentWidth);
                    continue;
                }

                if (child.Clear != CssClear.None)
                {
                    if (child.Clear == CssClear.Left || child.Clear == CssClear.Both)
                        floatLeftX = 0f;
                    if (child.Clear == CssClear.Right || child.Clear == CssClear.Both)
                        floatRightX = contentWidth;
                    // Clearing also moves cursor below any floated content.
                    cursorY += lineHeight;
                    lineHeight = 0f;
                    lineX = floatLeftX;
                }

                if (child.Float != CssFloat.None)
                {
                    LayoutBlock(child, contentWidth);
                    var mb = child.Dimensions.MarginBox();
                    if (child.Float == CssFloat.Left)
                    {
                        child.Dimensions.X = box.Dimensions.X + box.Dimensions.Padding.Left + floatLeftX;
                        child.Dimensions.Y = box.Dimensions.Y + box.Dimensions.Padding.Top + cursorY;
                        floatLeftX += mb.Width;
                    }
                    else
                    {
                        floatRightX -= mb.Width;
                        child.Dimensions.X = box.Dimensions.X + box.Dimensions.Padding.Left + floatRightX;
                        child.Dimensions.Y = box.Dimensions.Y + box.Dimensions.Padding.Top + cursorY;
                    }
                    if (mb.Height > lineHeight) lineHeight = mb.Height;
                    continue;
                }

                if (child.Display == CssDisplay.Block)
                {
                    // Flush any inline line before placing a block.
                    cursorY += lineHeight;
                    lineHeight = 0f;
                    lineX = floatLeftX;

                    LayoutBlock(child, contentWidth);
                    child.Dimensions.X = box.Dimensions.X + box.Dimensions.Padding.Left
                        + child.Dimensions.Margin.Left + child.Dimensions.Border.Left;
                    child.Dimensions.Y = box.Dimensions.Y + box.Dimensions.Padding.Top + cursorY
                        + child.Dimensions.Margin.Top + child.Dimensions.Border.Top;
                    cursorY += child.Dimensions.MarginBox().Height;
                }
                else // Inline or InlineBlock
                {
                    float availableWidth = floatRightX - floatLeftX;
                    if (child.Display == CssDisplay.InlineBlock)
                        LayoutBlock(child, availableWidth);
                    else
                        ResolveEdges(child, availableWidth);

                    var mb = child.Dimensions.MarginBox();
                    if (child.Display == CssDisplay.Inline && child.Element.IsTextNode)
                        mb = EstimateTextBox(child, availableWidth);

                    // Wrap to next line if needed.
                    if (lineX + mb.Width > availableWidth && lineX > floatLeftX)
                    {
                        cursorY += lineHeight;
                        lineHeight = 0f;
                        lineX = floatLeftX;
                    }

                    child.Dimensions.X = box.Dimensions.X + box.Dimensions.Padding.Left + lineX
                        + child.Dimensions.Margin.Left + child.Dimensions.Border.Left;
                    child.Dimensions.Y = box.Dimensions.Y + box.Dimensions.Padding.Top + cursorY
                        + child.Dimensions.Margin.Top + child.Dimensions.Border.Top;

                    lineX += mb.Width;
                    if (mb.Height > lineHeight) lineHeight = mb.Height;

                    // Lay out inline children recursively.
                    if (child.Display == CssDisplay.Inline)
                        LayoutInlineChildren(child, availableWidth);
                }
            }

            cursorY += lineHeight;
            box.Dimensions.Height = cursorY;
            ApplyPositionOffset(box);
        }

        private void LayoutInlineChildren(LayoutBox box, float containerWidth)
        {
            float x = 0f;
            foreach (var child in box.Children)
            {
                ResolveEdges(child, containerWidth);
                var mb = child.Dimensions.MarginBox();
                if (child.Element.IsTextNode)
                    mb = EstimateTextBox(child, containerWidth);

                child.Dimensions.X = box.Dimensions.X + box.Dimensions.Padding.Left + x
                    + child.Dimensions.Margin.Left + child.Dimensions.Border.Left
                    + child.Dimensions.Padding.Left;
                child.Dimensions.Y = box.Dimensions.Y + box.Dimensions.Padding.Top;
                x += mb.Width;
            }
            if (x > box.Dimensions.Width) box.Dimensions.Width = x;
        }

        private void LayoutOutOfFlow(LayoutBox box, float containerWidth) =>
            LayoutBlock(box, containerWidth);

        private static void ApplyPositionOffset(LayoutBox box)
        {
            if (box.Position != CssPosition.Static)
            {
                box.Dimensions.X += ParseCssValue(GetStyle(box.Element, "left"), 0f, 0f);
                box.Dimensions.Y += ParseCssValue(GetStyle(box.Element, "top"), 0f, 0f);
            }
        }

        private static Rect EstimateTextBox(LayoutBox box, float containerWidth)
        {
            string text = box.Element.TextContent ?? string.Empty;
            float w = Math.Min(text.Length * DefaultFontSize * 0.6f, containerWidth);
            box.Dimensions.Width = w;
            box.Dimensions.Height = DefaultFontSize;
            return new Rect(0, 0, w, DefaultFontSize);
        }

        private static void ResolveEdges(LayoutBox box, float containerWidth)
        {
            var d = box.Dimensions;
            var el = box.Element;
            d.Margin.Top = ParseCssValue(GetStyle(el, "margin-top"), containerWidth, 0f);
            d.Margin.Right = ParseCssValue(GetStyle(el, "margin-right"), containerWidth, 0f);
            d.Margin.Bottom = ParseCssValue(GetStyle(el, "margin-bottom"), containerWidth, 0f);
            d.Margin.Left = ParseCssValue(GetStyle(el, "margin-left"), containerWidth, 0f);
            d.Border.Top = ParseCssValue(GetStyle(el, "border-top-width"), containerWidth, 0f);
            d.Border.Right = ParseCssValue(GetStyle(el, "border-right-width"), containerWidth, 0f);
            d.Border.Bottom = ParseCssValue(GetStyle(el, "border-bottom-width"), containerWidth, 0f);
            d.Border.Left = ParseCssValue(GetStyle(el, "border-left-width"), containerWidth, 0f);
            d.Padding.Top = ParseCssValue(GetStyle(el, "padding-top"), containerWidth, 0f);
            d.Padding.Right = ParseCssValue(GetStyle(el, "padding-right"), containerWidth, 0f);
            d.Padding.Bottom = ParseCssValue(GetStyle(el, "padding-bottom"), containerWidth, 0f);
            d.Padding.Left = ParseCssValue(GetStyle(el, "padding-left"), containerWidth, 0f);
        }

        private CssDisplay ResolveDisplay(DomElement el)
        {
            string val = GetStyle(el, "display");
            if (val != null)
            {
                if (val.Equals("block", StringComparison.OrdinalIgnoreCase)) return CssDisplay.Block;
                if (val.Equals("inline-block", StringComparison.OrdinalIgnoreCase)) return CssDisplay.InlineBlock;
                if (val.Equals("none", StringComparison.OrdinalIgnoreCase)) return CssDisplay.None;
                if (val.Equals("inline", StringComparison.OrdinalIgnoreCase)) return CssDisplay.Inline;
            }
            if (el.IsTextNode) return CssDisplay.Inline;
            return BlockTags.Contains(el.TagName) ? CssDisplay.Block : CssDisplay.Inline;
        }

        private static CssPosition ResolvePosition(DomElement el)
        {
            string val = GetStyle(el, "position");
            if (val == null) return CssPosition.Static;
            if (val.Equals("relative", StringComparison.OrdinalIgnoreCase)) return CssPosition.Relative;
            if (val.Equals("absolute", StringComparison.OrdinalIgnoreCase)) return CssPosition.Absolute;
            if (val.Equals("fixed", StringComparison.OrdinalIgnoreCase)) return CssPosition.Fixed;
            return CssPosition.Static;
        }

        private static CssFloat ResolveFloat(DomElement el)
        {
            string val = GetStyle(el, "float");
            if (val == null) return CssFloat.None;
            if (val.Equals("left", StringComparison.OrdinalIgnoreCase)) return CssFloat.Left;
            if (val.Equals("right", StringComparison.OrdinalIgnoreCase)) return CssFloat.Right;
            return CssFloat.None;
        }

        private static CssClear ResolveClear(DomElement el)
        {
            string val = GetStyle(el, "clear");
            if (val == null) return CssClear.None;
            if (val.Equals("left", StringComparison.OrdinalIgnoreCase)) return CssClear.Left;
            if (val.Equals("right", StringComparison.OrdinalIgnoreCase)) return CssClear.Right;
            if (val.Equals("both", StringComparison.OrdinalIgnoreCase)) return CssClear.Both;
            return CssClear.None;
        }

        private static string GetStyle(DomElement el, string property)
        {
            el.Style.TryGetValue(property, out string value);
            return value;
        }

        /// <summary>
        /// Parses a CSS length value such as "10px", "50%", or "auto".
        /// Percentages are resolved against <paramref name="containerSize"/>.
        /// </summary>
        public static float ParseCssValue(string value, float containerSize, float defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Equals("auto", StringComparison.OrdinalIgnoreCase))
                return defaultValue;
            if (value.EndsWith("%", StringComparison.Ordinal))
                return float.TryParse(value.AsSpan(0, value.Length - 1),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float pct)
                    ? containerSize * pct / 100f : defaultValue;

            string numeric = value;
            foreach (var suffix in new[] { "px", "em", "rem", "pt" })
                if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                { numeric = value.Substring(0, value.Length - suffix.Length); break; }

            return float.TryParse(numeric, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
                ? result : defaultValue;
        }
    }
}

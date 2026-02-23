using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom;

internal sealed class CssBoxHr : CssBox
{
    public CssBoxHr(CssBox parent, HtmlTag tag) : base(parent, tag) => Display = CssConstants.Block;

    protected override void PerformLayoutImp(RGraphics g)
    {
        if (Display == CssConstants.None)
            return;

        RectanglesReset();

        var prevSibling = DomUtils.GetPreviousSibling(this);
        double left = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ActualMarginLeft + ContainingBlock.ActualBorderLeftWidth;
        double top = (prevSibling == null && ParentBox != null ? ParentBox.ClientTop : ParentBox == null ? Location.Y : 0) + MarginTopCollapse(prevSibling) + (prevSibling != null ? prevSibling.ActualBottom + prevSibling.ActualBorderBottomWidth : 0);
        Location = new RPoint(left, top);
        ActualBottom = top;

        //width at 100% (or auto)
        double minwidth = GetMinimumWidth();
        double width = ContainingBlock.Size.Width
                       - ContainingBlock.ActualPaddingLeft - ContainingBlock.ActualPaddingRight
                       - ContainingBlock.ActualBorderLeftWidth - ContainingBlock.ActualBorderRightWidth
                       - ActualMarginLeft - ActualMarginRight - ActualBorderLeftWidth - ActualBorderRightWidth;

        //Check width if not auto
        if (Width != CssConstants.Auto && !string.IsNullOrEmpty(Width))
            width = CssValueParser.ParseLength(Width, width, this);

        if (width < minwidth || width >= 9999)
            width = minwidth;

        double height = ActualHeight;

        if (height < 1)
            height = Size.Height + ActualBorderTopWidth + ActualBorderBottomWidth;

        if (height < 1)
            height = 2;

        if (height <= 2 && ActualBorderTopWidth < 1 && ActualBorderBottomWidth < 1)
        {
            BorderTopStyle = BorderBottomStyle = CssConstants.Solid;
            BorderTopWidth = "1px";
            BorderBottomWidth = "1px";
        }

        Size = new RSize(width, height);

        ActualBottom = Location.Y + ActualPaddingTop + ActualPaddingBottom + height;
    }

    protected override void PaintImp(RGraphics g)
    {
        var offset = (HtmlContainer != null && !IsFixed) ? HtmlContainer.ScrollOffset : RPoint.Empty;
        var rect = new RRect(Bounds.X + offset.X, Bounds.Y + offset.Y, Bounds.Width, Bounds.Height);

        if (rect.Height > 2 && RenderUtils.IsColorVisible(ActualBackgroundColor))
            g.DrawRectangle(g.GetSolidBrush(ActualBackgroundColor), rect.X, rect.Y, rect.Width, rect.Height);

        var b1 = g.GetSolidBrush(ActualBorderTopColor);
        BordersDrawHandler.DrawBorder(Border.Top, g, this, b1, rect);

        if (rect.Height <= 1)
            return;

        var b2 = g.GetSolidBrush(ActualBorderLeftColor);
        BordersDrawHandler.DrawBorder(Border.Left, g, this, b2, rect);

        var b3 = g.GetSolidBrush(ActualBorderRightColor);
        BordersDrawHandler.DrawBorder(Border.Right, g, this, b3, rect);

        var b4 = g.GetSolidBrush(ActualBorderBottomColor);
        BordersDrawHandler.DrawBorder(Border.Bottom, g, this, b4, rect);
    }
}
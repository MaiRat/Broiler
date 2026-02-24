using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace TheArtOfDev.HtmlRenderer.Core.Utils;

internal static class RenderUtils
{
    public static bool IsColorVisible(RColor color) => color.A > 0;

    public static bool ClipGraphicsByOverflow(RGraphics g, CssBox box)
    {
        var containingBlock = box.ContainingBlock;

        while (true)
        {
            if (containingBlock.Overflow == CssConstants.Hidden)
            {
                var prevClip = g.GetClip();
                var rect = box.ContainingBlock.ClientRectangle;
                rect.X -= 2; // TODO:a find better way to fix it
                rect.Width += 2;

                if (!box.IsFixed)
                    rect.Offset(box.ContainerInt.ScrollOffset);

                rect.Intersect(prevClip);
                g.PushClip(rect);
                return true;
            }
            else
            {
                var cBlock = containingBlock.ContainingBlock;
                if (cBlock == containingBlock)
                    return false;
                containingBlock = cBlock;
            }
        }
    }

    public static void DrawImageLoadingIcon(RGraphics g, IHtmlContainerInt htmlContainer, RRect r)
    {
        g.DrawRectangle(g.GetPen(RColor.LightGray), r.Left + 3, r.Top + 3, 13, 14);
        var image = htmlContainer.GetLoadingImage();
        g.DrawImage(image, new RRect(r.Left + 4, r.Top + 4, image.Width, image.Height));
    }

    public static void DrawImageErrorIcon(RGraphics g, IHtmlContainerInt htmlContainer, RRect r)
    {
        g.DrawRectangle(g.GetPen(RColor.LightGray), r.Left + 2, r.Top + 2, 15, 15);
        var image = htmlContainer.GetLoadingFailedImage();
        g.DrawImage(image, new RRect(r.Left + 3, r.Top + 3, image.Width, image.Height));
    }

    public static RGraphicsPath GetRoundRect(RGraphics g, RRect rect, double nwRadius, double neRadius, double seRadius, double swRadius)
    {
        var path = g.GetGraphicsPath();

        path.Start(rect.Left + nwRadius, rect.Top);

        path.LineTo(rect.Right - neRadius, rect.Y);

        if (neRadius > 0f)
            path.ArcTo(rect.Right, rect.Top + neRadius, neRadius, RGraphicsPath.Corner.TopRight);

        path.LineTo(rect.Right, rect.Bottom - seRadius);

        if (seRadius > 0f)
            path.ArcTo(rect.Right - seRadius, rect.Bottom, seRadius, RGraphicsPath.Corner.BottomRight);

        path.LineTo(rect.Left + swRadius, rect.Bottom);

        if (swRadius > 0f)
            path.ArcTo(rect.Left, rect.Bottom - swRadius, swRadius, RGraphicsPath.Corner.BottomLeft);

        path.LineTo(rect.Left, rect.Top + nwRadius);

        if (nwRadius > 0f)
            path.ArcTo(rect.Left + nwRadius, rect.Top, nwRadius, RGraphicsPath.Corner.TopLeft);

        return path;
    }
}
using System;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers;

internal sealed class BackgroundImageDrawHandler : IBackgroundImageDrawHandler
{
    public static readonly BackgroundImageDrawHandler Instance = new();

    public static void DrawBackgroundImage(RGraphics g, IBackgroundRenderData box, IImageLoadHandler imageLoadHandler, RRect rectangle)
    {
        // image size depends if specific rectangle given in image loader
        var imgSize = new RSize(imageLoadHandler.Rectangle == RRect.Empty ? imageLoadHandler.Image.Width : imageLoadHandler.Rectangle.Width,
            imageLoadHandler.Rectangle == RRect.Empty ? imageLoadHandler.Image.Height : imageLoadHandler.Rectangle.Height);

        var location = GetLocation(box.BackgroundPosition, rectangle, imgSize);
        var srcRect = imageLoadHandler.Rectangle == RRect.Empty
            ? new RRect(0, 0, imgSize.Width, imgSize.Height)
            : new RRect(imageLoadHandler.Rectangle.Left, imageLoadHandler.Rectangle.Top, imgSize.Width, imgSize.Height);

        var destRect = new RRect(location, imgSize);

        // need to clip so repeated image will be cut on rectangle
        var lRectangle = rectangle;
        lRectangle.Intersect(g.GetClip());
        g.PushClip(lRectangle);

        switch (box.BackgroundRepeat)
        {
            case "no-repeat":
                g.DrawImage(imageLoadHandler.Image, destRect, srcRect);
                break;
            case "repeat-x":
                DrawRepeatX(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                break;
            case "repeat-y":
                DrawRepeatY(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                break;
            default:
                DrawRepeat(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                break;
        }

        g.PopClip();
    }

    private static RPoint GetLocation(string backgroundPosition, RRect rectangle, RSize imgSize)
    {
        double left = rectangle.Left;
        if (backgroundPosition.IndexOf("left", StringComparison.OrdinalIgnoreCase) > -1)
        {
            left = rectangle.Left + .5f;
        }
        else if (backgroundPosition.IndexOf("right", StringComparison.OrdinalIgnoreCase) > -1)
        {
            left = rectangle.Right - imgSize.Width;
        }
        else if (backgroundPosition.IndexOf("0", StringComparison.OrdinalIgnoreCase) < 0)
        {
            left = rectangle.Left + (rectangle.Width - imgSize.Width) / 2 + .5f;
        }

        double top = rectangle.Top;
        if (backgroundPosition.IndexOf("top", StringComparison.OrdinalIgnoreCase) > -1)
        {
            top = rectangle.Top;
        }
        else if (backgroundPosition.IndexOf("bottom", StringComparison.OrdinalIgnoreCase) > -1)
        {
            top = rectangle.Bottom - imgSize.Height;
        }
        else if (backgroundPosition.IndexOf("0", StringComparison.OrdinalIgnoreCase) < 0)
        {
            top = rectangle.Top + (rectangle.Height - imgSize.Height) / 2 + .5f;
        }

        return new RPoint(left, top);
    }

    private static void DrawRepeatX(RGraphics g, IImageLoadHandler imageLoadHandler, RRect rectangle, RRect srcRect, RRect destRect, RSize imgSize)
    {
        while (destRect.X > rectangle.X)
            destRect.X -= imgSize.Width;

        using var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location);
        g.DrawRectangle(brush, rectangle.X, destRect.Y, rectangle.Width, srcRect.Height);
    }

    private static void DrawRepeatY(RGraphics g, IImageLoadHandler imageLoadHandler, RRect rectangle, RRect srcRect, RRect destRect, RSize imgSize)
    {
        while (destRect.Y > rectangle.Y)
            destRect.Y -= imgSize.Height;

        using var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location);
        g.DrawRectangle(brush, destRect.X, rectangle.Y, srcRect.Width, rectangle.Height);
    }

    private static void DrawRepeat(RGraphics g, IImageLoadHandler imageLoadHandler, RRect rectangle, RRect srcRect, RRect destRect, RSize imgSize)
    {
        while (destRect.X > rectangle.X)
            destRect.X -= imgSize.Width;

        while (destRect.Y > rectangle.Y)
            destRect.Y -= imgSize.Height;

        using var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location);
        g.DrawRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    void IBackgroundImageDrawHandler.DrawBackgroundImage(RGraphics g, IBackgroundRenderData box, IImageLoadHandler imageHandler, RRect rectangle)
        => DrawBackgroundImage(g, box, imageHandler, rectangle);
}
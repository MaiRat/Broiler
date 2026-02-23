using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Entities;

public delegate void HtmlImageLoadCallback(string path, Object image, RRect imageRectangle);

public sealed class HtmlImageLoadEventArgs : EventArgs
{
    private readonly HtmlImageLoadCallback _callback;

    internal HtmlImageLoadEventArgs(string src, Dictionary<string, string> attributes, HtmlImageLoadCallback callback)
    {
        Src = src;
        Attributes = attributes;
        _callback = callback;
    }

    public string Src { get; }
    public Dictionary<string, string> Attributes { get; }
    public bool Handled { get; set; }

    public void Callback()
    {
        Handled = true;
        _callback(null, null, new RRect());
    }

    public void Callback(string path)
    {
        ArgChecker.AssertArgNotNullOrEmpty(path, "path");

        Handled = true;
        _callback(path, null, RRect.Empty);
    }

    public void Callback(string path, double x, double y, double width, double height)
    {
        ArgChecker.AssertArgNotNullOrEmpty(path, "path");

        Handled = true;
        _callback(path, null, new RRect(x, y, width, height));
    }

    public void Callback(Object image)
    {
        ArgChecker.AssertArgNotNull(image, "image");

        Handled = true;
        _callback(null, image, RRect.Empty);
    }

    public void Callback(Object image, double x, double y, double width, double height)
    {
        ArgChecker.AssertArgNotNull(image, "image");

        Handled = true;
        _callback(null, image, new RRect(x, y, width, height));
    }
}
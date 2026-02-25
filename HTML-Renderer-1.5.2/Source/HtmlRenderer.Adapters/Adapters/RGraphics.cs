using System.Drawing;
using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Adapters;

public abstract class RGraphics : IDisposable
{
    protected readonly IResourceFactory _adapter;
    protected readonly Stack<RRect> _clipStack = new();
    private readonly Stack<RRect> _suspendedClips = new();

    protected RGraphics(IResourceFactory adapter, RRect initialClip)
    {
        ArgumentNullException.ThrowIfNull(adapter);

        _adapter = adapter;
        _clipStack.Push(initialClip);
    }

    public RPen GetPen(Color color) => _adapter.GetPen(color);
    public RBrush GetSolidBrush(Color color) => _adapter.GetSolidBrush(color);
    public RBrush GetLinearGradientBrush(RRect rect, Color color1, Color color2, double angle) => _adapter.GetLinearGradientBrush(rect, color1, color2, angle);
    public RRect GetClip() => _clipStack.Peek();
    public abstract void PopClip();
    public abstract void PushClip(RRect rect);
    public abstract void PushClipExclude(RRect rect);

    public void SuspendClipping()
    {
        while (_clipStack.Count > 1)
        {
            var clip = GetClip();
            _suspendedClips.Push(clip);
            PopClip();
        }
    }

    public void ResumeClipping()
    {
        while (_suspendedClips.Count > 0)
        {
            var clip = _suspendedClips.Pop();
            PushClip(clip);
        }
    }

    public abstract Object SetAntiAliasSmoothingMode();
    public abstract void ReturnPreviousSmoothingMode(Object prevMode);
    public abstract RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation);
    public abstract RGraphicsPath GetGraphicsPath();
    public abstract RSize MeasureString(string str, RFont font);
    public abstract void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth);
    public abstract void DrawString(String str, RFont font, Color color, RPoint point, RSize size, bool rtl);
    public abstract void DrawLine(RPen pen, double x1, double y1, double x2, double y2);
    public abstract void DrawRectangle(RPen pen, double x, double y, double width, double height);
    public abstract void DrawRectangle(RBrush brush, double x, double y, double width, double height);
    public abstract void DrawImage(RImage image, RRect destRect, RRect srcRect);
    public abstract void DrawImage(RImage image, RRect destRect);
    public abstract void DrawPath(RPen pen, RGraphicsPath path);
    public abstract void DrawPath(RBrush brush, RGraphicsPath path);
    public abstract void DrawPolygon(RBrush brush, RPoint[] points);
    public abstract void Dispose();
}
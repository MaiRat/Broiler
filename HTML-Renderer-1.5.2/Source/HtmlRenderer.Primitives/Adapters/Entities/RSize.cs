using System;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities;

public struct RSize
{
    public static readonly RSize Empty = new();

    public RSize(RSize size)
    {
        Width = size.Width;
        Height = size.Height;
    }

    public RSize(RPoint pt)
    {
        Width = pt.X;
        Height = pt.Y;
    }

    public RSize(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public readonly bool IsEmpty => Math.Abs(Width) < 0.0001 && Math.Abs(Height) < 0.0001;

    public double Width { readonly get; set; }

    public double Height { readonly get; set; }

    public static explicit operator RPoint(RSize size) => new(size.Width, size.Height);

    public static RSize operator +(RSize sz1, RSize sz2) => Add(sz1, sz2);

    public static RSize operator -(RSize sz1, RSize sz2) => Subtract(sz1, sz2);

    public static bool operator ==(RSize sz1, RSize sz2) => Math.Abs(sz1.Width - sz2.Width) < 0.001 && Math.Abs(sz1.Height - sz2.Height) < 0.001;

    public static bool operator !=(RSize sz1, RSize sz2) => !(sz1 == sz2);

    public static RSize Add(RSize sz1, RSize sz2) => new(sz1.Width + sz2.Width, sz1.Height + sz2.Height);

    public static RSize Subtract(RSize sz1, RSize sz2) => new(sz1.Width - sz2.Width, sz1.Height - sz2.Height);

    public override readonly bool Equals(object obj)
    {
        if (obj is not RSize)
            return false;

        var sizeF = (RSize)obj;

        if (Math.Abs(sizeF.Width - Width) < 0.001 && Math.Abs(sizeF.Height - Height) < 0.001)
            return sizeF.GetType() == GetType();
        else
            return false;
    }

    public override readonly int GetHashCode() => base.GetHashCode();

    public readonly RPoint ToPointF() => (RPoint)this;

    public override readonly string ToString() => "{Width=" + Width + ", Height=" + Height + "}";
}
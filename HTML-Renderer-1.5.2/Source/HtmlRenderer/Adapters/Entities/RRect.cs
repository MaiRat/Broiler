using System;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities;

public struct RRect
{
    public static readonly RRect Empty = new();

    public RRect(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public RRect(RPoint location, RSize size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public RPoint Location
    {
        readonly get { return new RPoint(X, Y); }
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public RSize Size
    {
        readonly get { return new RSize(Width, Height); }
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    public double X { readonly get; set; }

    public double Y { readonly get; set; }

    public double Width { readonly get; set; }

    public double Height { readonly get; set; }

    public readonly double Left => X;

    public readonly double Top => Y;

    public readonly double Right => X + Width;

    public readonly double Bottom => Y + Height;

    public readonly bool IsEmpty => Width <= 0.0 || Height <= 0.0;

    public static bool operator ==(RRect left, RRect right)
    {
        if (Math.Abs(left.X - right.X) < 0.001 && Math.Abs(left.Y - right.Y) < 0.001 && Math.Abs(left.Width - right.Width) < 0.001)
            return Math.Abs(left.Height - right.Height) < 0.001;
        else
            return false;
    }

    public static bool operator !=(RRect left, RRect right) => !(left == right);
    public static RRect FromLTRB(double left, double top, double right, double bottom) => new(left, top, right - left, bottom - top);

    public override readonly bool Equals(object obj)
    {
        if (obj is not RRect)
            return false;

        var rectangleF = (RRect)obj;

        if (Math.Abs(rectangleF.X - X) < 0.001 && Math.Abs(rectangleF.Y - Y) < 0.001 && Math.Abs(rectangleF.Width - Width) < 0.001)
            return Math.Abs(rectangleF.Height - Height) < 0.001;
        else
            return false;
    }

    public readonly bool Contains(double x, double y)
    {
        if (X <= x && x < X + Width && Y <= y)
            return y < Y + Height;
        else
            return false;
    }

    public readonly bool Contains(RPoint pt) => Contains(pt.X, pt.Y);

    public readonly bool Contains(RRect rect)
    {
        if (X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y)
            return rect.Y + rect.Height <= Y + Height;
        else
            return false;
    }

    public void Inflate(double x, double y)
    {
        X -= x;
        Y -= y;
        Width += 2f * x;
        Height += 2f * y;
    }

    public void Inflate(RSize size) => Inflate(size.Width, size.Height);

    public static RRect Inflate(RRect rect, double x, double y)
    {
        RRect rectangleF = rect;
        rectangleF.Inflate(x, y);
        return rectangleF;
    }

    public void Intersect(RRect rect)
    {
        RRect rectangleF = Intersect(rect, this);
        X = rectangleF.X;
        Y = rectangleF.Y;
        Width = rectangleF.Width;
        Height = rectangleF.Height;
    }

    public static RRect Intersect(RRect a, RRect b)
    {
        double x = Math.Max(a.X, b.X);
        double num1 = Math.Min(a.X + a.Width, b.X + b.Width);
        double y = Math.Max(a.Y, b.Y);
        double num2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

        if (num1 >= x && num2 >= y)
            return new RRect(x, y, num1 - x, num2 - y);
        else
            return Empty;
    }

    public readonly bool IntersectsWith(RRect rect)
    {
        if (rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height)
            return Y < rect.Y + rect.Height;
        else
            return false;
    }

    public static RRect Union(RRect a, RRect b)
    {
        double x = Math.Min(a.X, b.X);
        double num1 = Math.Max(a.X + a.Width, b.X + b.Width);
        double y = Math.Min(a.Y, b.Y);
        double num2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

        return new RRect(x, y, num1 - x, num2 - y);
    }

    public void Offset(RPoint pos) => Offset(pos.X, pos.Y);

    public void Offset(double x, double y)
    {
        X += x;
        Y += y;
    }

    public override readonly int GetHashCode() => (int)(uint)X ^ ((int)(uint)Y << 13 | (int)((uint)Y >> 19)) ^ ((int)(uint)Width << 26 | (int)((uint)Width >> 6)) ^ ((int)(uint)Height << 7 | (int)((uint)Height >> 25));

    public override readonly string ToString() => "{X=" + X + ",Y=" + Y + ",Width=" + Width + ",Height=" + Height + "}";
}
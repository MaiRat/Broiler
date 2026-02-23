using System;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities;

public struct RPoint(double x, double y)
{
    public static readonly RPoint Empty = new();

    static RPoint()
    { }

    public readonly bool IsEmpty
    {
        get
        {
            if (Math.Abs(x - 0.0) < 0.001)
                return Math.Abs(y - 0.0) < 0.001;
            else
                return false;
        }
    }

    public double X
    {
        readonly get { return x; }
        set { x = value; }
    }

    public double Y
    {
        readonly get { return y; }
        set { y = value; }
    }

    public static RPoint operator +(RPoint pt, RSize sz) => Add(pt, sz);
    public static RPoint operator -(RPoint pt, RSize sz) => Subtract(pt, sz);

    public static bool operator ==(RPoint left, RPoint right)
    {
        if (left.X == right.X)
            return left.Y == right.Y;
        else
            return false;
    }

    public static bool operator !=(RPoint left, RPoint right) => !(left == right);

    public static RPoint Add(RPoint pt, RSize sz) => new(pt.X + sz.Width, pt.Y + sz.Height);

    public static RPoint Subtract(RPoint pt, RSize sz) => new(pt.X - sz.Width, pt.Y - sz.Height);

    public override readonly bool Equals(object obj)
    {
        if (obj is not RPoint)
            return false;

        var pointF = (RPoint)obj;

        if (pointF.X == X && pointF.Y == Y)
            return pointF.GetType().Equals(GetType());
        else
            return false;
    }

    public override readonly int GetHashCode() => base.GetHashCode();

    public override readonly string ToString() => string.Format("{{X={0}, Y={1}}}", [x, y]);
}
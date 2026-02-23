using System;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities;

public readonly struct RColor
{
    public static readonly RColor Empty = new();
    private readonly long _value;

    private RColor(long value) => _value = value;

    public static RColor Transparent => new(0);

    public static RColor Black => FromArgb(0, 0, 0);

    public static RColor White => FromArgb(255, 255, 255);

    public static RColor WhiteSmoke => FromArgb(245, 245, 245);

    public static RColor LightGray => FromArgb(211, 211, 211);

    public byte R => (byte)((ulong)(_value >> 16) & byte.MaxValue);

    public byte G => (byte)((ulong)(_value >> 8) & byte.MaxValue);

    public byte B => (byte)((ulong)_value & byte.MaxValue);

    public byte A => (byte)((ulong)(_value >> 24) & byte.MaxValue);

    public bool IsEmpty => _value == 0;

    public static bool operator ==(RColor left, RColor right) => left._value == right._value;

    public static bool operator !=(RColor left, RColor right) => !(left == right);

    public static RColor FromArgb(int alpha, int red, int green, int blue)
    {
        CheckByte(alpha);
        CheckByte(red);
        CheckByte(green);
        CheckByte(blue);

        return new RColor((uint)(red << 16 | green << 8 | blue | alpha << 24) & (long)uint.MaxValue);
    }

    public static RColor FromArgb(int red, int green, int blue) => FromArgb(byte.MaxValue, red, green, blue);

    public override bool Equals(object obj)
    {
        if (obj is RColor color)
            return _value == color._value;

        return false;
    }

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString()
    {
        var stringBuilder = new StringBuilder(32);

        stringBuilder.Append(GetType().Name);
        stringBuilder.Append(" [");

        if (_value != 0)
        {
            stringBuilder.Append("A=");
            stringBuilder.Append(A);
            stringBuilder.Append(", R=");
            stringBuilder.Append(R);
            stringBuilder.Append(", G=");
            stringBuilder.Append(G);
            stringBuilder.Append(", B=");
            stringBuilder.Append(B);
        }
        else
            stringBuilder.Append("Empty");
    
        stringBuilder.Append(']');
    
        return stringBuilder.ToString();
    }

    private static void CheckByte(int value)
    {
        if (value >= 0 && value <= byte.MaxValue)
            return;

        throw new ArgumentException("InvalidEx2BoundArgument");
    }
}
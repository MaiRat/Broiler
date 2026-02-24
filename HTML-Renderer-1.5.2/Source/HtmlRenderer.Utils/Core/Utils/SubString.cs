using System;

namespace TheArtOfDev.HtmlRenderer.Core.Utils;

internal sealed class SubString
{
    private readonly int _startIdx;

    public SubString(string fullString)
    {
        ArgChecker.AssertArgNotNull(fullString, "fullString");

        FullString = fullString;
        _startIdx = 0;
        Length = fullString.Length;
    }

    public SubString(string fullString, int startIdx, int length)
    {
        ArgChecker.AssertArgNotNull(fullString, "fullString");

        if (startIdx < 0 || startIdx >= fullString.Length)
            throw new ArgumentOutOfRangeException("startIdx", "Must within fullString boundries");

        if (length < 0 || startIdx + length > fullString.Length)
            throw new ArgumentOutOfRangeException("length", "Must within fullString boundries");

        FullString = fullString;
        _startIdx = startIdx;
        Length = length;
    }

    public string FullString { get; }
    public int StartIdx => _startIdx;
    public int Length { get; }

    public char this[int idx]
    {
        get
        {
            if (idx < 0 || idx > Length)
                throw new ArgumentOutOfRangeException("idx", "must be within the string range");

            return FullString[_startIdx + idx];
        }
    }

    public bool IsEmpty() => Length < 1;

    public bool IsEmptyOrWhitespace()
    {
        for (int i = 0; i < Length; i++)
        {
            if (!char.IsWhiteSpace(FullString, _startIdx + i))
                return false;
        }

        return true;
    }

    public bool IsWhitespace()
    {
        if (Length < 1)
            return false;

        for (int i = 0; i < Length; i++)
        {
            if (!char.IsWhiteSpace(FullString, _startIdx + i))
                return false;
        }

        return true;
    }

    public string CutSubstring() => Length > 0 ? FullString.Substring(_startIdx, Length) : string.Empty;

    public string Substring(int startIdx, int length)
    {
        if (startIdx < 0 || startIdx > Length)
            throw new ArgumentOutOfRangeException(nameof(startIdx));

        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, Length);

        if (startIdx + length > Length)
            throw new ArgumentOutOfRangeException("length");

        return FullString.Substring(_startIdx + startIdx, length);
    }

    public override string ToString() => $"Sub-string: {(Length > 0 ? FullString.Substring(_startIdx, Length) : string.Empty)}";
}
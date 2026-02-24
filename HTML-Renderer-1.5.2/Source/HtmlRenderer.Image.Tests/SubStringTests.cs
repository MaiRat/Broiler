using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Unit tests for the <see cref="SubString"/> lightweight substring wrapper.
/// </summary>
public class SubStringTests
{
    [Fact]
    public void Constructor_FullString_CapturesEntireString()
    {
        var sub = new SubString("hello world");
        Assert.Equal(11, sub.Length);
        Assert.Equal("hello world", sub.CutSubstring());
    }

    [Fact]
    public void Constructor_Range_CapturesSubstring()
    {
        var sub = new SubString("hello world", 6, 5);
        Assert.Equal(5, sub.Length);
        Assert.Equal("world", sub.CutSubstring());
    }

    [Fact]
    public void Indexer_ReturnsCorrectCharacter()
    {
        var sub = new SubString("abcdef", 2, 3);
        Assert.Equal('c', sub[0]);
        Assert.Equal('d', sub[1]);
        Assert.Equal('e', sub[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var sub = new SubString("abc", 0, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = sub[-1]);
    }

    [Fact]
    public void IsEmpty_ZeroLength_ReturnsTrue()
    {
        var sub = new SubString("abc", 1, 0);
        Assert.True(sub.IsEmpty());
    }

    [Fact]
    public void IsEmpty_NonZeroLength_ReturnsFalse()
    {
        var sub = new SubString("abc", 0, 3);
        Assert.False(sub.IsEmpty());
    }

    [Fact]
    public void IsWhitespace_AllSpaces_ReturnsTrue()
    {
        var sub = new SubString("   ", 0, 3);
        Assert.True(sub.IsWhitespace());
    }

    [Fact]
    public void IsWhitespace_Empty_ReturnsFalse()
    {
        var sub = new SubString("abc", 0, 0);
        Assert.False(sub.IsWhitespace());
    }

    [Fact]
    public void IsEmptyOrWhitespace_Whitespace_ReturnsTrue()
    {
        var sub = new SubString("  \t  ", 0, 5);
        Assert.True(sub.IsEmptyOrWhitespace());
    }

    [Fact]
    public void IsEmptyOrWhitespace_NonWhitespace_ReturnsFalse()
    {
        var sub = new SubString("abc", 0, 3);
        Assert.False(sub.IsEmptyOrWhitespace());
    }

    [Fact]
    public void Substring_ExtractsCorrectly()
    {
        var sub = new SubString("hello world", 0, 11);
        Assert.Equal("world", sub.Substring(6, 5));
    }

    [Fact]
    public void Substring_InvalidRange_Throws()
    {
        var sub = new SubString("hello", 0, 5);
        Assert.Throws<ArgumentOutOfRangeException>(() => sub.Substring(3, 5));
    }

    [Fact]
    public void CutSubstring_EmptyLength_ReturnsEmpty()
    {
        var sub = new SubString("abc", 1, 0);
        Assert.Equal(string.Empty, sub.CutSubstring());
    }

    [Fact]
    public void Constructor_NullFullString_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SubString(null!));
    }

    [Fact]
    public void Constructor_InvalidStartIndex_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SubString("abc", -1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SubString("abc", 5, 1));
    }

    [Fact]
    public void Constructor_InvalidLength_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SubString("abc", 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SubString("abc", 1, 5));
    }

    [Fact]
    public void ToString_IncludesContent()
    {
        var sub = new SubString("hello", 0, 5);
        string str = sub.ToString();
        Assert.Contains("hello", str);
    }
}

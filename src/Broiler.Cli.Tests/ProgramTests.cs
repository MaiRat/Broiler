using Microsoft.Playwright;

namespace Broiler.Cli.Tests;

public class ProgramTests
{
    [Fact]
    public async Task Main_WithHelp_ReturnsZero()
    {
        var result = await Program.Main(["--help"]);
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Main_WithNoArgs_ReturnsOne()
    {
        var result = await Program.Main([]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithMissingOutput_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithMissingUrl_ReturnsOne()
    {
        var result = await Program.Main(["--output", "test.png"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithInvalidUrl_ReturnsOne()
    {
        var result = await Program.Main(["--url", "not-a-url", "--output", "test.png"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithNonHttpUrl_ReturnsOne()
    {
        var result = await Program.Main(["--url", "ftp://example.com", "--output", "test.png"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithUnrecognizedArg_ReturnsOne()
    {
        var result = await Program.Main(["--unknown"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithInvalidTimeout_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.png", "--timeout", "abc"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithNegativeTimeout_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.png", "--timeout", "-5"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithZeroTimeout_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.png", "--timeout", "0"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithTimeoutMissingValue_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.png", "--timeout"]);
        Assert.Equal(1, result);
    }
}

public class CaptureOptionsTests
{
    [Fact]
    public void ScreenshotType_PngExtension_ReturnsPng()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.png" };
        Assert.Equal(ScreenshotType.Png, options.ScreenshotType);
    }

    [Fact]
    public void ScreenshotType_JpgExtension_ReturnsJpeg()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.jpg" };
        Assert.Equal(ScreenshotType.Jpeg, options.ScreenshotType);
    }

    [Fact]
    public void ScreenshotType_JpegExtension_ReturnsJpeg()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.jpeg" };
        Assert.Equal(ScreenshotType.Jpeg, options.ScreenshotType);
    }

    [Fact]
    public void ScreenshotType_UpperCaseJpg_ReturnsJpeg()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.JPG" };
        Assert.Equal(ScreenshotType.Jpeg, options.ScreenshotType);
    }

    [Fact]
    public void ScreenshotType_UnknownExtension_DefaultsToPng()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.bmp" };
        Assert.Equal(ScreenshotType.Png, options.ScreenshotType);
    }

    [Fact]
    public void DefaultTimeout_IsThirtySeconds()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.png" };
        Assert.Equal(30, options.TimeoutSeconds);
    }

    [Fact]
    public void DefaultFullPage_IsFalse()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.png" };
        Assert.False(options.FullPage);
    }
}

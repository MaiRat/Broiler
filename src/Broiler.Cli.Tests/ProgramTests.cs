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
        var result = await Program.Main(["--output", "test.html"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithInvalidUrl_ReturnsOne()
    {
        var result = await Program.Main(["--url", "not-a-url", "--output", "test.html"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithNonHttpUrl_ReturnsOne()
    {
        var result = await Program.Main(["--url", "ftp://example.com", "--output", "test.html"]);
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
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.html", "--timeout", "abc"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithNegativeTimeout_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.html", "--timeout", "-5"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithZeroTimeout_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.html", "--timeout", "0"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithTimeoutMissingValue_ReturnsOne()
    {
        var result = await Program.Main(["--url", "https://example.com", "--output", "test.html", "--timeout"]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Main_WithTestEngines_ReturnsZero()
    {
        var result = await Program.Main(["--test-engines"]);
        Assert.Equal(0, result);
    }

    [Fact]
    public void RunEngineTests_ProducesOutput()
    {
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var exitCode = Program.RunEngineTests();
            var output = writer.ToString();

            Assert.Equal(0, exitCode);
            Assert.Contains("[PASS] HTML-Renderer", output);
            Assert.Contains("[PASS] YantraJS", output);
            Assert.Contains("All engine tests passed.", output);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }
}

public class CaptureOptionsTests
{
    [Fact]
    public void OutputFormat_HtmlExtension_ReturnsHtml()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.html" };
        Assert.Equal(OutputFormat.Html, options.OutputFormat);
    }

    [Fact]
    public void OutputFormat_TxtExtension_ReturnsText()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.txt" };
        Assert.Equal(OutputFormat.Text, options.OutputFormat);
    }

    [Fact]
    public void OutputFormat_UnknownExtension_DefaultsToHtml()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.bmp" };
        Assert.Equal(OutputFormat.Html, options.OutputFormat);
    }

    [Fact]
    public void DefaultTimeout_IsThirtySeconds()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.html" };
        Assert.Equal(30, options.TimeoutSeconds);
    }

    [Fact]
    public void DefaultFullPage_IsFalse()
    {
        var options = new CaptureOptions { Url = "https://example.com", OutputPath = "output.html" };
        Assert.False(options.FullPage);
    }
}

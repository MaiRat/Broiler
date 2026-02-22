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
}

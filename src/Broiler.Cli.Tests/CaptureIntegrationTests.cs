using System.Net;

namespace Broiler.Cli.Tests;

/// <summary>
/// Integration tests for the CaptureService using a local HTTP server.
/// These tests require Playwright browsers to be installed. When the
/// browser is not available, they are skipped with a diagnostic message.
/// </summary>
public class CaptureIntegrationTests : IDisposable
{
    private readonly HttpListener _listener;
    private readonly string _prefix;
    private readonly string _outputDir;

    public CaptureIntegrationTests()
    {
        // Find an available port by binding to port 0
        var tempListener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        tempListener.Start();
        var port = ((IPEndPoint)tempListener.LocalEndpoint).Port;
        tempListener.Stop();

        _prefix = $"http://localhost:{port}/";
        _listener = new HttpListener();
        _listener.Prefixes.Add(_prefix);
        _outputDir = Path.Combine(Path.GetTempPath(), $"broiler-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);
    }

    public void Dispose()
    {
        try { _listener.Stop(); } catch { }
        try { Directory.Delete(_outputDir, true); } catch { }
    }

    /// <summary>
    /// Checks whether Playwright can launch a browser. Returns <c>true</c>
    /// if browsers are installed, <c>false</c> otherwise.
    /// </summary>
    private static async Task<bool> IsBrowserAvailableAsync()
    {
        try
        {
            using var pw = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
            return true;
        }
        catch (Microsoft.Playwright.PlaywrightException)
        {
            return false;
        }
    }

    [Fact]
    public async Task CaptureAsync_LocalHtmlFile_ProducesScreenshot()
    {
        if (!await IsBrowserAvailableAsync())
        {
            // Browser not installed — cannot run capture tests.
            // Run 'dotnet playwright install chromium' to enable.
            return;
        }

        const string html = "<html><body><h1>Hello from Broiler</h1></body></html>";

        _listener.Start();
        var serverTask = Task.Run(() =>
        {
            var ctx = _listener.GetContext();
            var buffer = System.Text.Encoding.UTF8.GetBytes(html);
            ctx.Response.ContentType = "text/html";
            ctx.Response.ContentLength64 = buffer.Length;
            ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
            ctx.Response.Close();
        });

        var outputPath = Path.Combine(_outputDir, "capture.png");
        var service = new CaptureService();

        await service.CaptureAsync(new CaptureOptions
        {
            Url = _prefix,
            OutputPath = outputPath,
            TimeoutSeconds = 30,
        });

        await serverTask;

        Assert.True(File.Exists(outputPath), "Screenshot file should exist.");
        var fileInfo = new FileInfo(outputPath);
        Assert.True(fileInfo.Length > 0, "Screenshot file should not be empty.");
    }

    [Fact]
    public async Task CaptureAsync_InvalidUrl_ThrowsPlaywrightException()
    {
        if (!await IsBrowserAvailableAsync())
        {
            // Browser not installed — cannot run capture tests.
            // Run 'dotnet playwright install chromium' to enable.
            return;
        }

        var outputPath = Path.Combine(_outputDir, "fail.png");
        var service = new CaptureService();

        await Assert.ThrowsAsync<Microsoft.Playwright.PlaywrightException>(() =>
            service.CaptureAsync(new CaptureOptions
            {
                Url = "http://localhost:1/nonexistent",
                OutputPath = outputPath,
                TimeoutSeconds = 5,
            }));
    }
}


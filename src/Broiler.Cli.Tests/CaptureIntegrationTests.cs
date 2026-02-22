using System.Net;

namespace Broiler.Cli.Tests;

/// <summary>
/// Integration tests for the CaptureService using a local HTTP server.
/// Tests use the local rendering engines (HTML-Renderer and YantraJS)
/// instead of Playwright/Chromium.
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

    [Fact]
    public async Task CaptureAsync_LocalHtmlFile_ProducesOutput()
    {
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

        var outputPath = Path.Combine(_outputDir, "capture.html");
        var service = new CaptureService();

        await service.CaptureAsync(new CaptureOptions
        {
            Url = _prefix,
            OutputPath = outputPath,
            TimeoutSeconds = 30,
        });

        await serverTask;

        Assert.True(File.Exists(outputPath), "Captured file should exist.");
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("Hello from Broiler", content);
    }

    [Fact]
    public async Task CaptureAsync_InvalidUrl_ThrowsHttpRequestException()
    {
        var outputPath = Path.Combine(_outputDir, "fail.html");
        var service = new CaptureService();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.CaptureAsync(new CaptureOptions
            {
                Url = "http://localhost:1/nonexistent",
                OutputPath = outputPath,
                TimeoutSeconds = 5,
            }));
    }
}


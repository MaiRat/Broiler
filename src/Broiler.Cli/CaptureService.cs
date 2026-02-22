using Microsoft.Playwright;

namespace Broiler.Cli;

/// <summary>
/// Options for configuring a website capture operation.
/// </summary>
public class CaptureOptions
{
    /// <summary>
    /// The URL of the website to capture.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// The output file path for the screenshot.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Whether to capture the full scrollable page or only the viewport.
    /// Defaults to <c>false</c> (viewport only).
    /// </summary>
    public bool FullPage { get; init; }

    /// <summary>
    /// Navigation timeout in seconds. Defaults to 30.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Determines the screenshot format from the output file extension.
    /// Returns <see cref="ScreenshotType.Jpeg"/> for .jpg/.jpeg files,
    /// otherwise <see cref="ScreenshotType.Png"/>.
    /// </summary>
    public ScreenshotType ScreenshotType
    {
        get
        {
            var ext = Path.GetExtension(OutputPath);
            return ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                ? ScreenshotType.Jpeg
                : ScreenshotType.Png;
        }
    }
}

/// <summary>
/// Service that captures website screenshots using Playwright.
/// </summary>
public class CaptureService
{
    /// <summary>
    /// Captures a screenshot of the specified URL and saves it to the output path.
    /// </summary>
    /// <param name="options">Capture configuration options.</param>
    /// <returns>A task that completes when the capture is finished.</returns>
    /// <exception cref="PlaywrightException">Thrown when the browser cannot be launched or navigation fails.</exception>
    /// <exception cref="IOException">Thrown when the output file cannot be written.</exception>
    public async Task CaptureAsync(CaptureOptions options)
    {
        try
        {
            var outputDir = Path.GetDirectoryName(Path.GetFullPath(options.OutputPath));
            if (outputDir != null && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or PathTooLongException)
        {
            throw new IOException($"Cannot create output directory: {ex.Message}", ex);
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(options.Url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = (float)options.TimeoutSeconds * 1_000,
        });

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = options.OutputPath,
            FullPage = options.FullPage,
            Type = options.ScreenshotType,
        });
    }
}

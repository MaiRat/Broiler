using Microsoft.Playwright;

namespace Broiler.Cli;

/// <summary>
/// Entry point for the Broiler CLI tool.
/// Supports website capture via Playwright and engine smoke testing.
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        string? url = null;
        string? output = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--url" when i + 1 < args.Length:
                    url = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    output = args[++i];
                    break;
                case "--help":
                    PrintUsage();
                    return 0;
            }
        }

        if (url is null || output is null)
        {
            Console.Error.WriteLine("Error: Both --url and --output arguments are required.");
            PrintUsage();
            return 1;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            Console.Error.WriteLine($"Error: '{url}' is not a valid HTTP or HTTPS URL.");
            return 1;
        }

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000,
            });

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = output,
                FullPage = true,
            });

            Console.WriteLine($"Screenshot saved to {output}");
            return 0;
        }
        catch (PlaywrightException ex)
        {
            Console.Error.WriteLine($"Capture failed: {ex.Message}");
            Console.Error.WriteLine("Hint: Run 'dotnet playwright install chromium' to install the required browser.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: Broiler.Cli --url <URL> --output <FILE>");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --url <URL>      The URL of the website to capture");
        Console.WriteLine("  --output <FILE>  The output file path for the screenshot");
        Console.WriteLine("  --help           Show this help message");
    }
}

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
        bool fullPage = false;
        int timeoutSeconds = 30;

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
                case "--timeout" when i + 1 < args.Length:
                    if (!int.TryParse(args[++i], out timeoutSeconds) || timeoutSeconds <= 0)
                    {
                        Console.Error.WriteLine("Error: '--timeout' must be a positive integer (seconds).");
                        return 1;
                    }
                    break;
                case "--full-page":
                    fullPage = true;
                    break;
                case "--url":
                case "--output":
                case "--timeout":
                    Console.Error.WriteLine($"Error: '{args[i]}' requires a value.");
                    PrintUsage();
                    return 1;
                case "--help":
                    PrintUsage();
                    return 0;
                default:
                    Console.Error.WriteLine($"Error: Unrecognized argument '{args[i]}'.");
                    PrintUsage();
                    return 1;
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

        var captureOptions = new CaptureOptions
        {
            Url = url,
            OutputPath = output,
            FullPage = fullPage,
            TimeoutSeconds = timeoutSeconds,
        };

        try
        {
            var service = new CaptureService();
            await service.CaptureAsync(captureOptions);

            Console.WriteLine($"Screenshot saved to {output}");
            return 0;
        }
        catch (PlaywrightException ex)
        {
            Console.Error.WriteLine($"Capture failed: {ex.Message}");
            Console.Error.WriteLine("Hint: Run 'dotnet playwright install chromium' to install the required browser.");
            return 1;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"File I/O error: {ex.Message}");
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
        Console.WriteLine("Usage: Broiler.Cli --url <URL> --output <FILE> [OPTIONS]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --url <URL>        The URL of the website to capture");
        Console.WriteLine("  --output <FILE>    The output file path for the screenshot (PNG or JPEG)");
        Console.WriteLine("  --full-page        Capture the full scrollable page instead of the viewport");
        Console.WriteLine("  --timeout <SECS>   Navigation timeout in seconds (default: 30)");
        Console.WriteLine("  --help             Show this help message");
    }
}

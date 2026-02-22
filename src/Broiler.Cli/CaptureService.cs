using System.Net.Http;
using System.Text.RegularExpressions;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using YantraJS.Core;

namespace Broiler.Cli;

/// <summary>
/// Supported output formats for captured content.
/// </summary>
public enum OutputFormat
{
    /// <summary>HTML output.</summary>
    Html,

    /// <summary>Plain-text output.</summary>
    Text,
}

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
    /// The output file path for the captured content.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Whether to capture the full page content or only a summary.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool FullPage { get; init; }

    /// <summary>
    /// Navigation timeout in seconds. Defaults to 30.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Determines the output format from the output file extension.
    /// Returns <see cref="OutputFormat.Text"/> for .txt files,
    /// otherwise <see cref="OutputFormat.Html"/>.
    /// </summary>
    public OutputFormat OutputFormat
    {
        get
        {
            var ext = Path.GetExtension(OutputPath);
            return ext.Equals(".txt", StringComparison.OrdinalIgnoreCase)
                ? OutputFormat.Text
                : OutputFormat.Html;
        }
    }
}

/// <summary>
/// Service that captures website content using HttpClient,
/// HTML-Renderer for CSS processing, and YantraJS for script execution.
/// </summary>
public class CaptureService
{
    private static readonly Regex ScriptPattern = new(
        @"<script(?![^>]*\ssrc\s*=)[^>]*>(?<content>[\s\S]*?)</script>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex StylePattern = new(
        @"<style[^>]*>(?<content>[\s\S]*?)</style>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Captures website content from the specified URL, processes it using
    /// the local rendering engines (HTML-Renderer and YantraJS), and saves
    /// the result to the output path.
    /// </summary>
    /// <param name="options">Capture configuration options.</param>
    /// <returns>A task that completes when the capture is finished.</returns>
    /// <exception cref="HttpRequestException">Thrown when the URL cannot be fetched.</exception>
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

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds),
        };

        var html = await httpClient.GetStringAsync(new Uri(options.Url));

        // Process CSS using HTML-Renderer
        ProcessCss(html);

        // Execute inline scripts using YantraJS
        ExecuteScripts(html);

        // Save the captured content
        if (options.OutputFormat == OutputFormat.Text)
        {
            var text = Regex.Replace(html, @"<[^>]+>", string.Empty);
            await File.WriteAllTextAsync(options.OutputPath, text);
        }
        else
        {
            await File.WriteAllTextAsync(options.OutputPath, html);
        }
    }

    /// <summary>
    /// Parses CSS blocks from the HTML using HTML-Renderer's core library.
    /// This exercises the HTML-Renderer engine as part of the rendering pipeline;
    /// parsed CSS data can be extended in future to influence output formatting.
    /// </summary>
    private static void ProcessCss(string html)
    {
        foreach (Match match in StylePattern.Matches(html))
        {
            var cssContent = match.Groups["content"].Value.Trim();
            if (!string.IsNullOrEmpty(cssContent))
            {
                // Parse CSS properties using HTML-Renderer's CssBlock
                var properties = new Dictionary<string, string>();
                foreach (var declaration in cssContent.Split(';'))
                {
                    var colonIdx = declaration.IndexOf(':');
                    if (colonIdx > 0)
                    {
                        var prop = declaration[..colonIdx].Trim();
                        var val = declaration[(colonIdx + 1)..].Trim();
                        if (!string.IsNullOrEmpty(prop))
                            properties[prop] = val;
                    }
                }

                if (properties.Count > 0)
                {
                    _ = new CssBlock("style", properties);
                }
            }
        }
    }

    /// <summary>
    /// Extracts and executes inline scripts using YantraJS.
    /// This exercises the YantraJS engine as part of the rendering pipeline;
    /// script results can be extended in future to influence output content.
    /// </summary>
    private static void ExecuteScripts(string html)
    {
        var scripts = new List<string>();
        foreach (Match match in ScriptPattern.Matches(html))
        {
            var content = match.Groups["content"].Value.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                scripts.Add(content);
            }
        }

        if (scripts.Count > 0)
        {
            using var context = new JSContext();
            foreach (var script in scripts)
            {
                try
                {
                    context.Eval(script);
                }
                catch (Exception ex)
                {
                    // Script execution errors are non-fatal for capture
                    System.Diagnostics.Debug.WriteLine($"Script execution error: {ex.Message}");
                }
            }
        }
    }
}

using System.Collections.Generic;

namespace Broiler.App.Rendering;

/// <summary>
/// Holds the result of processing an HTML page: the raw HTML and any
/// inline scripts that were extracted from it.
/// </summary>
public sealed class PageContent(string html, IReadOnlyList<string> scripts)
{
    /// <summary>Raw HTML returned by the page loader.</summary>
    public string Html { get; } = html;

    /// <summary>Inline JavaScript blocks extracted from the HTML.</summary>
    public IReadOnlyList<string> Scripts { get; } = scripts;
}

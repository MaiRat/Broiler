using System.Collections.Generic;

namespace Broiler.App.Rendering;

/// <summary>
/// Extracts inline JavaScript blocks from an HTML string.
/// </summary>
public interface IScriptExtractor
{
    /// <summary>
    /// Return the non-empty inline script contents found in <paramref name="html"/>.
    /// </summary>
    IReadOnlyList<string> Extract(string html);

    /// <summary>
    /// Return only inline module scripts (<c>&lt;script type="module"&gt;</c>)
    /// found in <paramref name="html"/>.
    /// </summary>
    IReadOnlyList<string> ExtractModules(string html);
}

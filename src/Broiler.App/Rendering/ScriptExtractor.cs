using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Broiler.App.Rendering;

/// <summary>
/// Extracts the contents of <c>&lt;script&gt;</c> tags from HTML using a
/// regular expression.  Only non-empty inline scripts are returned;
/// external <c>src</c> references are skipped.
/// </summary>
public sealed class ScriptExtractor : IScriptExtractor
{
    // Match <script> tags that do NOT have a src attribute (inline only)
    private static readonly Regex ScriptPattern = new(
        @"<script(?![^>]*\ssrc\s*=)[^>]*>(?<content>[\s\S]*?)</script>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Match <script type="module"> tags (inline only, no src)
    private static readonly Regex ModuleScriptPattern = new(
        @"<script\s[^>]*type\s*=\s*[""']module[""'][^>]*>(?<content>[\s\S]*?)</script>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Match the type="module" attribute on a script tag
    private static readonly Regex ModuleTypeAttribute = new(
        @"\stype\s*=\s*[""']module[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public IReadOnlyList<string> Extract(string html)
    {
        var scripts = new List<string>();

        foreach (Match match in ScriptPattern.Matches(html))
        {
            // Skip module scripts — they are extracted separately
            var tag = match.Value;
            if (ModuleTypeAttribute.IsMatch(tag))
                continue;

            var content = match.Groups["content"].Value.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                scripts.Add(content);
            }
        }

        return scripts;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ExtractModules(string html)
    {
        var modules = new List<string>();

        foreach (Match match in ModuleScriptPattern.Matches(html))
        {
            // Skip if it has a src attribute (external module)
            if (Regex.IsMatch(match.Value, @"\ssrc\s*=", RegexOptions.IgnoreCase))
                continue;

            var content = match.Groups["content"].Value.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                modules.Add(content);
            }
        }

        return modules;
    }
}

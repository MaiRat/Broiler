using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Broiler.App.Rendering
{
    /// <summary>
    /// Extracts the contents of <c>&lt;script&gt;</c> tags from HTML using a
    /// regular expression.  Only non-empty inline scripts are returned;
    /// external <c>src</c> references are ignored for now.
    /// </summary>
    public sealed class ScriptExtractor : IScriptExtractor
    {
        private static readonly Regex ScriptPattern = new(
            @"<script[^>]*>(?<content>[\s\S]*?)</script>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc />
        public IReadOnlyList<string> Extract(string html)
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

            return scripts;
        }
    }
}

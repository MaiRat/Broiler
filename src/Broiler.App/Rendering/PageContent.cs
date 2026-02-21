using System.Collections.Generic;

namespace Broiler.App.Rendering
{
    /// <summary>
    /// Holds the result of processing an HTML page: the raw HTML and any
    /// inline scripts that were extracted from it.
    /// </summary>
    public sealed class PageContent
    {
        /// <summary>Raw HTML returned by the page loader.</summary>
        public string Html { get; }

        /// <summary>Inline JavaScript blocks extracted from the HTML.</summary>
        public IReadOnlyList<string> Scripts { get; }

        public PageContent(string html, IReadOnlyList<string> scripts)
        {
            Html = html;
            Scripts = scripts;
        }
    }
}

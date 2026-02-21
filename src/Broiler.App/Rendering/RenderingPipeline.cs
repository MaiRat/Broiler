using System;
using System.Threading.Tasks;

namespace Broiler.App.Rendering
{
    /// <summary>
    /// Orchestrates the page rendering flow:
    /// fetch HTML → extract scripts → render HTML → execute scripts.
    /// </summary>
    public sealed class RenderingPipeline : IDisposable
    {
        private readonly IPageLoader _pageLoader;
        private readonly IScriptExtractor _scriptExtractor;
        private readonly IScriptEngine _scriptEngine;

        public RenderingPipeline(
            IPageLoader pageLoader,
            IScriptExtractor scriptExtractor,
            IScriptEngine scriptEngine)
        {
            _pageLoader = pageLoader;
            _scriptExtractor = scriptExtractor;
            _scriptEngine = scriptEngine;
        }

        /// <summary>
        /// Load a page from <paramref name="url"/>, extract inline scripts,
        /// and return a <see cref="PageContent"/> ready for rendering.
        /// The normalised URL (with scheme) is included in the result tuple.
        /// </summary>
        public async Task<(string NormalisedUrl, PageContent Content)> LoadPageAsync(string url)
        {
            var (normalisedUrl, html) = await _pageLoader.FetchAsync(url);
            var scripts = _scriptExtractor.Extract(html);
            return (normalisedUrl, new PageContent(html, scripts));
        }

        /// <summary>
        /// Execute the scripts contained in <paramref name="content"/>.
        /// </summary>
        public bool ExecuteScripts(PageContent content)
        {
            return _scriptEngine.Execute(content.Scripts);
        }

        public void Dispose()
        {
            _pageLoader.Dispose();
        }
    }
}

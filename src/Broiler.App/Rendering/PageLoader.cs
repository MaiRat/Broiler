using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Broiler.App.Rendering
{
    /// <summary>
    /// Fetches page content over HTTP(S) using <see cref="HttpClient"/>.
    /// </summary>
    public sealed class PageLoader : IPageLoader
    {
        private readonly HttpClient _httpClient = new();

        /// <inheritdoc />
        public async Task<(string NormalisedUrl, string Html)> FetchAsync(string url)
        {
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            var html = await _httpClient.GetStringAsync(new Uri(url));
            return (url, html);
        }

        public void Dispose() => _httpClient.Dispose();
    }
}

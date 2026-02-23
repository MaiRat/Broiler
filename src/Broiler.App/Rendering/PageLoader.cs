using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Broiler.App.Rendering;

/// <summary>
/// Fetches page content over HTTP(S) using <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
/// Creates a new <see cref="PageLoader"/> using the provided
/// <paramref name="httpClient"/>.  Callers should reuse a single
/// <see cref="HttpClient"/> instance to avoid socket exhaustion.
/// </remarks>
public sealed class PageLoader(HttpClient httpClient) : IPageLoader
{

    /// <inheritdoc />
    public async Task<(string NormalisedUrl, string Html)> FetchAsync(string url)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        var html = await httpClient.GetStringAsync(new Uri(url));
        return (url, html);
    }

    public void Dispose() => httpClient.Dispose();
}

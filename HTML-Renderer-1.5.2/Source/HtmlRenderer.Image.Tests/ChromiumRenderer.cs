using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core.IR;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Renders HTML using headless Chromium via Playwright for differential testing (Phase 6).
/// Screenshots are captured at the same viewport/DPI settings used by the Broiler engine.
/// </summary>
public sealed class ChromiumRenderer : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    /// <summary>
    /// Initialises Playwright and launches headless Chromium.
    /// Must be called once before any rendering.
    /// </summary>
    public async Task InitialiseAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    /// <summary>
    /// Renders the supplied <paramref name="html"/> in headless Chromium at the
    /// viewport dimensions specified in <paramref name="config"/> and returns the
    /// result as an <see cref="SKBitmap"/>.  The caller owns the returned bitmap.
    /// </summary>
    public async Task<SKBitmap> RenderAsync(string html, DeterministicRenderConfig? config = null)
    {
        if (_browser is null)
            throw new InvalidOperationException("Call InitialiseAsync() before rendering.");

        config ??= DeterministicRenderConfig.Default;

        var page = await _browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = config.ViewportWidth,
                Height = config.ViewportHeight
            },
            DeviceScaleFactor = 1,           // match Broiler's 96 DPI / 1× scale
            HasTouch = false,
            ColorScheme = ColorScheme.Light
        });

        try
        {
            // Load HTML as data-URI so no server is needed.
            await page.SetContentAsync(html, new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.Load
            });

            // Small delay for any pending paints.
            await page.WaitForTimeoutAsync(200);

            var pngBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = false,
                Type = ScreenshotType.Png
            });

            return SKBitmap.Decode(pngBytes);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Extracts <c>getBoundingClientRect()</c> for the element matched by the
    /// CSS <paramref name="selector"/> after rendering <paramref name="html"/>.
    /// Returns <c>null</c> if the element is not found.
    /// </summary>
    public async Task<LayoutRect?> GetBoundingClientRectAsync(
        string html, string selector, DeterministicRenderConfig? config = null)
    {
        if (_browser is null)
            throw new InvalidOperationException("Call InitialiseAsync() before rendering.");

        config ??= DeterministicRenderConfig.Default;

        var page = await _browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = config.ViewportWidth,
                Height = config.ViewportHeight
            },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Light
        });

        try
        {
            await page.SetContentAsync(html, new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.Load
            });

            var element = await page.QuerySelectorAsync(selector);
            if (element is null)
                return null;

            var box = await element.BoundingBoxAsync();
            if (box is null)
                return null;

            return new LayoutRect(box.X, box.Y, box.Width, box.Height);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
    }
}

/// <summary>
/// A simple rectangle returned by Chromium's <c>getBoundingClientRect()</c>.
/// </summary>
public readonly record struct LayoutRect(double X, double Y, double Width, double Height);

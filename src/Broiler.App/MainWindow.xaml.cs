using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WPF;
using YantraJS.Core;

namespace Broiler.App
{
    /// <summary>
    /// Main browser window with navigation bar and HTML rendering panel.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<string> _history = new();
        private int _historyIndex = -1;
        private readonly HttpClient _httpClient = new();

        public MainWindow()
        {
            InitializeComponent();
            NavigateTo("about:blank");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                LoadUrl(_history[_historyIndex]);
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
                LoadUrl(_history[_historyIndex]);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_historyIndex >= 0 && _historyIndex < _history.Count)
            {
                LoadUrl(_history[_historyIndex]);
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(UrlTextBox.Text);
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NavigateTo(UrlTextBox.Text);
            }
        }

        private void HtmlPanel_LinkClicked(object sender, RoutedEventArgs<HtmlLinkClickedEventArgs> e)
        {
            e.Handled = true;
            NavigateTo(e.Data.Link);
        }

        /// <summary>
        /// Navigate to a URL, adding it to the history stack.
        /// </summary>
        public void NavigateTo(string url)
        {
            // Remove forward history when navigating to a new URL
            if (_historyIndex < _history.Count - 1)
            {
                _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
            }

            _history.Add(url);
            _historyIndex = _history.Count - 1;
            LoadUrl(url);
        }

        private async void LoadUrl(string url)
        {
            UrlTextBox.Text = url;
            UpdateNavigationButtons();

            if (url == "about:blank")
            {
                RenderHtml(GetWelcomePage());
                StatusText.Text = "Ready";
                return;
            }

            try
            {
                StatusText.Text = $"Loading {url}...";
                var html = await FetchHtmlAsync(url);
                RenderHtml(html);
                StatusText.Text = "Done";
            }
            catch (Exception ex)
            {
                RenderHtml($"<html><body><h1>Error</h1><p>{ex.Message}</p></body></html>");
                StatusText.Text = "Error loading page";
            }
        }

        private void RenderHtml(string html)
        {
            HtmlPanel.Text = html;
            ExecuteJavaScript(html);
        }

        private void ExecuteJavaScript(string html)
        {
            // Extract and execute inline scripts using YantraJS
            try
            {
                var scripts = ExtractScripts(html);
                if (scripts.Count == 0) return;

                var context = new JSContext();
                foreach (var script in scripts)
                {
                    context.Eval(script);
                }
            }
            catch
            {
                // Script errors are silently handled for now
            }
        }

        private static List<string> ExtractScripts(string html)
        {
            var scripts = new List<string>();
            var searchFrom = 0;

            while (true)
            {
                var start = html.IndexOf("<script>", searchFrom, StringComparison.OrdinalIgnoreCase);
                if (start < 0) break;

                start += "<script>".Length;
                var end = html.IndexOf("</script>", start, StringComparison.OrdinalIgnoreCase);
                if (end < 0) break;

                scripts.Add(html.Substring(start, end - start).Trim());
                searchFrom = end + "</script>".Length;
            }

            return scripts;
        }

        private async Task<string> FetchHtmlAsync(string url)
        {
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
                UrlTextBox.Text = url;
            }

            return await _httpClient.GetStringAsync(new Uri(url));
        }

        private void UpdateNavigationButtons()
        {
            BackButton.IsEnabled = _historyIndex > 0;
            ForwardButton.IsEnabled = _historyIndex < _history.Count - 1;
        }

        private static string GetWelcomePage()
        {
            return @"
<html>
<head>
    <style>
        body { font-family: Segoe UI, Arial, sans-serif; margin: 40px; background: #fafafa; color: #333; }
        h1 { color: #2c3e50; }
        p { line-height: 1.6; }
        .info { background: #ecf0f1; padding: 16px; border-radius: 4px; margin-top: 20px; }
    </style>
</head>
<body>
    <h1>Welcome to Broiler</h1>
    <p>Broiler is a lightweight WPF web browser powered by HTML-Renderer and YantraJS.</p>
    <div class='info'>
        <p><strong>Getting Started:</strong> Enter a URL in the address bar above and press Enter or click Go.</p>
        <p><strong>Features:</strong></p>
        <ul>
            <li>HTML 4.01 and CSS Level 2 rendering</li>
            <li>JavaScript execution via YantraJS</li>
            <li>Navigation history (Back / Forward / Refresh)</li>
            <li>Link navigation</li>
        </ul>
    </div>
</body>
</html>";
        }
    }
}

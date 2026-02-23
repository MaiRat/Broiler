using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers;

public delegate void DownloadFileAsyncCallback(Uri imageUri, string filePath, Exception error, bool canceled);

internal sealed class ImageDownloader : IDisposable
{
    private readonly List<WebClient> _clients = [];
    private readonly Dictionary<string, List<DownloadFileAsyncCallback>> _imageDownloadCallbacks = [];

    public ImageDownloader() => ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

    public void DownloadImage(Uri imageUri, string filePath, bool async, DownloadFileAsyncCallback cachedFileCallback)
    {
        ArgChecker.AssertArgNotNull(imageUri, "imageUri");
        ArgChecker.AssertArgNotNull(cachedFileCallback, "cachedFileCallback");

        // to handle if the file is already been downloaded
        bool download = true;

        lock (_imageDownloadCallbacks)
        {
            if (_imageDownloadCallbacks.TryGetValue(filePath, out List<DownloadFileAsyncCallback> value))
            {
                download = false;
                value.Add(cachedFileCallback);
            }
            else
            {
                _imageDownloadCallbacks[filePath] = [cachedFileCallback];
            }
        }

        if (!download)
            return;

        var tempPath = Path.GetTempFileName();

        if (async)
            ThreadPool.QueueUserWorkItem(DownloadImageFromUrlAsync, new DownloadData(imageUri, tempPath, filePath));
        else
            DownloadImageFromUrl(imageUri, tempPath, filePath);
    }

    public void Dispose() => ReleaseObjects();


    private void DownloadImageFromUrl(Uri source, string tempPath, string filePath)
    {
        try
        {
            using var client = new WebClient();
            
            _clients.Add(client);
            client.DownloadFile(source, tempPath);
            OnDownloadImageCompleted(client, source, tempPath, filePath, null, false);
        }
        catch (Exception ex)
        {
            OnDownloadImageCompleted(null, source, tempPath, filePath, ex, false);
        }
    }

    private void DownloadImageFromUrlAsync(object data)
    {
        var downloadData = (DownloadData)data;
        try
        {
            using var client = new WebClient();
            _clients.Add(client);
            client.DownloadFileCompleted += OnDownloadImageAsyncCompleted;
            client.DownloadFileAsync(downloadData._uri, downloadData._tempPath, downloadData);
        }
        catch (Exception ex)
        {
            OnDownloadImageCompleted(null, downloadData._uri, downloadData._tempPath, downloadData._filePath, ex, false);
        }
    }

    private void OnDownloadImageAsyncCompleted(object sender, AsyncCompletedEventArgs e)
    {
        var downloadData = (DownloadData)e.UserState;
        try
        {
            using var client = (WebClient)sender;
            client.DownloadFileCompleted -= OnDownloadImageAsyncCompleted;
            OnDownloadImageCompleted(client, downloadData._uri, downloadData._tempPath, downloadData._filePath, e.Error, e.Cancelled);
        }
        catch (Exception ex)
        {
            OnDownloadImageCompleted(null, downloadData._uri, downloadData._tempPath, downloadData._filePath, ex, false);
        }
    }

    private void OnDownloadImageCompleted(WebClient client, Uri source, string tempPath, string filePath, Exception error, bool cancelled)
    {
        if (!cancelled)
        {
            if (error == null)
            {
                var contentType = CommonUtils.GetResponseContentType(client);
                if (contentType == null || !contentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    error = new Exception("Failed to load image, not image content type: " + contentType);
            }

            if (error == null)
            {
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Move(tempPath, filePath);
                    }
                    catch (Exception ex)
                    {
                        error = new Exception("Failed to move downloaded image from temp to cache location", ex);
                    }
                }

                error = File.Exists(filePath) ? null : (error ?? new Exception("Failed to download image, unknown error"));
            }
        }

        List<DownloadFileAsyncCallback> callbacksList;
        lock (_imageDownloadCallbacks)
        {
            if (_imageDownloadCallbacks.TryGetValue(filePath, out callbacksList))
                _imageDownloadCallbacks.Remove(filePath);
        }

        if (callbacksList == null)
            return;

        foreach (var cachedFileCallback in callbacksList)
        {
            try
            {
                cachedFileCallback(source, filePath, error, cancelled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HtmlRenderer] ImageDownloader callback error: {ex.Message}");
            }
        }
    }

    private void ReleaseObjects()
    {
        _imageDownloadCallbacks.Clear();
        while (_clients.Count > 0)
        {
            try
            {
                var client = _clients[0];
                client.CancelAsync();
                client.Dispose();
                _clients.RemoveAt(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HtmlRenderer] ImageDownloader.ReleaseObjects error: {ex.Message}");
            }
        }
    }

    private sealed class DownloadData(Uri uri, string tempPath, string filePath)
    {
        public readonly Uri _uri = uri;
        public readonly string _tempPath = tempPath;
        public readonly string _filePath = filePath;
    }
}

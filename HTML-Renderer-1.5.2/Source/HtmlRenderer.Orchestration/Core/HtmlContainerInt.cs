using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core;

public sealed class HtmlContainerInt : IHtmlContainerInt, IDisposable
{
    private List<HoverBoxBlock> _hoverBoxes;
    private ISelectionHandler _selectionHandler;
    private ImageDownloader _imageDownloader;
    private CssData _cssData;
    private bool _loadComplete;
    private int _marginTop;
    private int _marginBottom;
    private int _marginLeft;
    private int _marginRight;
    private readonly IHandlerFactory _handlerFactory;

    internal HtmlContainerInt(IAdapter adapter, IHandlerFactory handlerFactory)
    {
        ArgChecker.AssertArgNotNull(adapter, "global");
        ArgChecker.AssertArgNotNull(handlerFactory, "handlerFactory");

        Adapter = adapter;
        _handlerFactory = handlerFactory;
        CssParser = new CssParser(adapter);
    }

    internal IAdapter Adapter { get; }

    internal CssParser CssParser { get; }

    public event EventHandler LoadComplete;
    public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;
    public event EventHandler<HtmlRefreshEventArgs> Refresh;
    public event EventHandler<HtmlScrollEventArgs> ScrollChange;
    public event EventHandler<HtmlRenderErrorEventArgs> RenderError;
    public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;
    public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

    public CssData CssData => _cssData;

    public bool AvoidGeometryAntialias { get; set; }

    public bool AvoidAsyncImagesLoading { get; set; }

    public bool AvoidImagesLateLoading { get; set; }

    public bool IsSelectionEnabled { get; set; } = true;

    public bool IsContextMenuEnabled { get; set; } = true;

    public RPoint ScrollOffset { get; set; }

    public RPoint Location { get; set; }

    public RSize MaxSize { get; set; }

    public RSize ActualSize { get; set; }

    public RSize PageSize { get; set; }

    public int MarginTop
    {
        get { return _marginTop; }
        set
        {
            if (value > -1)
                _marginTop = value;
        }
    }

    public int MarginBottom
    {
        get { return _marginBottom; }
        set
        {
            if (value > -1)
                _marginBottom = value;
        }
    }

    public int MarginLeft
    {
        get { return _marginLeft; }
        set
        {
            if (value > -1)
                _marginLeft = value;
        }
    }

    public int MarginRight
    {
        get { return _marginRight; }
        set
        {
            if (value > -1)
                _marginRight = value;
        }
    }

    public void SetMargins(int value)
    {
        if (value > -1)
            _marginBottom = _marginLeft = _marginTop = _marginRight = value;
    }

    public string SelectedText => _selectionHandler.GetSelectedText();
    public string SelectedHtml => _selectionHandler.GetSelectedHtml();
    internal CssBox Root { get; private set; }
    internal RColor SelectionForeColor { get; set; }
    internal RColor SelectionBackColor { get; set; }
    public void SetHtml(string htmlSource, CssData baseCssData = null)
    {
        Clear();

        if (string.IsNullOrEmpty(htmlSource))
            return;

        _loadComplete = false;
        _cssData = baseCssData ?? Adapter.DefaultCssData;

        DomParser parser = new(CssParser, new StylesheetLoadHandler(this));
        Root = parser.GenerateCssTree(htmlSource, this, ref _cssData);

        if (Root == null)
            return;

        _selectionHandler = _handlerFactory.CreateSelectionHandler(Root);
        _imageDownloader = new ImageDownloader();
    }

    public void Clear()
    {
        if (Root == null)
            return;

        Root.Dispose();
        Root = null;

        _selectionHandler?.Dispose();
        _selectionHandler = null;

        _imageDownloader?.Dispose();
        _imageDownloader = null;

        _hoverBoxes = null;
    }

    public void ClearSelection()
    {
        if (_selectionHandler == null)
            return;

        _selectionHandler.ClearSelection();
        RequestRefresh(false);
    }

    public string GetHtml(HtmlGenerationStyle styleGen = HtmlGenerationStyle.Inline) => DomUtils.GenerateHtml(Root, styleGen);

    public string GetAttributeAt(RPoint location, string attribute)
    {
        ArgChecker.AssertArgNotNull(attribute, "attribute");

        var cssBox = DomUtils.GetCssBox(Root, OffsetByScroll(location));
        return cssBox != null ? DomUtils.GetAttribute(cssBox, attribute) : null;
    }

    public List<LinkElementData<RRect>> GetLinks()
    {
        var linkBoxes = new List<CssBox>();
        DomUtils.GetAllLinkBoxes(Root, linkBoxes);

        var linkElements = new List<LinkElementData<RRect>>();

        foreach (var box in linkBoxes)
            linkElements.Add(new LinkElementData<RRect>(box.GetAttribute("id"), box.GetAttribute("href"), CommonUtils.GetFirstValueOrDefault(box.Rectangles, box.Bounds)));

        return linkElements;
    }

    public string GetLinkAt(RPoint location)
    {
        var link = DomUtils.GetLinkBox(Root, OffsetByScroll(location));
        return link?.HrefLink;
    }

    public RRect? GetElementRectangle(string elementId)
    {
        ArgChecker.AssertArgNotNullOrEmpty(elementId, "elementId");

        var box = DomUtils.GetBoxById(Root, elementId.ToLower());
        return box != null ? CommonUtils.GetFirstValueOrDefault(box.Rectangles, box.Bounds) : (RRect?)null;
    }

    public void PerformLayout(RGraphics g)
    {
        ArgChecker.AssertArgNotNull(g, "g");

        ActualSize = RSize.Empty;
        if (Root == null)
            return;

        // if width is not restricted we set it to large value to get the actual later
        Root.Size = new RSize(MaxSize.Width > 0 ? MaxSize.Width : 99999, 0);
        Root.Location = Location;
        Root.PerformLayout(g);

        if (MaxSize.Width <= 0.1)
        {
            // in case the width is not restricted we need to double layout, first will find the width so second can layout by it (center alignment)
            Root.Size = new RSize((int)Math.Ceiling(ActualSize.Width), 0);
            ActualSize = RSize.Empty;
            Root.PerformLayout(g);
        }

        if (!_loadComplete)
        {
            _loadComplete = true;
            LoadComplete?.Invoke(this, EventArgs.Empty);
        }
    }

    public void PerformPaint(RGraphics g)
    {
        ArgChecker.AssertArgNotNull(g, "g");

        if (MaxSize.Height > 0)
        {
            g.PushClip(new RRect(Location.X, Location.Y, Math.Min(MaxSize.Width, PageSize.Width), Math.Min(MaxSize.Height, PageSize.Height)));
        }
        else
        {
            g.PushClip(new RRect(MarginLeft, MarginTop, PageSize.Width, PageSize.Height));
        }

        Root?.Paint(g);

        g.PopClip();
    }

    public void HandleMouseDown(object parent, RPoint location)
    {
        ArgChecker.AssertArgNotNull(parent, "parent");

        try
        {
            _selectionHandler?.HandleMouseDown(parent, OffsetByScroll(location), IsMouseInContainer(location));
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse down handle", ex);
        }
    }

    public void HandleMouseUp(object parent, RPoint location, RMouseEvent e)
    {
        ArgChecker.AssertArgNotNull(parent, "parent");

        try
        {
            if (_selectionHandler == null || !IsMouseInContainer(location))
                return;

            var ignore = _selectionHandler.HandleMouseUp(parent, e.LeftButton);
            if (!ignore && e.LeftButton)
            {
                var loc = OffsetByScroll(location);
                var link = DomUtils.GetLinkBox(Root, loc);
                if (link != null)
                    HandleLinkClicked(parent, location, link);
            }
        }
        catch (HtmlLinkClickedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse up handle", ex);
        }
    }

    public void HandleMouseDoubleClick(object parent, RPoint location)
    {
        ArgChecker.AssertArgNotNull(parent, "parent");

        try
        {
            if (_selectionHandler != null && IsMouseInContainer(location))
                _selectionHandler.SelectWord(parent, OffsetByScroll(location));
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse double click handle", ex);
        }
    }

    public void HandleMouseMove(object parent, RPoint location)
    {
        ArgChecker.AssertArgNotNull(parent, "parent");

        try
        {
            var loc = OffsetByScroll(location);
            if (_selectionHandler != null && IsMouseInContainer(location))
                _selectionHandler.HandleMouseMove(parent, loc);

            /*
            if( _hoverBoxes != null )
            {
                bool refresh = false;
                foreach(var hoverBox in _hoverBoxes)
                {
                    foreach(var rect in hoverBox.Item1.Rectangles.Values)
                    {
                        if( rect.Contains(loc) )
                        {
                            //hoverBox.Item1.Color = "gold";
                            refresh = true;
                        }
                    }
                }

                if(refresh)
                    RequestRefresh(true);
            }
             */
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse move handle", ex);
        }
    }

    public void HandleMouseLeave(object parent)
    {
        ArgChecker.AssertArgNotNull(parent, "parent");

        try
        {
            _selectionHandler?.HandleMouseLeave(parent);
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse leave handle", ex);
        }
    }

    public void HandleKeyDown(object parent, RKeyEvent e)
    {
        ArgChecker.AssertArgNotNull(parent, "parent");
        ArgChecker.AssertArgNotNull(e, "e");

        try
        {
            if (!e.Control || _selectionHandler == null)
                return;

            // select all
            if (e.AKeyCode)
                _selectionHandler.SelectAll(parent);

            // copy currently selected text
            if (e.CKeyCode)
                _selectionHandler.CopySelectedHtml();
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed key down handle", ex);
        }
    }

    internal void RaiseHtmlStylesheetLoadEvent(HtmlStylesheetLoadEventArgs args)
    {
        try
        {
            StylesheetLoad?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.CssParsing, "Failed stylesheet load event", ex);
        }
    }

    internal void RaiseHtmlImageLoadEvent(HtmlImageLoadEventArgs args)
    {
        try
        {
            ImageLoad?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.Image, "Failed image load event", ex);
        }
    }

    public void RequestRefresh(bool layout)
    {
        try
        {
            Refresh?.Invoke(this, new HtmlRefreshEventArgs(layout));
        }
        catch (Exception ex)
        {
            ReportError(HtmlRenderErrorType.General, "Failed refresh request", ex);
        }
    }

    internal void ReportError(HtmlRenderErrorType type, string message, Exception exception = null)
    {
        try
        {
            RenderError?.Invoke(this, new HtmlRenderErrorEventArgs(type, message, exception));
        }
        catch
        { }
    }

    internal void HandleLinkClicked(object parent, RPoint location, CssBox link)
    {
        EventHandler<HtmlLinkClickedEventArgs> clickHandler = LinkClicked;
        if (clickHandler != null)
        {
            var args = new HtmlLinkClickedEventArgs(link.HrefLink, link.HtmlTag.Attributes);
            try
            {
                clickHandler(this, args);
            }
            catch (Exception ex)
            {
                throw new HtmlLinkClickedException("Error in link clicked intercept", ex);
            }
            if (args.Handled)
                return;
        }

        if (string.IsNullOrEmpty(link.HrefLink))
            return;

        if (link.HrefLink.StartsWith("#") && link.HrefLink.Length > 1)
        {
            EventHandler<HtmlScrollEventArgs> scrollHandler = ScrollChange;
            if (scrollHandler != null)
            {
                var rect = GetElementRectangle(link.HrefLink.Substring(1));
                if (rect.HasValue)
                {
                    scrollHandler(this, new HtmlScrollEventArgs(rect.Value.Location));
                    HandleMouseMove(parent, location);
                }
            }
        }
        else
        {
            var nfo = new ProcessStartInfo(link.HrefLink) { UseShellExecute = true };
            Process.Start(nfo);
        }
    }

    internal void AddHoverBox(CssBox box, CssBlock block)
    {
        ArgChecker.AssertArgNotNull(box, "box");
        ArgChecker.AssertArgNotNull(block, "block");

        _hoverBoxes ??= [];
        _hoverBoxes.Add(new HoverBoxBlock(box, block));
    }

    internal ImageDownloader GetImageDownloader() => _imageDownloader;

    #region IHtmlContainerInt

    void IHtmlContainerInt.ReportError(HtmlRenderErrorType type, string message, Exception exception)
        => ReportError(type, message, exception);

    RColor IHtmlContainerInt.SelectionForeColor => SelectionForeColor;

    RColor IHtmlContainerInt.SelectionBackColor => SelectionBackColor;

    void IHtmlContainerInt.RaiseHtmlImageLoadEvent(HtmlImageLoadEventArgs args)
        => RaiseHtmlImageLoadEvent(args);

    RPoint IHtmlContainerInt.RootLocation => Root?.Location ?? RPoint.Empty;

    RFont IHtmlContainerInt.GetFont(string family, double size, RFontStyle style) => Adapter.GetFont(family, size, style);

    RColor IHtmlContainerInt.ParseColor(string colorStr) => CssParser.ParseColor(colorStr);

    RImage IHtmlContainerInt.ConvertImage(object image) => Adapter.ConvertImage(image);

    RImage IHtmlContainerInt.ImageFromStream(Stream stream) => Adapter.ImageFromStream(stream);

    RImage IHtmlContainerInt.GetLoadingImage() => Adapter.GetLoadingImage();

    RImage IHtmlContainerInt.GetLoadingFailedImage() => Adapter.GetLoadingFailedImage();

    void IHtmlContainerInt.DownloadImage(Uri uri, string filePath, bool async, Action<Uri, string, Exception, bool> callback)
        => _imageDownloader?.DownloadImage(uri, filePath, async, (imageUri, fp, error, canceled) => callback(imageUri, fp, error, canceled));

    IImageLoadHandler IHtmlContainerInt.CreateImageLoadHandler(ActionInt<RImage, RRect, bool> loadCompleteCallback)
        => new ImageLoadHandler(this, loadCompleteCallback);

    void IHtmlContainerInt.AddHoverBox(object box, CssBlock block)
        => AddHoverBox((CssBox)box, block);

    CssData IHtmlContainerInt.CssData => _cssData;

    CssData IHtmlContainerInt.DefaultCssData => Adapter.DefaultCssData;

    CssBlock IHtmlContainerInt.ParseCssBlock(string className, string blockSource)
        => CssParser.ParseCssBlock(className, blockSource);

    #endregion

    public void Dispose() => Dispose(true);


    private RPoint OffsetByScroll(RPoint location) => new(location.X - ScrollOffset.X, location.Y - ScrollOffset.Y);

    private bool IsMouseInContainer(RPoint location)
    {
        return location.X >= Location.X
            && location.X <= Location.X + ActualSize.Width
            && location.Y >= Location.Y + ScrollOffset.Y
            && location.Y <= Location.Y + ScrollOffset.Y + ActualSize.Height;
    }

    private void Dispose(bool all)
    {
        try
        {
            if (all)
            {
                LinkClicked = null;
                Refresh = null;
                RenderError = null;
                StylesheetLoad = null;
                ImageLoad = null;
            }

            _cssData = null;

            Root?.Dispose();
            Root = null;

            _selectionHandler?.Dispose();
            _selectionHandler = null;
        }
        catch
        { }
    }
}

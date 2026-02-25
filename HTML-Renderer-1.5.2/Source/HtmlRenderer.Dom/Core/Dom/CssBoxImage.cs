using System;
using System.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom;

internal sealed class CssBoxImage : CssBox
{
    private readonly CssRectImage _imageWord;
    private IImageLoadHandler _imageLoadHandler;
    private bool _imageLoadingComplete;

    public CssBoxImage(CssBox parent, HtmlTag tag) : base(parent, tag)
    {
        _imageWord = new CssRectImage(this);
        Words.Add(_imageWord);
    }

    public RImage Image => _imageWord.Image;

    protected override void PaintImp(RGraphics g)
    {
        // load image if it is in visible rectangle
        if (_imageLoadHandler == null)
        {
            _imageLoadHandler = ContainerInt.CreateImageLoadHandler(OnLoadImageComplete);
            _imageLoadHandler.LoadImage(GetAttribute("src"), HtmlTag?.Attributes);
        }

        var rect = CommonUtils.GetFirstValueOrDefault(Rectangles);
        RPoint offset = RPoint.Empty;

        if (!IsFixed)
            offset = ContainerInt.ScrollOffset;

        rect.Offset(offset);

        var clipped = RenderUtils.ClipGraphicsByOverflow(g, this);

        PaintBackground(g, rect, true, true);
        BordersDrawHandler.DrawBoxBorders(g, this, rect, true, true);

        RRect r = _imageWord.Rectangle;
        r.Offset(offset);
        r.Height -= ActualBorderTopWidth + ActualBorderBottomWidth + ActualPaddingTop + ActualPaddingBottom;
        r.Y += ActualBorderTopWidth + ActualPaddingTop;
        r.X = Math.Floor(r.X);
        r.Y = Math.Floor(r.Y);

        if (_imageWord.Image != null)
        {
            if (r.Width > 0 && r.Height > 0)
            {
                if (_imageWord.ImageRectangle == RRect.Empty)
                    g.DrawImage(_imageWord.Image, r);
                else
                    g.DrawImage(_imageWord.Image, r, _imageWord.ImageRectangle);

                if (_imageWord.Selected)
                    g.DrawRectangle(GetSelectionBackBrush(g, true), _imageWord.Left + offset.X, _imageWord.Top + offset.Y, _imageWord.Width + 2, DomUtils.GetCssLineBoxByWord(_imageWord).LineHeight);
            }
        }
        else if (_imageLoadingComplete)
        {
            if (_imageLoadingComplete && r.Width > 19 && r.Height > 19)
                RenderUtils.DrawImageErrorIcon(g, ContainerInt, r);
        }
        else
        {
            RenderUtils.DrawImageLoadingIcon(g, ContainerInt, r);
            if (r.Width > 19 && r.Height > 19)
                g.DrawRectangle(g.GetPen(System.Drawing.Color.LightGray), r.X, r.Y, r.Width, r.Height);
        }

        if (clipped)
            g.PopClip();
    }

    internal override void MeasureWordsSize(RGraphics g)
    {
        if (!_wordsSizeMeasured)
        {
            if (_imageLoadHandler == null && (ContainerInt.AvoidAsyncImagesLoading || ContainerInt.AvoidImagesLateLoading))
            {
                _imageLoadHandler = ContainerInt.CreateImageLoadHandler(OnLoadImageComplete);

                if (Content != null && Content != CssConstants.Normal)
                    _imageLoadHandler.LoadImage(Content, HtmlTag?.Attributes);
                else
                    _imageLoadHandler.LoadImage(GetAttribute("src"), HtmlTag?.Attributes);
            }

            MeasureWordSpacing(g);
            _wordsSizeMeasured = true;
        }

        CssLayoutEngine.MeasureImageSize(_imageWord);
    }

    public override void Dispose()
    {
        _imageLoadHandler?.Dispose();
        base.Dispose();
    }

    private void SetErrorBorder()
    {
        SetAllBorders(CssConstants.Solid, "2px", "#A0A0A0");
        BorderRightColor = BorderBottomColor = "#E3E3E3";
    }

    private void OnLoadImageComplete(RImage image, RRect rectangle, bool async)
    {
        _imageWord.Image = image;
        _imageWord.ImageRectangle = rectangle;
        _imageLoadingComplete = true;
        _wordsSizeMeasured = false;

        if (_imageLoadingComplete && image == null)
            SetErrorBorder();

        if (!ContainerInt.AvoidImagesLateLoading || async)
        {
            var width = new CssLength(Width);
            var height = new CssLength(Height);
            var layout = width.Number <= 0 || width.Unit != CssUnit.Pixels || height.Number <= 0 || height.Unit != CssUnit.Pixels;

            ContainerInt.RequestRefresh(layout);
        }
    }
}
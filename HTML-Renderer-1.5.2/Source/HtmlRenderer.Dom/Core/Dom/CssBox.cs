using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom;

internal class CssBox : CssBoxProperties, IDisposable
{
    private CssBox _parentBox;
    protected IHtmlContainerInt _htmlContainer;
    private ReadOnlyMemory<char> _text;

    internal bool _tableFixed;

    protected bool _wordsSizeMeasured;
    private CssBox _listItemBox;
    private IImageLoadHandler _imageLoadHandler;

    public CssBox(CssBox parentBox, HtmlTag tag)
    {
        if (parentBox != null)
        {
            _parentBox = parentBox;
            _parentBox.Boxes.Add(this);
        }

        HtmlTag = tag;
    }

    /// <summary>
    /// The container abstracted through <see cref="IHtmlContainerInt"/>. Used by
    /// CssBox and subclass code for decoupled access.
    /// </summary>
    internal IHtmlContainerInt ContainerInt
    {
        get { return _htmlContainer ??= _parentBox?.ContainerInt; }
        set { _htmlContainer = value; }
    }

    public CssBox ParentBox
    {
        get { return _parentBox; }
        set
        {
            _parentBox?.Boxes.Remove(this);
            _parentBox = value;

            if (value != null)
                _parentBox.Boxes.Add(this);
        }
    }

    public List<CssBox> Boxes { get; } = [];

    public override bool AvoidGeometryAntialias => ContainerInt?.AvoidGeometryAntialias ?? false;

    public bool IsBrElement => HtmlTag != null && HtmlTag.Name.Equals("br", StringComparison.InvariantCultureIgnoreCase);
    public bool IsInline => (Display == CssConstants.Inline || Display == CssConstants.InlineBlock) && !IsBrElement;
    public bool IsBlock => Display == CssConstants.Block;
    public virtual bool IsClickable => HtmlTag != null && HtmlTag.Name == HtmlConstants.A && !HtmlTag.HasAttribute("id");

    public virtual bool IsFixed
    {
        get
        {
            if (Position == CssConstants.Fixed)
                return true;

            if (ParentBox == null)
                return false;

            CssBox parent = this;

            while (!(parent.ParentBox == null || parent == parent.ParentBox))
            {
                parent = parent.ParentBox;

                if (parent.Position == CssConstants.Fixed)
                    return true;
            }

            return false;
        }
    }

    public virtual string HrefLink => GetAttribute(HtmlConstants.Href);

    public CssBox ContainingBlock
    {
        get
        {
            if (ParentBox == null)
                return this; //This is the initial containing block.

            var box = ParentBox;

            while (!box.IsBlock && box.Display != CssConstants.ListItem && box.Display != CssConstants.Table &&
                   box.Display != CssConstants.TableCell && box.ParentBox != null)
            {
                box = box.ParentBox;
            }

            //Comment this following line to treat always superior box as block
            if (box == null)
                throw new Exception("There's no containing block on the chain");

            return box;
        }
    }

    public HtmlTag HtmlTag { get; }

    public bool IsImage => Words.Count == 1 && Words[0].IsImage;

    public bool IsSpaceOrEmpty
    {
        get
        {
            if ((Words.Count != 0 || Boxes.Count != 0) && (Words.Count != 1 || !Words[0].IsSpaces))
            {
                foreach (CssRect word in Words)
                {
                    if (!word.IsSpaces)
                        return false;
                }
            }

            return true;
        }
    }

    public ReadOnlyMemory<char> Text
    {
        get { return _text; }
        set
        {
            _text = value;
            Words.Clear();
        }
    }

    internal List<CssLineBox> LineBoxes { get; } = [];
    internal List<CssLineBox> ParentLineBoxes { get; } = [];
    internal Dictionary<CssLineBox, RectangleF> Rectangles { get; } = [];
    internal List<CssRect> Words { get; } = [];
    internal CssRect FirstWord => Words[0];

    internal CssLineBox FirstHostingLineBox { get; set; }

    internal CssLineBox LastHostingLineBox { get; set; }

    public void PerformLayout(RGraphics g)
    {
        try
        {
            PerformLayoutImp(g);
        }
        catch (Exception ex)
        {
            ContainerInt.ReportError(HtmlRenderErrorType.Layout, "Exception in box layout", ex);
        }
    }

    public void Paint(RGraphics g)
    {
        try
        {
            if (Display != CssConstants.None && Visibility == CssConstants.Visible)
            {
                // use initial clip to draw blocks with Position = fixed. I.e. ignrore page margins
                if (Position == CssConstants.Fixed)
                    g.SuspendClipping();

                // don't call paint if the rectangle of the box is not in visible rectangle
                bool visible = Rectangles.Count == 0;

                if (!visible)
                {
                    var clip = g.GetClip();
                    var rect = ContainingBlock.ClientRectangle;

                    rect.X -= 2;
                    rect.Width += 2;

                    if (!IsFixed)
                    {
                        //rect.Offset(new PointF(-HtmlContainer.Location.X, -HtmlContainer.Location.Y));
                        rect.Offset(ContainerInt.ScrollOffset);
                    }

                    clip.Intersect(rect);

                    if (clip != RectangleF.Empty)
                        visible = true;
                }

                if (visible)
                    PaintImp(g);

                if (Position == CssConstants.Fixed)
                {
                    g.ResumeClipping();
                }
            }
        }
        catch (Exception ex)
        {
            ContainerInt.ReportError(HtmlRenderErrorType.Paint, "Exception in box paint", ex);
        }
    }

    public void SetBeforeBox(CssBox before)
    {
        int index = _parentBox.Boxes.IndexOf(before);

        if (index < 0)
            throw new Exception("before box doesn't exist on parent");

        _parentBox.Boxes.Remove(this);
        _parentBox.Boxes.Insert(index, this);
    }

    public void SetAllBoxes(CssBox fromBox)
    {
        foreach (var childBox in fromBox.Boxes)
            childBox._parentBox = this;

        Boxes.AddRange(fromBox.Boxes);
        fromBox.Boxes.Clear();
    }

    public void ParseToWords()
    {
        Words.Clear();

        int startIdx = 0;
        bool preserveSpaces = WhiteSpace == CssConstants.Pre || WhiteSpace == CssConstants.PreWrap;
        bool respoctNewline = preserveSpaces || WhiteSpace == CssConstants.PreLine;

        var textSpan = _text.Span;
        while (startIdx < textSpan.Length)
        {
            while (startIdx < textSpan.Length && textSpan[startIdx] == '\r')
                startIdx++;

            if (startIdx < textSpan.Length)
            {
                var endIdx = startIdx;

                while (endIdx < textSpan.Length && char.IsWhiteSpace(textSpan[endIdx]) && textSpan[endIdx] != '\n')
                    endIdx++;

                if (endIdx > startIdx)
                {
                    if (preserveSpaces)
                        Words.Add(new CssRectWord(this, HtmlUtils.DecodeHtml(_text.Slice(startIdx, endIdx - startIdx).ToString()), false, false));
                }
                else
                {
                    endIdx = startIdx;

                    while (endIdx < textSpan.Length && !char.IsWhiteSpace(textSpan[endIdx]) && textSpan[endIdx] != '-' && WordBreak != CssConstants.BreakAll && !CommonUtils.IsAsianCharecter(textSpan[endIdx]))
                        endIdx++;

                    if (endIdx < textSpan.Length && (textSpan[endIdx] == '-' || WordBreak == CssConstants.BreakAll || CommonUtils.IsAsianCharecter(textSpan[endIdx])))
                        endIdx++;

                    if (endIdx > startIdx)
                    {
                        var hasSpaceBefore = !preserveSpaces && startIdx > 0 && Words.Count == 0 && char.IsWhiteSpace(textSpan[startIdx - 1]);
                        var hasSpaceAfter = !preserveSpaces && endIdx < textSpan.Length && char.IsWhiteSpace(textSpan[endIdx]);

                        Words.Add(new CssRectWord(this, HtmlUtils.DecodeHtml(_text.Slice(startIdx, endIdx - startIdx).ToString()), hasSpaceBefore, hasSpaceAfter));
                    }
                }

                // create new-line word so it will effect the layout
                if (endIdx < textSpan.Length && textSpan[endIdx] == '\n')
                {
                    endIdx++;

                    if (respoctNewline)
                        Words.Add(new CssRectWord(this, "\n", false, false));
                }

                startIdx = endIdx;
            }
        }
    }

    public virtual void Dispose()
    {
        _imageLoadHandler?.Dispose();

        foreach (var childBox in Boxes)
            childBox.Dispose();
    }

    protected virtual void PerformLayoutImp(RGraphics g)
    {
        if (Display != CssConstants.None)
        {
            RectanglesReset();
            MeasureWordsSize(g);
        }

        if (IsBlock || Display == CssConstants.ListItem || Display == CssConstants.Table || Display == CssConstants.InlineTable || Display == CssConstants.TableCell)
        {
            // Because their width and height are set by CssTable
            if (Display != CssConstants.TableCell && Display != CssConstants.Table)
            {
                double width = ContainingBlock.Size.Width
                               - ContainingBlock.ActualPaddingLeft - ContainingBlock.ActualPaddingRight
                               - ContainingBlock.ActualBorderLeftWidth - ContainingBlock.ActualBorderRightWidth;

                if (Width != CssConstants.Auto && !string.IsNullOrEmpty(Width))
                {
                    double containingWidth = width;
                    width = CssValueParser.ParseLength(Width, containingWidth, GetEmHeight());

                    // Apply max-width constraint before adding padding/border
                    if (MaxWidth != "none" && !string.IsNullOrEmpty(MaxWidth))
                    {
                        double maxW = CssValueParser.ParseLength(MaxWidth, containingWidth, GetEmHeight());
                        if (width > maxW) width = maxW;
                    }

                    width += ActualPaddingLeft + ActualPaddingRight + ActualBorderLeftWidth + ActualBorderRightWidth;
                }

                Size = new SizeF((float)width, Size.Height);

                // Margins reduce the box width only for auto-width elements.
                // For explicit widths, margins affect position only (CSS1 box model).
                if (Width == CssConstants.Auto || string.IsNullOrEmpty(Width))
                {
                    Size = new SizeF((float)(width - ActualMarginLeft - ActualMarginRight), Size.Height);
                }
            }

            if (Display != CssConstants.TableCell)
            {
                var prevSibling = DomUtils.GetPreviousSibling(this);

                if (Position != CssConstants.Fixed)
                {
                    double left = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ActualMarginLeft + ContainingBlock.ActualBorderLeftWidth;
                    double top = (prevSibling == null && ParentBox != null ? ParentBox.ClientTop : ParentBox == null ? Location.Y : 0) + MarginTopCollapse(prevSibling) + (prevSibling != null ? prevSibling.ActualBottom + prevSibling.ActualBorderBottomWidth : 0);

                    // --- Float positioning ---
                    if (Float != CssConstants.None)
                    {
                        // Align Y with previous float sibling if consecutive
                        if (prevSibling != null && prevSibling.Float != CssConstants.None)
                            top = prevSibling.Location.Y;

                        double containerLeft = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ContainingBlock.ActualBorderLeftWidth;
                        double containerRight = ContainingBlock.ClientLeft + ContainingBlock.AvailableWidth;
                        double floatHeight = Math.Max(ActualHeight, 1);

                        if (Float == CssConstants.Left)
                        {
                            // Iteratively resolve collisions with all prior left floats
                            for (int iter = 0; iter < 100; iter++)
                            {
                                left = containerLeft + ActualMarginLeft;

                                if (ParentBox != null)
                                {
                                    foreach (var sibling in ParentBox.Boxes)
                                    {
                                        if (sibling == this) break;
                                        if (sibling.Float == CssConstants.Left && sibling.Display != CssConstants.None)
                                        {
                                            double fBottom = sibling.ActualBottom + sibling.ActualBorderBottomWidth;
                                            if (top < fBottom && top + floatHeight > sibling.Location.Y)
                                                left = Math.Max(left, sibling.Location.X + sibling.Size.Width + ActualMarginLeft);
                                        }
                                    }
                                }

                                if (left + Size.Width <= containerRight)
                                    break;

                                // Move below the lowest overlapping float
                                double maxBottom = top;
                                if (ParentBox != null)
                                {
                                    foreach (var sibling in ParentBox.Boxes)
                                    {
                                        if (sibling == this) break;
                                        if (sibling.Float != CssConstants.None && sibling.Display != CssConstants.None)
                                        {
                                            double fBottom = sibling.ActualBottom + sibling.ActualBorderBottomWidth;
                                            if (top < fBottom && top + floatHeight > sibling.Location.Y)
                                                maxBottom = Math.Max(maxBottom, fBottom);
                                        }
                                    }
                                }

                                if (maxBottom <= top) break;
                                top = maxBottom;
                            }
                        }
                        else if (Float == CssConstants.Right)
                        {
                            left = containerRight - Size.Width - ActualMarginRight;
                        }
                    }

                    // Handle clear property
                    if (Clear != CssConstants.None && prevSibling != null)
                    {
                        double maxFloatBottom = CssBoxHelper.GetMaxFloatBottom(this);
                        if (maxFloatBottom > top)
                            top = maxFloatBottom;
                    }

                    Location = new PointF((float)left, (float)top);
                    ActualBottom = top;
                }
            }

            //If we're talking about a table here..
            if (Display == CssConstants.Table || Display == CssConstants.InlineTable)
            {
                CssLayoutEngineTable.PerformLayout(g, this);
            }
            else
            {
                //If there's just inline boxes, create LineBoxes
                if (DomUtils.ContainsInlinesOnly(this))
                {
                    ActualBottom = Location.Y;
                    CssLayoutEngine.CreateLineBoxes(g, this); //This will automatically set the bottom of this block
                }
                else if (Boxes.Count > 0)
                {
                    foreach (var childBox in Boxes)
                        childBox.PerformLayout(g);

                    ActualRight = CalculateActualRight();
                    ActualBottom = MarginBottomCollapse();
                }
            }
        }
        else
        {
            var prevSibling = DomUtils.GetPreviousSibling(this);
            if (prevSibling != null)
            {
                if (Location == PointF.Empty)
                    Location = prevSibling.Location;

                ActualBottom = prevSibling.ActualBottom;
            }
        }

        ActualBottom = Math.Max(ActualBottom, Location.Y + ActualHeight);

        // Floats with an explicit CSS height establish a new BFC.
        // Their ActualBottom should reflect the stated height, not
        // content overflow from child floats (CSS2.1 §10.6.1).
        if (Float != CssConstants.None && Height != CssConstants.Auto && !string.IsNullOrEmpty(Height))
            ActualBottom = Location.Y + ActualHeight;

        // Apply position:relative offset after layout (visual only, does not affect flow)
        if (Position == CssConstants.Relative)
        {
            double dx = 0, dy = 0;
            if (Left != null && Left != CssConstants.Auto)
                dx = CssValueParser.ParseLength(Left, Size.Width, GetEmHeight());
            if (Top != null && Top != CssConstants.Auto)
                dy = CssValueParser.ParseLength(Top, Size.Height, GetEmHeight());

            if (dx != 0)
                OffsetLeft(dx);
            if (dy != 0)
                OffsetTop(dy);
        }

        CreateListItemBox(g);

        if (!IsFixed)
        {
            var actualWidth = Math.Max(GetMinimumWidth() + CssBoxHelper.GetWidthMarginDeep(this), Size.Width < 90999 ? ActualRight - ContainerInt.RootLocation.X : 0);
            ContainerInt.ActualSize = CommonUtils.Max(ContainerInt.ActualSize, new SizeF((float)actualWidth, (float)(ActualBottom - ContainerInt.RootLocation.Y)));
        }
    }

    internal virtual void MeasureWordsSize(RGraphics g)
    {
        if (_wordsSizeMeasured)
            return;

        if (BackgroundImage != CssConstants.None && _imageLoadHandler == null)
        {
            _imageLoadHandler = ContainerInt.CreateImageLoadHandler(OnImageLoadComplete);
            _imageLoadHandler.LoadImage(BackgroundImage, HtmlTag != null ? HtmlTag.Attributes : null);
        }

        MeasureWordSpacing(g);

        if (Words.Count > 0)
        {
            foreach (var boxWord in Words)
            {
                boxWord.Width = boxWord.Text != "\n" ? g.MeasureString(boxWord.Text, ActualFont).Width : 0;
                boxWord.Height = ActualFont.Height;
            }
        }

        _wordsSizeMeasured = true;
    }

    protected override sealed CssBoxProperties GetParent() => _parentBox;

    private int GetIndexForList()
    {
        bool reversed = !string.IsNullOrEmpty(ParentBox.GetAttribute("reversed"));

        if (!int.TryParse(ParentBox.GetAttribute("start"), out int index))
        {
            if (reversed)
            {
                index = 0;
                foreach (CssBox b in ParentBox.Boxes)
                {
                    if (b.Display == CssConstants.ListItem)
                        index++;
                }
            }
            else
            {
                index = 1;
            }
        }

        foreach (CssBox b in ParentBox.Boxes)
        {
            if (b.Equals(this))
                return index;

            if (b.Display == CssConstants.ListItem)
                index += reversed ? -1 : 1;
        }

        return index;
    }

    private void CreateListItemBox(RGraphics g)
    {
        if (Display != CssConstants.ListItem || ListStyleType == CssConstants.None)
            return;

        if (_listItemBox == null)
        {
            _listItemBox = new CssBox(null, null);
            _listItemBox.InheritStyle(this);
            _listItemBox.Display = CssConstants.Inline;
            _listItemBox._htmlContainer = ContainerInt;

            if (ListStyleType.Equals(CssConstants.Disc, StringComparison.InvariantCultureIgnoreCase))
            {
                _listItemBox.Text = "•".AsMemory();
            }
            else if (ListStyleType.Equals(CssConstants.Circle, StringComparison.InvariantCultureIgnoreCase))
            {
                _listItemBox.Text = "o".AsMemory();
            }
            else if (ListStyleType.Equals(CssConstants.Square, StringComparison.InvariantCultureIgnoreCase))
            {
                _listItemBox.Text = "♠".AsMemory();
            }
            else if (ListStyleType.Equals(CssConstants.Decimal, StringComparison.InvariantCultureIgnoreCase))
            {
                _listItemBox.Text = (GetIndexForList().ToString(CultureInfo.InvariantCulture) + ".").AsMemory();
            }
            else if (ListStyleType.Equals(CssConstants.DecimalLeadingZero, StringComparison.InvariantCultureIgnoreCase))
            {
                _listItemBox.Text = (GetIndexForList().ToString("00", CultureInfo.InvariantCulture) + ".").AsMemory();
            }
            else
            {
                _listItemBox.Text = (CommonUtils.ConvertToAlphaNumber(GetIndexForList(), ListStyleType) + ".").AsMemory();
            }

            _listItemBox.ParseToWords();

            _listItemBox.PerformLayoutImp(g);
            _listItemBox.Size = new SizeF((float)_listItemBox.Words[0].Width, (float)_listItemBox.Words[0].Height);
        }

        _listItemBox.Words[0].Left = Location.X - _listItemBox.Size.Width - 5;
        _listItemBox.Words[0].Top = Location.Y + ActualPaddingTop; // +FontAscent;
    }

    internal string GetAttribute(string attribute) => GetAttribute(attribute, string.Empty);
    internal string GetAttribute(string attribute, string defaultValue) => HtmlTag != null ? HtmlTag.TryGetAttribute(attribute, defaultValue) : defaultValue;

    internal double GetMinimumWidth()
    {
        double maxWidth = 0;
        CssRect maxWidthWord = null;
        CssBoxHelper.GetMinimumWidth_LongestWord(this, ref maxWidth, ref maxWidthWord);

        double padding = 0f;
        if (maxWidthWord != null)
        {
            var box = maxWidthWord.OwnerBox;
            while (box != null)
            {
                padding += box.ActualBorderRightWidth + box.ActualPaddingRight + box.ActualBorderLeftWidth + box.ActualPaddingLeft;
                box = box != this ? box.ParentBox : null;
            }
        }

        return maxWidth + padding;
    }

    internal void GetMinMaxWidth(out double minWidth, out double maxWidth)
    {
        double min = 0f;
        double maxSum = 0f;
        double paddingSum = 0f;
        double marginSum = 0f;

        CssBoxHelper.GetMinMaxSumWords(this, ref min, ref maxSum, ref paddingSum, ref marginSum);

        maxWidth = paddingSum + maxSum;
        minWidth = paddingSum + (min < 90999 ? min : 0);
    }

    internal bool HasJustInlineSiblings() => ParentBox != null && DomUtils.ContainsInlinesOnly(ParentBox);

    internal new void InheritStyle(CssBox box = null, bool everything = false) => base.InheritStyle(box ?? ParentBox, everything);

    protected double MarginTopCollapse(CssBoxProperties prevSibling)
    {
        double value;

        if (prevSibling != null)
        {
            value = Math.Max(prevSibling.ActualMarginBottom, ActualMarginTop);
            CollapsedMarginTop = value;
        }
        else if (_parentBox != null && ActualPaddingTop < 0.1 && ActualPaddingBottom < 0.1 && _parentBox.ActualPaddingTop < 0.1 && _parentBox.ActualPaddingBottom < 0.1)
        {
            value = Math.Max(0, ActualMarginTop - Math.Max(_parentBox.ActualMarginTop, _parentBox.CollapsedMarginTop));
        }
        else
        {
            value = ActualMarginTop;
        }

        // fix for hr tag
        if (value < 0.1 && HtmlTag != null && HtmlTag.Name == "hr")
            value = GetEmHeight() * 1.1f;

        return value;
    }

    public bool BreakPage()
    {
        var container = ContainerInt;

        if (Size.Height >= container.PageSize.Height)
            return false;

        var remTop = (Location.Y - container.MarginTop) % container.PageSize.Height;
        var remBottom = (ActualBottom - container.MarginTop) % container.PageSize.Height;

        if (remTop > remBottom)
        {
            var diff = container.PageSize.Height - remTop;
            Location = new PointF(Location.X, (float)(Location.Y + diff + 1));
            
            return true;
        }

        return false;
    }

    private double CalculateActualRight()
    {
        if (ActualRight <= 90999)
            return ActualRight;

        var maxRight = 0d;

        foreach (var box in Boxes)
            maxRight = Math.Max(maxRight, box.ActualRight + box.ActualMarginRight);

        return maxRight + ActualPaddingRight + ActualMarginRight + ActualBorderRightWidth;
    }

    private double MarginBottomCollapse()
    {
        double margin = 0;

        if (ParentBox != null && ParentBox.Boxes.IndexOf(this) == ParentBox.Boxes.Count - 1 && _parentBox.ActualMarginBottom < 0.1)
        {
            var lastChildBottomMargin = Boxes[Boxes.Count - 1].ActualMarginBottom;
            margin = Height == "auto" ? Math.Max(ActualMarginBottom, lastChildBottomMargin) : lastChildBottomMargin;
        }

        // Use the maximum ActualBottom across all children to handle
        // floated children that may not be the last in source order.
        double maxChildBottom = 0;
        
        foreach (var child in Boxes)
            maxChildBottom = Math.Max(maxChildBottom, child.ActualBottom + child.ActualBorderBottomWidth);

        return Math.Max(ActualBottom, maxChildBottom + margin + ActualPaddingBottom + ActualBorderBottomWidth);
    }

    internal void OffsetTop(double amount)
    {
        List<CssLineBox> lines = [.. Rectangles.Keys];

        foreach (CssLineBox line in lines)
        {
            RectangleF r = Rectangles[line];
            Rectangles[line] = new RectangleF(r.X, (float)(r.Y + amount), r.Width, r.Height);
        }

        foreach (CssRect word in Words)
            word.Top += amount;

        foreach (CssBox b in Boxes)
            b.OffsetTop(amount);

        _listItemBox?.OffsetTop(amount);

        Location = new PointF(Location.X, (float)(Location.Y + amount));
    }

    internal void OffsetLeft(double amount)
    {
        List<CssLineBox> lines = [.. Rectangles.Keys];

        foreach (CssLineBox line in lines)
        {
            RectangleF r = Rectangles[line];
            Rectangles[line] = new RectangleF((float)(r.X + amount), r.Y, r.Width, r.Height);
        }

        foreach (CssRect word in Words)
            word.Left += amount;

        foreach (CssBox b in Boxes)
            b.OffsetLeft(amount);

        _listItemBox?.OffsetLeft(amount);

        Location = new PointF((float)(Location.X + amount), Location.Y);
    }

    protected virtual void PaintImp(RGraphics g)
    {
        if (Display == CssConstants.None || Display == CssConstants.TableCell && EmptyCells == CssConstants.Hide && IsSpaceOrEmpty)
            return;

        var clipped = RenderUtils.ClipGraphicsByOverflow(g, this);

        var areas = Rectangles.Count == 0 ? [Bounds] : new List<RectangleF>(Rectangles.Values);
        var clip = g.GetClip();
        RectangleF[] rects = areas.ToArray();
        PointF offset = PointF.Empty;

        if (!IsFixed)
            offset = ContainerInt.ScrollOffset;

        for (int i = 0; i < rects.Length; i++)
        {
            var actualRect = rects[i];
            actualRect.Offset(offset);

            if (CssBoxHelper.IsRectVisible(actualRect, clip))
            {
                PaintBackground(g, actualRect, i == 0, i == rects.Length - 1);
                BordersDrawHandler.DrawBoxBorders(g, this, actualRect, i == 0, i == rects.Length - 1);
            }
        }

        PaintWords(g, offset);

        for (int i = 0; i < rects.Length; i++)
        {
            var actualRect = rects[i];
            actualRect.Offset(offset);

            if (CssBoxHelper.IsRectVisible(actualRect, clip))
                PaintDecoration(g, actualRect, i == 0, i == rects.Length - 1);
        }

        // split paint to handle z-order
        foreach (CssBox b in Boxes)
        {
            if (b.Position != CssConstants.Absolute && !b.IsFixed)
                b.Paint(g);
        }

        foreach (CssBox b in Boxes)
        {
            if (b.Position == CssConstants.Absolute)
                b.Paint(g);
        }

        foreach (CssBox b in Boxes)
        {
            if (b.IsFixed)
                b.Paint(g);
        }

        if (clipped)
            g.PopClip();

        _listItemBox?.Paint(g);
    }

    protected void PaintBackground(RGraphics g, RectangleF rect, bool isFirst, bool isLast)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        RBrush brush = null;

        if (BackgroundGradient != CssConstants.None)
        {
            brush = g.GetLinearGradientBrush(rect, ActualBackgroundColor, ActualBackgroundGradient, ActualBackgroundGradientAngle);
        }
        else if (RenderUtils.IsColorVisible(ActualBackgroundColor))
        {
            brush = g.GetSolidBrush(ActualBackgroundColor);
        }

        if (brush != null)
        {
            // TODO:a handle it correctly (tables background)
            // if (isLast)
            //  rectangle.Width -= ActualWordSpacing + CssUtils.GetWordEndWhitespace(ActualFont);

            RGraphicsPath roundrect = null;
            if (IsRounded)
                roundrect = RenderUtils.GetRoundRect(g, rect, ActualCornerNw, ActualCornerNe, ActualCornerSe, ActualCornerSw);

            Object prevMode = null;
            if (ContainerInt != null && !ContainerInt.AvoidGeometryAntialias && IsRounded)
                prevMode = g.SetAntiAliasSmoothingMode();

            if (roundrect != null)
            {
                g.DrawPath(brush, roundrect);
            }
            else
            {
                g.DrawRectangle(brush, Math.Ceiling(rect.X), Math.Ceiling(rect.Y), rect.Width, rect.Height);
            }

            g.ReturnPreviousSmoothingMode(prevMode);

            roundrect?.Dispose();
            brush.Dispose();
        }

        if (_imageLoadHandler != null && _imageLoadHandler.Image != null && isFirst)
            BackgroundImageDrawHandler.DrawBackgroundImage(g, this, _imageLoadHandler, rect);
    }

    private void PaintWords(RGraphics g, PointF offset)
    {
        if (Width.Length == 0)
            return;

        var isRtl = Direction == CssConstants.Rtl;

        foreach (var word in Words)
        {
            if (word.IsLineBreak)
                continue;

            var clip = g.GetClip();
            var wordRect = word.Rectangle;

            wordRect.Offset(offset);
            clip.Intersect(wordRect);

            if (clip == RectangleF.Empty)
                continue;

            var wordPoint = new PointF((float)(word.Left + offset.X), (float)(word.Top + offset.Y));

            if (word.Selected)
            {
                // handle paint selected word background and with partial word selection
                var wordLine = DomUtils.GetCssLineBoxByWord(word);
                var left = word.SelectedStartOffset > -1 ? word.SelectedStartOffset : (wordLine.Words[0] != word && word.HasSpaceBefore ? -ActualWordSpacing : 0);
                var padWordRight = word.HasSpaceAfter && !wordLine.IsLastSelectedWord(word);
                var width = word.SelectedEndOffset > -1 ? word.SelectedEndOffset : word.Width + (padWordRight ? ActualWordSpacing : 0);
                var rect = new RectangleF((float)(word.Left + offset.X + left), (float)(word.Top + offset.Y), (float)(width - left), (float)wordLine.LineHeight);

                g.DrawRectangle(GetSelectionBackBrush(g, false), rect.X, rect.Y, rect.Width, rect.Height);

                if (ContainerInt.SelectionForeColor != System.Drawing.Color.Empty && (word.SelectedStartOffset > 0 || word.SelectedEndIndexOffset > -1))
                {
                    g.PushClipExclude(rect);
                    g.DrawString(word.Text, ActualFont, ActualColor, wordPoint, new SizeF((float)word.Width, (float)word.Height), isRtl);
                    g.PopClip();
                    g.PushClip(rect);
                    g.DrawString(word.Text, ActualFont, GetSelectionForeBrush(), wordPoint, new SizeF((float)word.Width, (float)word.Height), isRtl);
                    g.PopClip();
                }
                else
                {
                    g.DrawString(word.Text, ActualFont, GetSelectionForeBrush(), wordPoint, new SizeF((float)word.Width, (float)word.Height), isRtl);
                }
            }
            else
            {
                //                            g.DrawRectangle(HtmlContainer.Adapter.GetPen(Color.Black), wordPoint.X, wordPoint.Y, word.Width - 1, word.Height - 1);
                g.DrawString(word.Text, ActualFont, ActualColor, wordPoint, new SizeF((float)word.Width, (float)word.Height), isRtl);
            }
        }
    }

    protected void PaintDecoration(RGraphics g, RectangleF rectangle, bool isFirst, bool isLast)
    {
        if (string.IsNullOrEmpty(TextDecoration) || TextDecoration == CssConstants.None)
            return;

        double y = 0f;
        if (TextDecoration == CssConstants.Underline)
        {
            y = Math.Round(rectangle.Top + ActualFont.UnderlineOffset);
        }
        else if (TextDecoration == CssConstants.LineThrough)
        {
            y = rectangle.Top + rectangle.Height / 2f;
        }
        else if (TextDecoration == CssConstants.Overline)
        {
            y = rectangle.Top;
        }
        y -= ActualPaddingBottom - ActualBorderBottomWidth;

        double x1 = rectangle.X;
        if (isFirst)
            x1 += ActualPaddingLeft + ActualBorderLeftWidth;

        double x2 = rectangle.Right;
        if (isLast)
            x2 -= ActualPaddingRight + ActualBorderRightWidth;

        var pen = g.GetPen(ActualColor);
        pen.Width = 1;
        pen.DashStyle = DashStyle.Solid;
        g.DrawLine(pen, x1, y, x2, y);
    }

    internal void OffsetRectangle(CssLineBox lineBox, double gap)
    {
        if (Rectangles.TryGetValue(lineBox, out RectangleF r))
            Rectangles[lineBox] = new RectangleF(r.X, (float)(r.Y + gap), r.Width, r.Height);
    }

    internal void RectanglesReset() => Rectangles.Clear();

    private void OnImageLoadComplete(RImage image, RectangleF rectangle, bool async)
    {
        if (image != null && async)
            ContainerInt.RequestRefresh(false);
    }

    protected Color GetSelectionForeBrush() => ContainerInt.SelectionForeColor != System.Drawing.Color.Empty ? ContainerInt.SelectionForeColor : ActualColor;

    protected RBrush GetSelectionBackBrush(RGraphics g, bool forceAlpha)
    {
        var backColor = ContainerInt.SelectionBackColor;
        if (backColor != System.Drawing.Color.Empty)
        {
            if (forceAlpha && backColor.A > 180)
                return g.GetSolidBrush(System.Drawing.Color.FromArgb(180, backColor.R, backColor.G, backColor.B));
            else
                return g.GetSolidBrush(backColor);
        }
        else
        {
            return g.GetSolidBrush(CssUtils.DefaultSelectionBackcolor);
        }
    }

    protected override RFont GetCachedFont(string fontFamily, double fsize, FontStyle st) => ContainerInt.GetFont(fontFamily, fsize, st);

    protected override Color GetActualColor(string colorStr) => ContainerInt.ParseColor(colorStr);

    protected override PointF GetActualLocation(string X, string Y)
    {
        var left = CssValueParser.ParseLength(X, ContainerInt.PageSize.Width, GetEmHeight(), null);
        var top = CssValueParser.ParseLength(Y, ContainerInt.PageSize.Height, GetEmHeight(), null);

        return new PointF((float)left, (float)top);
    }

    public override string ToString()
    {
        var tag = HtmlTag != null ? $"<{HtmlTag.Name}>" : "anon";

        if (IsBlock)
        {
            return $"{(ParentBox == null ? "Root: " : string.Empty)}{tag} Block {FontSize}, Children:{Boxes.Count}";
        }
        else if (Display == CssConstants.None)
        {
            return $"{(ParentBox == null ? "Root: " : string.Empty)}{tag} None";
        }
        else
        {
            return $"{(ParentBox == null ? "Root: " : string.Empty)}{tag} {Display}: {Text}";
        }
    }
}
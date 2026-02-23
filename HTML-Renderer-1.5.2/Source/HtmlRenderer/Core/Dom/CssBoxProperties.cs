using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom;

internal abstract class CssBoxProperties
{
    #region CSS Fields

    private string _borderTopWidth = "medium";
    private string _borderRightWidth = "medium";
    private string _borderBottomWidth = "medium";
    private string _borderLeftWidth = "medium";
    private string _borderTopColor = "black";
    private string _borderRightColor = "black";
    private string _borderBottomColor = "black";
    private string _borderLeftColor = "black";
    private string _bottom;
    private string _color = "black";
    private string _cornerRadius = "0";
    private string _fontSize = "medium";
    private string _left = "auto";
    private string _lineHeight = "normal";
    private string _paddingLeft = "0";
    private string _paddingBottom = "0";
    private string _paddingRight = "0";
    private string _paddingTop = "0";
    private string _right;
    private string _textIndent = "0";
    private string _top = "auto";
    private string _wordSpacing = "normal";

    #endregion


    #region Fields

    /// <summary>
    /// Gets or sets the location of the box
    /// </summary>
    private RPoint _location;

    /// <summary>
    /// Gets or sets the size of the box
    /// </summary>
    private RSize _size;

    private double _actualCornerNw = double.NaN;
    private double _actualCornerNe = double.NaN;
    private double _actualCornerSw = double.NaN;
    private double _actualCornerSe = double.NaN;
    private RColor _actualColor = RColor.Empty;
    private double _actualBackgroundGradientAngle = double.NaN;
    private double _actualHeight = double.NaN;
    private double _actualWidth = double.NaN;
    private double _actualPaddingTop = double.NaN;
    private double _actualPaddingBottom = double.NaN;
    private double _actualPaddingRight = double.NaN;
    private double _actualPaddingLeft = double.NaN;
    private double _actualMarginTop = double.NaN;
    private double _collapsedMarginTop = double.NaN;
    private double _actualMarginBottom = double.NaN;
    private double _actualMarginRight = double.NaN;
    private double _actualMarginLeft = double.NaN;
    private double _actualBorderTopWidth = double.NaN;
    private double _actualBorderLeftWidth = double.NaN;
    private double _actualBorderBottomWidth = double.NaN;
    private double _actualBorderRightWidth = double.NaN;

    /// <summary>
    /// the width of whitespace between words
    /// </summary>
    private double _actualLineHeight = double.NaN;
    private double _actualTextIndent = double.NaN;
    private double _actualBorderSpacingHorizontal = double.NaN;
    private double _actualBorderSpacingVertical = double.NaN;
    private RColor _actualBackgroundGradient = RColor.Empty;
    private RColor _actualBorderTopColor = RColor.Empty;
    private RColor _actualBorderLeftColor = RColor.Empty;
    private RColor _actualBorderBottomColor = RColor.Empty;
    private RColor _actualBorderRightColor = RColor.Empty;
    private RColor _actualBackgroundColor = RColor.Empty;
    private RFont _actualFont;

    #endregion


    #region CSS Properties

    public string BorderBottomWidth
    {
        get { return _borderBottomWidth; }
        set
        {
            _borderBottomWidth = value;
            _actualBorderBottomWidth = Single.NaN;
        }
    }

    public string BorderLeftWidth
    {
        get { return _borderLeftWidth; }
        set
        {
            _borderLeftWidth = value;
            _actualBorderLeftWidth = Single.NaN;
        }
    }

    public string BorderRightWidth
    {
        get { return _borderRightWidth; }
        set
        {
            _borderRightWidth = value;
            _actualBorderRightWidth = Single.NaN;
        }
    }

    public string BorderTopWidth
    {
        get { return _borderTopWidth; }
        set
        {
            _borderTopWidth = value;
            _actualBorderTopWidth = Single.NaN;
        }
    }

    public string BorderBottomStyle { get; set; } = "none";
    public string BorderLeftStyle { get; set; } = "none";
    public string BorderRightStyle { get; set; } = "none";
    public string BorderTopStyle { get; set; } = "none";

    public string BorderBottomColor
    {
        get { return _borderBottomColor; }
        set
        {
            _borderBottomColor = value;
            _actualBorderBottomColor = RColor.Empty;
        }
    }

    public string BorderLeftColor
    {
        get { return _borderLeftColor; }
        set
        {
            _borderLeftColor = value;
            _actualBorderLeftColor = RColor.Empty;
        }
    }

    public string BorderRightColor
    {
        get { return _borderRightColor; }
        set
        {
            _borderRightColor = value;
            _actualBorderRightColor = RColor.Empty;
        }
    }

    public string BorderTopColor
    {
        get { return _borderTopColor; }
        set
        {
            _borderTopColor = value;
            _actualBorderTopColor = RColor.Empty;
        }
    }

    public string BorderSpacing { get; set; } = "0";
    public string BorderCollapse { get; set; } = "separate";

    public string CornerRadius
    {
        get { return _cornerRadius; }
        set
        {
            MatchCollection r = RegexParserUtils.Match(RegexParserUtils.CssLength, value);

            switch (r.Count)
            {
                case 1:
                    CornerNeRadius = r[0].Value;
                    CornerNwRadius = r[0].Value;
                    CornerSeRadius = r[0].Value;
                    CornerSwRadius = r[0].Value;
                    break;
                case 2:
                    CornerNeRadius = r[0].Value;
                    CornerNwRadius = r[0].Value;
                    CornerSeRadius = r[1].Value;
                    CornerSwRadius = r[1].Value;
                    break;
                case 3:
                    CornerNeRadius = r[0].Value;
                    CornerNwRadius = r[1].Value;
                    CornerSeRadius = r[2].Value;
                    break;
                case 4:
                    CornerNeRadius = r[0].Value;
                    CornerNwRadius = r[1].Value;
                    CornerSeRadius = r[2].Value;
                    CornerSwRadius = r[3].Value;
                    break;
            }

            _cornerRadius = value;
        }
    }

    public string CornerNwRadius { get; set; } = "0";
    public string CornerNeRadius { get; set; } = "0";
    public string CornerSeRadius { get; set; } = "0";
    public string CornerSwRadius { get; set; } = "0";
    public string MarginBottom { get; set; } = "0";
    public string MarginLeft { get; set; } = "0";
    public string MarginRight { get; set; } = "0";
    public string MarginTop { get; set; } = "0";

    public string PaddingBottom
    {
        get { return _paddingBottom; }
        set
        {
            _paddingBottom = value;
            _actualPaddingBottom = double.NaN;
        }
    }

    public string PaddingLeft
    {
        get { return _paddingLeft; }
        set
        {
            _paddingLeft = value;
            _actualPaddingLeft = double.NaN;
        }
    }

    public string PaddingRight
    {
        get { return _paddingRight; }
        set
        {
            _paddingRight = value;
            _actualPaddingRight = double.NaN;
        }
    }

    public string PaddingTop
    {
        get { return _paddingTop; }
        set
        {
            _paddingTop = value;
            _actualPaddingTop = double.NaN;
        }
    }

    public string PageBreakInside { get; set; } = CssConstants.Auto;

    public string Left
    {
        get { return _left; }
        set
        {
            _left = value;

            if (Position == CssConstants.Fixed)
                _location = GetActualLocation(Left, Top);
        }
    }

    public string Top
    {
        get { return _top; }
        set 
        {
            _top = value;

            if (Position == CssConstants.Fixed)
                _location = GetActualLocation(Left, Top);
        }
    }

    public string Width { get; set; } = "auto";
    public string MaxWidth { get; set; } = "none";
    public string Height { get; set; } = "auto";
    public string BackgroundColor { get; set; } = "transparent";
    public string BackgroundImage { get; set; } = "none";
    public string BackgroundPosition { get; set; } = "0% 0%";
    public string BackgroundRepeat { get; set; } = "repeat";
    public string BackgroundSize { get; set; } = "auto";
    public string BackgroundGradient { get; set; } = "none";
    public string BackgroundGradientAngle { get; set; } = "90";

    public string Color
    {
        get { return _color; }
        set
        {
            _color = value;
            _actualColor = RColor.Empty;
        }
    }

    public string Content { get; set; } = "normal";
    public string Display { get; set; } = "inline";
    public string Direction { get; set; } = "ltr";
    public string EmptyCells { get; set; } = "show";
    public string Float { get; set; } = "none";
    public string Clear { get; set; } = "none";
    public string Position { get; set; } = "static";

    public string LineHeight
    {
        get { return _lineHeight; }
        set { _lineHeight = $"{CssValueParser.ParseLength(value, Size.Height, this, CssConstants.Em)}px"; }
    }

    public string VerticalAlign { get; set; } = "baseline";

    public string TextIndent
    {
        get { return _textIndent; }
        set { _textIndent = NoEms(value); }
    }

    public string TextAlign { get; set; } = string.Empty;
    public string TextDecoration { get; set; } = string.Empty;
    public string WhiteSpace { get; set; } = "normal";
    public string Visibility { get; set; } = "visible";

    public string WordSpacing
    {
        get { return _wordSpacing; }
        set { _wordSpacing = NoEms(value); }
    }

    public string WordBreak { get; set; } = "normal";
    public string Opacity { get; set; } = "1";
    public string BoxShadow { get; set; } = "none";
    public string FlexDirection { get; set; } = "row";
    public string JustifyContent { get; set; } = "flex-start";
    public string AlignItems { get; set; } = "stretch";
    public string FontFamily { get; set; }

    public string FontSize
    {
        get { return _fontSize; }
        set
        {
            string length = RegexParserUtils.Search(RegexParserUtils.CssLength, value);

            if (length != null)
            {
                string computedValue;
                CssLength len = new(length);

                if (len.HasError)
                {
                    computedValue = "medium";
                }
                else if (len.Unit == CssUnit.Ems && GetParent() != null)
                {
                    computedValue = len.ConvertEmToPoints(GetParent().ActualFont.Size).ToString();
                }
                else
                {
                    computedValue = len.ToString();
                }

                _fontSize = computedValue;
            }
            else
            {
                _fontSize = value;
            }
        }
    }

    public string FontStyle { get; set; } = "normal";
    public string FontVariant { get; set; } = "normal";
    public string FontWeight { get; set; } = "normal";
    public string ListStyle { get; set; } = string.Empty;
    public string Overflow { get; set; } = "visible";
    public string ListStylePosition { get; set; } = "outside";
    public string ListStyleImage { get; set; } = string.Empty;
    public string ListStyleType { get; set; } = "disc";

    #endregion CSS Propertier

    public RPoint Location
    {
        get {
            if (_location.IsEmpty && Position == CssConstants.Fixed)
                _location = GetActualLocation(Left, Top);

            return _location;
        }
        set {
            _location = value;
        }
    }

    public RSize Size
    {
        get { return _size; }
        set { _size = value; }
    }

    public RRect Bounds => new(Location, Size);

    public double AvailableWidth => Size.Width - ActualBorderLeftWidth - ActualPaddingLeft - ActualPaddingRight - ActualBorderRightWidth;

    public double ActualRight
    {
        get { return Location.X + Size.Width; }
        set { Size = new RSize(value - Location.X, Size.Height); }
    }

    public double ActualBottom
    {
        get { return Location.Y + Size.Height; }
        set { Size = new RSize(Size.Width, value - Location.Y); }
    }

    public double ClientLeft => Location.X + ActualBorderLeftWidth + ActualPaddingLeft;
    public double ClientTop => Location.Y + ActualBorderTopWidth + ActualPaddingTop;
    public double ClientRight => ActualRight - ActualPaddingRight - ActualBorderRightWidth;
    public double ClientBottom => ActualBottom - ActualPaddingBottom - ActualBorderBottomWidth;
    public RRect ClientRectangle => RRect.FromLTRB(ClientLeft, ClientTop, ClientRight, ClientBottom);

    public double ActualHeight
    {
        get
        {
            if (double.IsNaN(_actualHeight))
                _actualHeight = CssValueParser.ParseLength(Height, Size.Height, this);

            return _actualHeight;
        }
    }

    public double ActualWidth
    {
        get
        {
            if (double.IsNaN(_actualWidth))
                _actualWidth = CssValueParser.ParseLength(Width, Size.Width, this);

            return _actualWidth;
        }
    }

    public double ActualPaddingTop
    {
        get
        {
            if (double.IsNaN(_actualPaddingTop))
                _actualPaddingTop = CssValueParser.ParseLength(PaddingTop, Size.Width, this);

            return _actualPaddingTop;
        }
    }

    public double ActualPaddingLeft
    {
        get
        {
            if (double.IsNaN(_actualPaddingLeft))
                _actualPaddingLeft = CssValueParser.ParseLength(PaddingLeft, Size.Width, this);

            return _actualPaddingLeft;
        }
    }

    public double ActualPaddingBottom
    {
        get
        {
            if (double.IsNaN(_actualPaddingBottom))
                _actualPaddingBottom = CssValueParser.ParseLength(PaddingBottom, Size.Width, this);

            return _actualPaddingBottom;
        }
    }

    public double ActualPaddingRight
    {
        get
        {
            if (double.IsNaN(_actualPaddingRight))
                _actualPaddingRight = CssValueParser.ParseLength(PaddingRight, Size.Width, this);

            return _actualPaddingRight;
        }
    }

    public double ActualMarginTop
    {
        get
        {
            if (double.IsNaN(_actualMarginTop))
            {
                if (MarginTop == CssConstants.Auto)
                    MarginTop = "0";

                var actualMarginTop = CssValueParser.ParseLength(MarginTop, Size.Width, this);

                if (MarginLeft.EndsWith("%"))
                    return actualMarginTop;

                _actualMarginTop = actualMarginTop;
            }

            return _actualMarginTop;
        }
    }

    public double CollapsedMarginTop
    {
        get { return double.IsNaN(_collapsedMarginTop) ? 0 : _collapsedMarginTop; }
        set { _collapsedMarginTop = value; }
    }

    public double ActualMarginLeft
    {
        get
        {
            if (double.IsNaN(_actualMarginLeft))
            {
                if (MarginLeft == CssConstants.Auto)
                    MarginLeft = "0";

                var actualMarginLeft = CssValueParser.ParseLength(MarginLeft, Size.Width, this);

                if (MarginLeft.EndsWith("%"))
                    return actualMarginLeft;

                _actualMarginLeft = actualMarginLeft;
            }
            return _actualMarginLeft;
        }
    }

    public double ActualMarginBottom
    {
        get
        {
            if (double.IsNaN(_actualMarginBottom))
            {
                if (MarginBottom == CssConstants.Auto)
                    MarginBottom = "0";

                var actualMarginBottom = CssValueParser.ParseLength(MarginBottom, Size.Width, this);

                if (MarginLeft.EndsWith("%"))
                    return actualMarginBottom;

                _actualMarginBottom = actualMarginBottom;
            }

            return _actualMarginBottom;
        }
    }

    public double ActualMarginRight
    {
        get
        {
            if (double.IsNaN(_actualMarginRight))
            {
                if (MarginRight == CssConstants.Auto)
                    MarginRight = "0";
                var actualMarginRight = CssValueParser.ParseLength(MarginRight, Size.Width, this);
                if (MarginLeft.EndsWith("%"))
                    return actualMarginRight;
                _actualMarginRight = actualMarginRight;
            }
            return _actualMarginRight;
        }
    }

    public double ActualBorderTopWidth
    {
        get
        {
            if (double.IsNaN(_actualBorderTopWidth))
            {
                _actualBorderTopWidth = CssValueParser.GetActualBorderWidth(BorderTopWidth, this);

                if (string.IsNullOrEmpty(BorderTopStyle) || BorderTopStyle == CssConstants.None)
                    _actualBorderTopWidth = 0f;
            }

            return _actualBorderTopWidth;
        }
    }

    public double ActualBorderLeftWidth
    {
        get
        {
            if (double.IsNaN(_actualBorderLeftWidth))
            {
                _actualBorderLeftWidth = CssValueParser.GetActualBorderWidth(BorderLeftWidth, this);

                if (string.IsNullOrEmpty(BorderLeftStyle) || BorderLeftStyle == CssConstants.None)
                    _actualBorderLeftWidth = 0f;
            }

            return _actualBorderLeftWidth;
        }
    }

    public double ActualBorderBottomWidth
    {
        get
        {
            if (double.IsNaN(_actualBorderBottomWidth))
            {
                _actualBorderBottomWidth = CssValueParser.GetActualBorderWidth(BorderBottomWidth, this);

                if (string.IsNullOrEmpty(BorderBottomStyle) || BorderBottomStyle == CssConstants.None)
                    _actualBorderBottomWidth = 0f;
            }

            return _actualBorderBottomWidth;
        }
    }

    public double ActualBorderRightWidth
    {
        get
        {
            if (double.IsNaN(_actualBorderRightWidth))
            {
                _actualBorderRightWidth = CssValueParser.GetActualBorderWidth(BorderRightWidth, this);

                if (string.IsNullOrEmpty(BorderRightStyle) || BorderRightStyle == CssConstants.None)
                    _actualBorderRightWidth = 0f;
            }

            return _actualBorderRightWidth;
        }
    }

    public RColor ActualBorderTopColor
    {
        get
        {
            if (_actualBorderTopColor.IsEmpty)
                _actualBorderTopColor = GetActualColor(BorderTopColor);

            return _actualBorderTopColor;
        }
    }

    protected abstract RPoint GetActualLocation(string X, string Y);

    protected abstract RColor GetActualColor(string colorStr);

    public RColor ActualBorderLeftColor
    {
        get
        {
            if (_actualBorderLeftColor.IsEmpty)
                _actualBorderLeftColor = GetActualColor(BorderLeftColor);

            return _actualBorderLeftColor;
        }
    }

    public RColor ActualBorderBottomColor
    {
        get
        {
            if (_actualBorderBottomColor.IsEmpty)
                _actualBorderBottomColor = GetActualColor(BorderBottomColor);

            return _actualBorderBottomColor;
        }
    }

    public RColor ActualBorderRightColor
    {
        get
        {
            if (_actualBorderRightColor.IsEmpty)
                _actualBorderRightColor = GetActualColor(BorderRightColor);

            return _actualBorderRightColor;
        }
    }

    public double ActualCornerNw
    {
        get
        {
            if (double.IsNaN(_actualCornerNw))
                _actualCornerNw = CssValueParser.ParseLength(CornerNwRadius, 0, this);

            return _actualCornerNw;
        }
    }

    public double ActualCornerNe
    {
        get
        {
            if (double.IsNaN(_actualCornerNe))
                _actualCornerNe = CssValueParser.ParseLength(CornerNeRadius, 0, this);

            return _actualCornerNe;
        }
    }

    public double ActualCornerSe
    {
        get
        {
            if (double.IsNaN(_actualCornerSe))
                _actualCornerSe = CssValueParser.ParseLength(CornerSeRadius, 0, this);

            return _actualCornerSe;
        }
    }

    public double ActualCornerSw
    {
        get
        {
            if (double.IsNaN(_actualCornerSw))
                _actualCornerSw = CssValueParser.ParseLength(CornerSwRadius, 0, this);

            return _actualCornerSw;
        }
    }

    public bool IsRounded => ActualCornerNe > 0f || ActualCornerNw > 0f || ActualCornerSe > 0f || ActualCornerSw > 0f;
    public double ActualWordSpacing { get; private set; } = double.NaN;

    public RColor ActualColor
    {
        get
        {
            if (_actualColor.IsEmpty)
                _actualColor = GetActualColor(Color);

            return _actualColor;
        }
    }

    public RColor ActualBackgroundColor
    {
        get
        {
            if (_actualBackgroundColor.IsEmpty)
                _actualBackgroundColor = GetActualColor(BackgroundColor);

            return _actualBackgroundColor;
        }
    }

    public RColor ActualBackgroundGradient
    {
        get
        {
            if (_actualBackgroundGradient.IsEmpty)
                _actualBackgroundGradient = GetActualColor(BackgroundGradient);

            return _actualBackgroundGradient;
        }
    }

    public double ActualBackgroundGradientAngle
    {
        get
        {
            if (double.IsNaN(_actualBackgroundGradientAngle))
                _actualBackgroundGradientAngle = CssValueParser.ParseNumber(BackgroundGradientAngle, 360f);

            return _actualBackgroundGradientAngle;
        }
    }

    public RFont ActualParentFont => GetParent() == null ? ActualFont : GetParent().ActualFont;

    public RFont ActualFont
    {
        get
        {
            if (_actualFont == null)
            {
                if (string.IsNullOrEmpty(FontFamily))
                    FontFamily = CssConstants.DefaultFont;

                if (string.IsNullOrEmpty(FontSize))
                    FontSize = CssConstants.FontSize.ToString(CultureInfo.InvariantCulture) + "pt";

                RFontStyle st = RFontStyle.Regular;

                if (FontStyle == CssConstants.Italic || FontStyle == CssConstants.Oblique)
                    st |= RFontStyle.Italic;

                if (FontWeight != CssConstants.Normal && FontWeight != CssConstants.Lighter && !string.IsNullOrEmpty(FontWeight) && FontWeight != CssConstants.Inherit)
                    st |= RFontStyle.Bold;
                
                double parentSize = CssConstants.FontSize;

                if (GetParent() != null)
                    parentSize = GetParent().ActualFont.Size;
                
                var fsize = FontSize switch
                {
                    CssConstants.Medium => CssConstants.FontSize,
                    CssConstants.XXSmall => CssConstants.FontSize - 4,
                    CssConstants.XSmall => CssConstants.FontSize - 3,
                    CssConstants.Small => CssConstants.FontSize - 2,
                    CssConstants.Large => CssConstants.FontSize + 2,
                    CssConstants.XLarge => CssConstants.FontSize + 3,
                    CssConstants.XXLarge => CssConstants.FontSize + 4,
                    CssConstants.Smaller => parentSize - 2,
                    CssConstants.Larger => parentSize + 2,
                    _ => CssValueParser.ParseLength(FontSize, parentSize, parentSize, null, true, true),
                };
                
                if (fsize <= 1f)
                    fsize = CssConstants.FontSize;

                _actualFont = GetCachedFont(FontFamily, fsize, st);
            }

            return _actualFont;
        }
    }

    protected abstract RFont GetCachedFont(string fontFamily, double fsize, RFontStyle st);

    public double ActualLineHeight
    {
        get
        {
            if (double.IsNaN(_actualLineHeight))
                _actualLineHeight = CssValueParser.ParseLength(LineHeight, Size.Height, this);

            return _actualLineHeight;
        }
    }

    public double ActualTextIndent
    {
        get
        {
            if (double.IsNaN(_actualTextIndent))
                _actualTextIndent = CssValueParser.ParseLength(TextIndent, Size.Width, this);

            return _actualTextIndent;
        }
    }

    public double ActualBorderSpacingHorizontal
    {
        get
        {
            if (double.IsNaN(_actualBorderSpacingHorizontal))
            {
                MatchCollection matches = RegexParserUtils.Match(RegexParserUtils.CssLength, BorderSpacing);

                if (matches.Count == 0)
                {
                    _actualBorderSpacingHorizontal = 0;
                }
                else if (matches.Count > 0)
                {
                    _actualBorderSpacingHorizontal = CssValueParser.ParseLength(matches[0].Value, 1, this);
                }
            }

            return _actualBorderSpacingHorizontal;
        }
    }

    public double ActualBorderSpacingVertical
    {
        get
        {
            if (double.IsNaN(_actualBorderSpacingVertical))
            {
                MatchCollection matches = RegexParserUtils.Match(RegexParserUtils.CssLength, BorderSpacing);

                if (matches.Count == 0)
                {
                    _actualBorderSpacingVertical = 0;
                }
                else if (matches.Count == 1)
                {
                    _actualBorderSpacingVertical = CssValueParser.ParseLength(matches[0].Value, 1, this);
                }
                else
                {
                    _actualBorderSpacingVertical = CssValueParser.ParseLength(matches[1].Value, 1, this);
                }
            }

            return _actualBorderSpacingVertical;
        }
    }

    protected abstract CssBoxProperties GetParent();

    public double GetEmHeight() => ActualFont.Size * (96.0 / 72.0);

    protected string NoEms(string length)
    {
        var len = new CssLength(length);

        if (len.Unit == CssUnit.Ems)
            length = len.ConvertEmToPixels(GetEmHeight()).ToString();

        return length;
    }

    protected void SetAllBorders(string style = null, string width = null, string color = null)
    {
        if (style != null)
            BorderLeftStyle = BorderTopStyle = BorderRightStyle = BorderBottomStyle = style;

        if (width != null)
            BorderLeftWidth = BorderTopWidth = BorderRightWidth = BorderBottomWidth = width;

        if (color != null)
            BorderLeftColor = BorderTopColor = BorderRightColor = BorderBottomColor = color;
    }

    protected void MeasureWordSpacing(RGraphics g)
    {
        if (!double.IsNaN(ActualWordSpacing))
            return;

        ActualWordSpacing = CssUtils.WhiteSpace(g, this);

        if (WordSpacing == CssConstants.Normal)
            return;

        string len = RegexParserUtils.Search(RegexParserUtils.CssLength, WordSpacing);
        ActualWordSpacing += CssValueParser.ParseLength(len, 1, this);
    }

    protected void InheritStyle(CssBox p, bool everything)
    {
        if (p == null)
            return;

        BorderSpacing = p.BorderSpacing;
        BorderCollapse = p.BorderCollapse;
        _color = p._color;
        EmptyCells = p.EmptyCells;
        WhiteSpace = p.WhiteSpace;
        Visibility = p.Visibility;
        _textIndent = p._textIndent;
        TextAlign = p.TextAlign;
        FontFamily = p.FontFamily;
        _fontSize = p._fontSize;
        FontStyle = p.FontStyle;
        FontVariant = p.FontVariant;
        FontWeight = p.FontWeight;
        ListStyleImage = p.ListStyleImage;
        ListStylePosition = p.ListStylePosition;
        ListStyleType = p.ListStyleType;
        ListStyle = p.ListStyle;
        _lineHeight = p._lineHeight;
        WordBreak = p.WordBreak;
        Direction = p.Direction;

        if (!everything)
            return;

        BackgroundColor = p.BackgroundColor;
        BackgroundGradient = p.BackgroundGradient;
        BackgroundGradientAngle = p.BackgroundGradientAngle;
        BackgroundImage = p.BackgroundImage;
        BackgroundPosition = p.BackgroundPosition;
        BackgroundRepeat = p.BackgroundRepeat;
        BackgroundSize = p.BackgroundSize;
        _borderTopWidth = p._borderTopWidth;
        _borderRightWidth = p._borderRightWidth;
        _borderBottomWidth = p._borderBottomWidth;
        _borderLeftWidth = p._borderLeftWidth;
        _borderTopColor = p._borderTopColor;
        _borderRightColor = p._borderRightColor;
        _borderBottomColor = p._borderBottomColor;
        _borderLeftColor = p._borderLeftColor;
        BorderTopStyle = p.BorderTopStyle;
        BorderRightStyle = p.BorderRightStyle;
        BorderBottomStyle = p.BorderBottomStyle;
        BorderLeftStyle = p.BorderLeftStyle;
        _bottom = p._bottom;
        CornerNwRadius = p.CornerNwRadius;
        CornerNeRadius = p.CornerNeRadius;
        CornerSeRadius = p.CornerSeRadius;
        CornerSwRadius = p.CornerSwRadius;
        _cornerRadius = p._cornerRadius;
        Display = p.Display;
        Float = p.Float;
        Height = p.Height;
        MarginBottom = p.MarginBottom;
        MarginLeft = p.MarginLeft;
        MarginRight = p.MarginRight;
        MarginTop = p.MarginTop;
        _left = p._left;
        _lineHeight = p._lineHeight;
        Overflow = p.Overflow;
        _paddingLeft = p._paddingLeft;
        _paddingBottom = p._paddingBottom;
        _paddingRight = p._paddingRight;
        _paddingTop = p._paddingTop;
        _right = p._right;
        TextDecoration = p.TextDecoration;
        _top = p._top;
        Position = p.Position;
        VerticalAlign = p.VerticalAlign;
        Width = p.Width;
        MaxWidth = p.MaxWidth;
        _wordSpacing = p._wordSpacing;
        Opacity = p.Opacity;
        BoxShadow = p.BoxShadow;
        FlexDirection = p.FlexDirection;
        JustifyContent = p.JustifyContent;
        AlignItems = p.AlignItems;
    }
}
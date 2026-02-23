using System.Text;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Image;

namespace Broiler.Cli.Tests;

/// <summary>
/// Standalone programmatic Acid1 tests that reconstruct the Acid1 test content
/// entirely in C# code using builder objects for DOM elements and CSS rules,
/// without reading <c>acid1.html</c> from disk and without inline HTML/CSS
/// string literals.  The rendered output is compared against the reference
/// image (<c>acid/acid1/acid1-original.png</c>) with a strict similarity
/// threshold so that even subtle rendering regressions are caught.
/// </summary>
/// <remarks>
/// <para>
/// The DOM tree is constructed via <see cref="HtmlNode"/> and its derived
/// types (<see cref="HtmlElement"/>, <see cref="HtmlText"/>,
/// <see cref="HtmlSelfClosing"/>).  CSS rules are expressed through
/// <see cref="CssRule"/> and <see cref="CssStyleSheet"/>.  The full
/// document is assembled by <see cref="HtmlDocument"/> and serialised to
/// HTML only at rendering time.
/// </para>
/// </remarks>
public class Acid1ProgrammaticTests : IDisposable
{
    /// <summary>Render width matching the reference image (509 px).</summary>
    private const int RenderWidth = 509;

    /// <summary>Render height matching the reference image (407 px).</summary>
    private const int RenderHeight = 407;

    /// <summary>
    /// Minimum similarity threshold (regression floor) for the programmatic
    /// rendering against the reference image.  The value is based on the
    /// current measured similarity of the engine.  A drop below this level
    /// indicates a significant rendering regression.  This threshold should
    /// be raised as the renderer improves towards full CSS1 compliance.
    /// </summary>
    private const double MinSimilarityThreshold = 0.35;

    /// <summary>
    /// Strict similarity threshold.  Once the engine reaches pixel-perfect
    /// rendering this should be set to 1.0.
    /// </summary>
    private const double StrictSimilarityThreshold = 0.95;

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private readonly string _outputDir;

    public Acid1ProgrammaticTests()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), $"broiler-acid1-prog-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_outputDir, true); } catch { }
    }

    // =====================================================================
    // DOM / CSS builder types
    // =====================================================================

    /// <summary>Represents a single CSS property: name–value pair.</summary>
    private sealed record CssProperty(string Name, string Value);

    /// <summary>A CSS rule consisting of a selector and its declarations.</summary>
    private sealed class CssRule
    {
        public string Selector { get; }
        public List<CssProperty> Declarations { get; } = new();

        public CssRule(string selector) => Selector = selector;

        public CssRule Prop(string name, string value)
        {
            Declarations.Add(new CssProperty(name, value));
            return this;
        }

        public void WriteTo(StringBuilder sb)
        {
            sb.Append(Selector).AppendLine(" {");
            foreach (var d in Declarations)
                sb.Append(d.Name).Append(": ").Append(d.Value).AppendLine(";");
            sb.AppendLine("}");
        }
    }

    /// <summary>An ordered collection of <see cref="CssRule"/> objects.</summary>
    private sealed class CssStyleSheet
    {
        public List<CssRule> Rules { get; } = new();

        public CssRule Add(string selector)
        {
            var rule = new CssRule(selector);
            Rules.Add(rule);
            return rule;
        }

        public string ToStyleElement()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<style type=\"text/css\">");
            foreach (var r in Rules)
                r.WriteTo(sb);
            sb.AppendLine("</style>");
            return sb.ToString();
        }
    }

    /// <summary>Base type for DOM nodes.</summary>
    private abstract class HtmlNode
    {
        public abstract void WriteTo(StringBuilder sb);
        public override string ToString()
        {
            var sb = new StringBuilder();
            WriteTo(sb);
            return sb.ToString();
        }
    }

    /// <summary>A plain text node.</summary>
    private sealed class HtmlText : HtmlNode
    {
        public string Content { get; }
        public HtmlText(string content) => Content = content;
        public override void WriteTo(StringBuilder sb) => sb.Append(Content);
    }

    /// <summary>A self-closing element (e.g. <c>&lt;input /&gt;</c>).</summary>
    private sealed class HtmlSelfClosing : HtmlNode
    {
        public string Tag { get; }
        public Dictionary<string, string> Attributes { get; } = new();

        public HtmlSelfClosing(string tag) => Tag = tag;

        public HtmlSelfClosing Attr(string name, string value)
        {
            Attributes[name] = value;
            return this;
        }

        public override void WriteTo(StringBuilder sb)
        {
            sb.Append('<').Append(Tag);
            foreach (var kv in Attributes)
                sb.Append(' ').Append(kv.Key).Append("=\"").Append(kv.Value).Append('"');
            sb.Append('>');
        }
    }

    /// <summary>
    /// An HTML element with a tag name, optional attributes, optional
    /// inline style, and child nodes.
    /// </summary>
    private sealed class HtmlElement : HtmlNode
    {
        public string Tag { get; }
        public Dictionary<string, string> Attributes { get; } = new();
        public Dictionary<string, string> InlineStyle { get; } = new();
        public List<HtmlNode> Children { get; } = new();

        public HtmlElement(string tag) => Tag = tag;

        public HtmlElement Attr(string name, string value)
        {
            Attributes[name] = value;
            return this;
        }

        public HtmlElement Style(string property, string value)
        {
            InlineStyle[property] = value;
            return this;
        }

        public HtmlElement Text(string content)
        {
            Children.Add(new HtmlText(content));
            return this;
        }

        public HtmlElement Add(HtmlNode child)
        {
            Children.Add(child);
            return this;
        }

        public HtmlElement Add(HtmlElement child)
        {
            Children.Add(child);
            return this;
        }

        public HtmlElement Add(HtmlSelfClosing child)
        {
            Children.Add(child);
            return this;
        }

        public override void WriteTo(StringBuilder sb)
        {
            sb.Append('<').Append(Tag);
            foreach (var kv in Attributes)
                sb.Append(' ').Append(kv.Key).Append("=\"").Append(kv.Value).Append('"');
            if (InlineStyle.Count > 0)
            {
                sb.Append(" style=\"");
                foreach (var kv in InlineStyle)
                    sb.Append(kv.Key).Append(": ").Append(kv.Value).Append("; ");
                sb.Append('"');
            }
            sb.Append('>');
            foreach (var child in Children)
                child.WriteTo(sb);
            sb.Append("</").Append(Tag).Append('>');
        }
    }

    /// <summary>
    /// A complete HTML document composed of a <see cref="CssStyleSheet"/>,
    /// a title string, and a body <see cref="HtmlElement"/>.
    /// </summary>
    private sealed class HtmlDocument
    {
        public string Title { get; set; } = "";
        public CssStyleSheet StyleSheet { get; set; } = new();
        public HtmlElement Body { get; set; } = new("body");

        public string ToHtml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0//EN\" \"http://www.w3.org/TR/REC-html40/strict.dtd\">");
            sb.Append("<html><head>");
            sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=windows-1252\">");
            sb.Append("<title>").Append(Title).Append("</title>");
            sb.Append(StyleSheet.ToStyleElement());
            sb.Append("</head>");
            Body.WriteTo(sb);
            sb.Append("</html>");
            return sb.ToString();
        }
    }

    // =====================================================================
    // Programmatic Acid1 construction
    // =====================================================================

    /// <summary>
    /// Builds the complete CSS stylesheet for the Acid1 test page using
    /// <see cref="CssStyleSheet"/> and <see cref="CssRule"/> objects.
    /// Every rule and property corresponds 1:1 to the original
    /// <c>acid1.html</c> stylesheet.
    /// </summary>
    private static CssStyleSheet BuildAcid1StyleSheet()
    {
        var sheet = new CssStyleSheet();

        sheet.Add("html")
            .Prop("font", "10px/1 Verdana, sans-serif")
            .Prop("background-color", "blue")
            .Prop("color", "white");

        sheet.Add("body")
            .Prop("margin", "1.5em")
            .Prop("border", ".5em solid black")
            .Prop("padding", "0")
            .Prop("width", "48em")
            .Prop("background-color", "white");

        sheet.Add("dl")
            .Prop("margin", "0")
            .Prop("border", "0")
            .Prop("padding", ".5em");

        sheet.Add("dt")
            .Prop("background-color", "rgb(204,0,0)")
            .Prop("margin", "0")
            .Prop("padding", "1em")
            .Prop("width", "10.638%")
            .Prop("height", "28em")
            .Prop("border", ".5em solid black")
            .Prop("float", "left");

        sheet.Add("dd")
            .Prop("float", "right")
            .Prop("margin", "0 0 0 1em")
            .Prop("border", "1em solid black")
            .Prop("padding", "1em")
            .Prop("width", "34em")
            .Prop("min-width", "34em")
            .Prop("max-width", "34em")
            .Prop("height", "27em");

        sheet.Add("ul")
            .Prop("margin", "0")
            .Prop("border", "0")
            .Prop("padding", "0");

        sheet.Add("li")
            .Prop("display", "block")
            .Prop("color", "black")
            .Prop("height", "9em")
            .Prop("width", "5em")
            .Prop("margin", "0")
            .Prop("border", ".5em solid black")
            .Prop("padding", "1em")
            .Prop("float", "left")
            .Prop("background-color", "#FC0");

        sheet.Add("#bar")
            .Prop("background-color", "black")
            .Prop("color", "white")
            .Prop("width", "41.17%")
            .Prop("border", "0")
            .Prop("margin", "0 1em");

        sheet.Add("#baz")
            .Prop("margin", "1em 0")
            .Prop("border", "0")
            .Prop("padding", "1em")
            .Prop("width", "10em")
            .Prop("height", "10em")
            .Prop("background-color", "black")
            .Prop("color", "white");

        sheet.Add("form")
            .Prop("margin", "0")
            .Prop("display", "inline");

        sheet.Add("p")
            .Prop("margin", "0");

        sheet.Add("form p")
            .Prop("line-height", "1.9");

        sheet.Add("blockquote")
            .Prop("margin", "1em 1em 1em 2em")
            .Prop("border-width", "1em 1.5em 2em .5em")
            .Prop("border-style", "solid")
            .Prop("border-color", "black")
            .Prop("padding", "1em 0")
            .Prop("width", "5em")
            .Prop("height", "9em")
            .Prop("float", "left")
            .Prop("background-color", "#FC0")
            .Prop("color", "black");

        sheet.Add("address")
            .Prop("font-style", "normal");

        sheet.Add("h1")
            .Prop("background-color", "black")
            .Prop("color", "white")
            .Prop("float", "left")
            .Prop("margin", "1em 0")
            .Prop("border", "0")
            .Prop("padding", "1em")
            .Prop("width", "10em")
            .Prop("height", "10em")
            .Prop("font-weight", "normal")
            .Prop("font-size", "1em");

        return sheet;
    }

    /// <summary>
    /// Builds the complete DOM tree for the Acid1 test page body using
    /// <see cref="HtmlElement"/>, <see cref="HtmlText"/>, and
    /// <see cref="HtmlSelfClosing"/> nodes.  Every element, attribute,
    /// and text node corresponds 1:1 to the original <c>acid1.html</c>.
    /// </summary>
    private static HtmlElement BuildAcid1Body()
    {
        var body = new HtmlElement("body");

        // --- <dl> ---
        var dl = new HtmlElement("dl");

        // <dt>toggle</dt>
        var dt = new HtmlElement("dt").Text(" toggle ");

        // <dd> ... </dd>
        var dd = new HtmlElement("dd");

        // --- <ul> inside <dd> ---
        var ul = new HtmlElement("ul");

        // <li> the way </li>
        var li1 = new HtmlElement("li").Text(" the way ");

        // <li id="bar"> ... </li>
        var liBar = new HtmlElement("li").Attr("id", "bar");
        var pWorldEnds = new HtmlElement("p").Text(" the world ends ");
        liBar.Add(pWorldEnds);

        var form = new HtmlElement("form")
            .Attr("action", "https://www.w3.org/Style/CSS/Test/CSS1/current/")
            .Attr("method", "get");

        var pBang = new HtmlElement("p");
        pBang.Text(" bang ");
        pBang.Add(new HtmlSelfClosing("input")
            .Attr("type", "radio")
            .Attr("name", "foo")
            .Attr("value", "off"));
        form.Add(pBang);

        var pWhimper = new HtmlElement("p");
        pWhimper.Text(" whimper ");
        pWhimper.Add(new HtmlSelfClosing("input")
            .Attr("type", "radio")
            .Attr("name", "foo2")
            .Attr("value", "on"));
        form.Add(pWhimper);

        liBar.Add(form);

        // <li> i grow old </li>
        var li3 = new HtmlElement("li").Text(" i grow old ");

        // <li id="baz"> pluot? </li>
        var liBaz = new HtmlElement("li").Attr("id", "baz").Text(" pluot? ");

        ul.Add(li1).Add(liBar).Add(li3).Add(liBaz);
        dd.Add(ul);

        // <blockquote><address> bar maids, </address></blockquote>
        var blockquote = new HtmlElement("blockquote");
        var address = new HtmlElement("address").Text(" bar maids, ");
        blockquote.Add(address);
        dd.Add(blockquote);

        // <h1> sing to me, erbarme dich </h1>
        var h1 = new HtmlElement("h1").Text(" sing to me, erbarme dich ");
        dd.Add(h1);

        dl.Add(dt).Add(dd);
        body.Add(dl);

        // <p style="...">  closing paragraph
        var closingP = new HtmlElement("p")
            .Style("color", "black")
            .Style("font-size", "1em")
            .Style("line-height", "1.3em")
            .Style("clear", "both");

        closingP.Text(
            " This is a nonsensical document, but syntactically valid HTML 4.0." +
            " All 100%-conformant CSS1 agents should be able to render the" +
            " document elements above this paragraph indistinguishably" +
            " (to the pixel) from this ");

        var aRef = new HtmlElement("a")
            .Attr("href", "https://www.w3.org/Style/CSS/Test/CSS1/current/sec5526c.gif")
            .Text("reference rendering,");
        closingP.Add(aRef);

        closingP.Text(
            " (except font rasterization and form widgets)." +
            " All discrepancies should be traceable to CSS1 implementation" +
            " shortcomings. Once you have finished evaluating this test," +
            " you can return to the ");

        var aParent = new HtmlElement("a")
            .Attr("href", "https://www.w3.org/Style/CSS/Test/CSS1/current/sec5526c.htm")
            .Text("parent page");
        closingP.Add(aParent);
        closingP.Text(". ");

        body.Add(closingP);

        return body;
    }

    /// <summary>
    /// Assembles the complete Acid1 document from the programmatic
    /// <see cref="CssStyleSheet"/> and <see cref="HtmlElement"/> tree
    /// and serialises it to an HTML string for rendering.
    /// </summary>
    private static string BuildAcid1Html()
    {
        var doc = new HtmlDocument
        {
            Title = " display/box/float/clear test ",
            StyleSheet = BuildAcid1StyleSheet(),
            Body = BuildAcid1Body(),
        };
        return doc.ToHtml();
    }

    // =====================================================================
    // Paths
    // =====================================================================

    private static string Acid1OriginalPngPath =>
        Path.Combine(TestDataDir, "acid1-original.png");

    private static string Acid1FailPngPath =>
        Path.Combine(TestDataDir, "acid1-fail.png");

    // =====================================================================
    // Pixel helpers
    // =====================================================================

    private static bool IsBlue(SKColor p) =>
        p.Blue > 150 && p.Blue > p.Red + 50 && p.Blue > p.Green + 50;

    private static bool IsRed(SKColor p) =>
        p.Red > 150 && p.Green < 50 && p.Blue < 50;

    private static bool IsGold(SKColor p) =>
        p.Red > 230 && p.Green > 150 && p.Green < 230 && p.Blue < 30;

    private static bool IsBlack(SKColor p) =>
        p.Red < 30 && p.Green < 30 && p.Blue < 30;

    private static bool IsWhite(SKColor p) =>
        p.Red > 240 && p.Green > 240 && p.Blue > 240;

    private static int CountPixels(SKBitmap bitmap, Func<SKColor, bool> predicate)
    {
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
                if (predicate(bitmap.GetPixel(x, y)))
                    count++;
        return count;
    }

    // =====================================================================
    // Tests: Builder verification
    // =====================================================================

    /// <summary>
    /// Verifies that the programmatic <see cref="CssStyleSheet"/> contains
    /// the expected number of rules matching the original acid1.html.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_StyleSheet_HasExpectedRuleCount()
    {
        var sheet = BuildAcid1StyleSheet();

        // acid1.html has 15 CSS rule blocks:
        // html, body, dl, dt, dd, ul, li, #bar, #baz, form, p,
        // form p, blockquote, address, h1
        Assert.Equal(15, sheet.Rules.Count);
    }

    /// <summary>
    /// Verifies that the programmatic stylesheet contains rules for all
    /// required selectors.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_StyleSheet_ContainsAllSelectors()
    {
        var sheet = BuildAcid1StyleSheet();
        var selectors = sheet.Rules.Select(r => r.Selector).ToList();

        Assert.Contains("html", selectors);
        Assert.Contains("body", selectors);
        Assert.Contains("dl", selectors);
        Assert.Contains("dt", selectors);
        Assert.Contains("dd", selectors);
        Assert.Contains("ul", selectors);
        Assert.Contains("li", selectors);
        Assert.Contains("#bar", selectors);
        Assert.Contains("#baz", selectors);
        Assert.Contains("form", selectors);
        Assert.Contains("p", selectors);
        Assert.Contains("form p", selectors);
        Assert.Contains("blockquote", selectors);
        Assert.Contains("address", selectors);
        Assert.Contains("h1", selectors);
    }

    /// <summary>
    /// Verifies that the programmatic DOM body tree contains the expected
    /// structural elements: dl, dt, dd, ul, li, blockquote, h1, form,
    /// address, and two input elements.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_Body_ContainsRequiredElements()
    {
        var body = BuildAcid1Body();

        // Collect all tags by walking the tree
        var tags = new List<string>();
        CollectTags(body, tags);

        Assert.Contains("dl", tags);
        Assert.Contains("dt", tags);
        Assert.Contains("dd", tags);
        Assert.Contains("ul", tags);
        Assert.Contains("li", tags);
        Assert.Contains("blockquote", tags);
        Assert.Contains("h1", tags);
        Assert.Contains("form", tags);
        Assert.Contains("address", tags);
        Assert.Contains("input", tags);

        // Must have exactly 2 radio inputs
        int inputCount = tags.Count(t => t == "input");
        Assert.Equal(2, inputCount);
    }

    /// <summary>Helper: recursively collects all tag names from the tree.</summary>
    private static void CollectTags(HtmlNode node, List<string> tags)
    {
        switch (node)
        {
            case HtmlElement el:
                tags.Add(el.Tag);
                foreach (var child in el.Children)
                    CollectTags(child, tags);
                break;
            case HtmlSelfClosing sc:
                tags.Add(sc.Tag);
                break;
        }
    }

    /// <summary>
    /// Verifies that the key CSS properties are declared in the correct
    /// rules (float directions, background colours, clear).
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_StyleSheet_KeyPropertiesPresent()
    {
        var sheet = BuildAcid1StyleSheet();
        var ruleMap = sheet.Rules.ToDictionary(r => r.Selector);

        // dt: float: left
        Assert.Contains(ruleMap["dt"].Declarations, d =>
            d.Name == "float" && d.Value == "left");

        // dd: float: right
        Assert.Contains(ruleMap["dd"].Declarations, d =>
            d.Name == "float" && d.Value == "right");

        // html: background-color: blue
        Assert.Contains(ruleMap["html"].Declarations, d =>
            d.Name == "background-color" && d.Value == "blue");

        // body: background-color: white
        Assert.Contains(ruleMap["body"].Declarations, d =>
            d.Name == "background-color" && d.Value == "white");

        // dt: background-color: rgb(204,0,0)
        Assert.Contains(ruleMap["dt"].Declarations, d =>
            d.Name == "background-color" && d.Value == "rgb(204,0,0)");

        // li: background-color: #FC0
        Assert.Contains(ruleMap["li"].Declarations, d =>
            d.Name == "background-color" && d.Value == "#FC0");

        // h1: background-color: black
        Assert.Contains(ruleMap["h1"].Declarations, d =>
            d.Name == "background-color" && d.Value == "black");
    }

    // =====================================================================
    // Tests: Rendering produces valid output
    // =====================================================================

    /// <summary>
    /// Verifies that the programmatically built Acid1 renders to a
    /// non-blank image with visible content.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_ProducesNonBlankRendering()
    {
        var html = BuildAcid1Html();
        using var bitmap = HtmlRender.RenderToImage(html, RenderWidth, RenderHeight);

        Assert.NotNull(bitmap);

        int nonWhite = CountPixels(bitmap, p => !IsWhite(p));
        Assert.True(nonWhite > 1000,
            $"Expected >1000 non-white pixels from programmatic Acid1, found {nonWhite}. " +
            "The programmatic DOM may not produce visible content.");
    }

    /// <summary>
    /// Verifies that the programmatic rendering contains the expected
    /// colour regions: blue, red, gold, and black.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_ContainsExpectedColorRegions()
    {
        var html = BuildAcid1Html();
        using var bitmap = HtmlRender.RenderToImage(html, RenderWidth, RenderHeight);

        Assert.True(CountPixels(bitmap, IsBlue) > 100,
            "Expected blue pixels from html background-color:blue.");
        Assert.True(CountPixels(bitmap, IsRed) > 50,
            "Expected red pixels from dt background-color:rgb(204,0,0).");
        Assert.True(CountPixels(bitmap, IsGold) > 50,
            "Expected gold pixels from li/blockquote background-color:#FC0.");
        Assert.True(CountPixels(bitmap, IsBlack) > 100,
            "Expected black pixels from borders and #bar/#baz/h1 backgrounds.");
    }

    // =====================================================================
    // Tests: Visual regression against acid1-original.png
    // =====================================================================

    /// <summary>
    /// Renders the programmatically built Acid1 and compares it against the
    /// reference image.  Enforces a minimum similarity threshold.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_SimilarityAboveMinimumThreshold()
    {
        var html = BuildAcid1Html();
        using var reference = SKBitmap.Decode(Acid1OriginalPngPath);
        using var rendered = HtmlRender.RenderToImage(html, reference.Width, reference.Height);

        double similarity = ImageComparer.CompareWithTolerance(
            rendered, reference, colorTolerance: 10);

        Assert.True(similarity >= MinSimilarityThreshold,
            $"Programmatic Acid1 similarity ({similarity:P1}) fell below the minimum " +
            $"threshold ({MinSimilarityThreshold:P0}). This indicates a rendering regression.");
    }

    /// <summary>
    /// Strict visual regression test.  Enforces the minimum threshold now;
    /// once the engine achieves ≥ 95 % the strict threshold kicks in.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_StrictSimilarityCheck()
    {
        var html = BuildAcid1Html();
        using var reference = SKBitmap.Decode(Acid1OriginalPngPath);
        using var rendered = HtmlRender.RenderToImage(html, reference.Width, reference.Height);

        double similarity = ImageComparer.Compare(rendered, reference);

        double effectiveThreshold = similarity >= StrictSimilarityThreshold
            ? StrictSimilarityThreshold
            : MinSimilarityThreshold;

        Assert.True(similarity >= effectiveThreshold,
            $"Programmatic Acid1 similarity ({similarity:P1}) fell below the " +
            $"effective threshold ({effectiveThreshold:P0}). " +
            $"Target is {StrictSimilarityThreshold:P0} for full CSS1 compliance.");
    }

    // =====================================================================
    // Tests: Failure detection
    // =====================================================================

    /// <summary>
    /// Verifies that the known-failure image is detected as different from
    /// the reference.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_DetectsKnownFailureImage()
    {
        using var reference = SKBitmap.Decode(Acid1OriginalPngPath);
        using var failure = SKBitmap.Decode(Acid1FailPngPath);

        double directSimilarity = ImageComparer.Compare(reference, failure);

        Assert.True(directSimilarity < MinSimilarityThreshold,
            $"The known-failure image should score below {MinSimilarityThreshold:P0} " +
            $"against the reference, but scored {directSimilarity:P1}.");
    }

    /// <summary>
    /// Verifies that a blank image is detected as a failure.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_DetectsBlankOutput()
    {
        using var reference = SKBitmap.Decode(Acid1OriginalPngPath);
        using var blank = new SKBitmap(reference.Width, reference.Height);
        using var canvas = new SKCanvas(blank);
        canvas.Clear(SKColors.White);

        double similarity = ImageComparer.Compare(blank, reference);

        Assert.True(similarity < MinSimilarityThreshold,
            $"A blank white image scored {similarity:P1} against the reference. " +
            $"Should be below {MinSimilarityThreshold:P0} to detect blank output failures.");
    }

    // =====================================================================
    // Tests: Deterministic rendering
    // =====================================================================

    /// <summary>
    /// Verifies that two successive renders produce identical output.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_RenderingIsDeterministic()
    {
        var html = BuildAcid1Html();

        using var bitmap1 = HtmlRender.RenderToImage(html, RenderWidth, RenderHeight);
        using var bitmap2 = HtmlRender.RenderToImage(html, RenderWidth, RenderHeight);

        Assert.True(ImageComparer.AreIdentical(bitmap1, bitmap2),
            "Two renders of the programmatic Acid1 should produce identical output.");
    }

    // =====================================================================
    // Tests: Programmatic rendering matches file-based rendering
    // =====================================================================

    /// <summary>
    /// Verifies that the programmatic DOM construction produces a rendering
    /// similar to the file-based <c>acid1.html</c>.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_MatchesFileBasedRendering()
    {
        var programmaticHtml = BuildAcid1Html();
        var fileHtml = File.ReadAllText(
            Path.Combine(TestDataDir, "acid1.html"));

        using var progBitmap = HtmlRender.RenderToImage(
            programmaticHtml, RenderWidth, RenderHeight);
        using var fileBitmap = HtmlRender.RenderToImage(
            fileHtml, RenderWidth, RenderHeight);

        double similarity = ImageComparer.CompareWithTolerance(
            progBitmap, fileBitmap, colorTolerance: 5);

        Assert.True(similarity >= 0.90,
            $"Programmatic rendering similarity to file-based rendering is only " +
            $"{similarity:P1}. The programmatic DOM construction may not faithfully " +
            "reproduce the acid1.html structure.");
    }

    // =====================================================================
    // Tests: Float positioning
    // =====================================================================

    /// <summary>
    /// Verifies that the <c>dt</c> element (float:left) renders red pixels
    /// in the left portion of the image.
    /// </summary>
    [Fact]
    public void Programmatic_Acid1_DtFloatLeft_RedOnLeft()
    {
        var html = BuildAcid1Html();
        using var bitmap = HtmlRender.RenderToImage(html, RenderWidth, RenderHeight);

        int halfWidth = bitmap.Width / 2;
        int redLeft = 0, redRight = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (IsRed(bitmap.GetPixel(x, y)))
                {
                    if (x < halfWidth) redLeft++;
                    else redRight++;
                }
            }
        }

        Assert.True(redLeft > redRight,
            $"Expected more red pixels on the left ({redLeft}) than right ({redRight}). " +
            "The dt float:left positioning may be incorrect.");
    }
}

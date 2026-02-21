using Broiler.App.Rendering;

namespace Broiler.App.Tests;

public class DomBridgeTests
{
    private readonly ScriptEngine _engine = new();

    [Fact]
    public void Execute_WithHtml_DocumentTitleIsAccessible()
    {
        var html = "<html><head><title>Test Page</title></head><body></body></html>";
        var result = _engine.Execute(new[] { "var t = document.title;" }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_GetElementByIdReturnsElement()
    {
        var html = "<html><body><div id=\"main\">Hello</div></body></html>";
        var result = _engine.Execute(new[] { "var el = document.getElementById('main');" }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_GetElementByIdReturnsNullForMissing()
    {
        var html = "<html><body><div id=\"main\">Hello</div></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.getElementById('missing'); if (el !== null) throw new Error('expected null');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_GetElementsByTagNameReturnsMatches()
    {
        var html = "<html><body><p id=\"a\">One</p><p id=\"b\">Two</p></body></html>";
        var result = _engine.Execute(
            new[] { "var ps = document.getElementsByTagName('p'); if (ps.length < 1) throw new Error('expected elements');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_CreateElementReturnsNewElement()
    {
        var html = "<html><body></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.createElement('span'); if (el.tagName !== 'SPAN') throw new Error('wrong tag');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_ElementHasExpectedProperties()
    {
        var html = "<html><body><div id=\"test\" class=\"box\">Content</div></body></html>";
        var result = _engine.Execute(
            new[] { @"
                var el = document.getElementById('test');
                if (el.tagName !== 'DIV') throw new Error('wrong tagName');
                if (el.id !== 'test') throw new Error('wrong id');
                if (el.className !== 'box') throw new Error('wrong className');
                if (el.innerHTML !== 'Content') throw new Error('wrong innerHTML');
            " },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_EmptyScriptsReturnsTrue()
    {
        var result = _engine.Execute(Array.Empty<string>(), "<html></html>");
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_InvalidScriptReturnsFalse()
    {
        var result = _engine.Execute(new[] { "invalid js @@!!" }, "<html></html>");
        Assert.False(result);
    }

    [Fact]
    public void DomBridge_ParsesTitle()
    {
        var bridge = new DomBridge();
        using var context = new YantraJS.Core.JSContext();
        bridge.Attach(context, "<html><head><title>My Title</title></head></html>");

        Assert.Equal("My Title", bridge.Title);
    }

    [Fact]
    public void DomBridge_ParsesElements()
    {
        var bridge = new DomBridge();
        using var context = new YantraJS.Core.JSContext();
        bridge.Attach(context, "<html><body><div id=\"a\" class=\"c\">Text</div></body></html>");

        Assert.True(bridge.Elements.Count > 0);
        var div = bridge.Elements[0];
        Assert.Equal("div", div.TagName);
        Assert.Equal("a", div.Id);
        Assert.Equal("c", div.ClassName);
        Assert.Equal("Text", div.InnerHtml);
    }

    [Fact]
    public void DomBridge_NoTitle_ReturnsEmpty()
    {
        var bridge = new DomBridge();
        using var context = new YantraJS.Core.JSContext();
        bridge.Attach(context, "<html><body></body></html>");

        Assert.Equal(string.Empty, bridge.Title);
    }
}

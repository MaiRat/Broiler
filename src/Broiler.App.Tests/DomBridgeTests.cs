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
    public void Execute_WithHtml_CreateElementWithoutArgReturnsFalse()
    {
        var html = "<html><body></body></html>";
        var result = _engine.Execute(new[] { "document.createElement();" }, html);
        Assert.False(result);
    }

    [Fact]
    public void Execute_WithHtml_VoidElementsAreParsed()
    {
        var html = "<html><body><input id=\"field\"><img id=\"logo\" /></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.getElementById('field'); if (el === null) throw new Error('expected input element');" },
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

    // ------------------------------------------------------------------
    //  getElementsByClassName
    // ------------------------------------------------------------------

    [Fact]
    public void Execute_WithHtml_GetElementsByClassNameReturnsMatches()
    {
        var html = "<html><body><div class=\"box\">A</div><p class=\"box\">B</p><span class=\"other\">C</span></body></html>";
        var result = _engine.Execute(
            new[] { "var els = document.getElementsByClassName('box'); if (els.length < 1) throw new Error('expected elements');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_GetElementsByClassNameReturnsEmptyForMissing()
    {
        var html = "<html><body><div class=\"box\">A</div></body></html>";
        var result = _engine.Execute(
            new[] { "var els = document.getElementsByClassName('missing'); if (els.length !== 0) throw new Error('expected empty');" },
            html);
        Assert.True(result);
    }

    // ------------------------------------------------------------------
    //  querySelector / querySelectorAll
    // ------------------------------------------------------------------

    [Fact]
    public void Execute_WithHtml_QuerySelectorByTag()
    {
        var html = "<html><body><p id=\"first\">One</p><p id=\"second\">Two</p></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.querySelector('p'); if (el === null) throw new Error('expected element');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_QuerySelectorById()
    {
        var html = "<html><body><div id=\"target\">Hello</div></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.querySelector('#target'); if (el === null) throw new Error('expected element'); if (el.id !== 'target') throw new Error('wrong id');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_QuerySelectorByClass()
    {
        var html = "<html><body><div class=\"card active\">Hello</div></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.querySelector('.card'); if (el === null) throw new Error('expected element');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_QuerySelectorByAttribute()
    {
        var html = "<html><body><input type=\"text\" id=\"name\"></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.querySelector('[type=text]'); if (el === null) throw new Error('expected element');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_QuerySelectorReturnsNullForNoMatch()
    {
        var html = "<html><body><div>Hello</div></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.querySelector('#nonexistent'); if (el !== null) throw new Error('expected null');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_QuerySelectorAll()
    {
        var html = "<html><body><li class=\"item\">A</li><li class=\"item\">B</li><li class=\"other\">C</li></body></html>";
        var result = _engine.Execute(
            new[] { "var els = document.querySelectorAll('.item'); if (els.length < 1) throw new Error('expected elements');" },
            html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_QuerySelectorCompound()
    {
        var html = "<html><body><div id=\"box\" class=\"card\">Hello</div></body></html>";
        var result = _engine.Execute(
            new[] { "var el = document.querySelector('div.card#box'); if (el === null) throw new Error('expected element');" },
            html);
        Assert.True(result);
    }

    // ------------------------------------------------------------------
    //  element.style
    // ------------------------------------------------------------------

    [Fact]
    public void Execute_WithHtml_StyleSetPropertyAndGetPropertyValue()
    {
        var html = "<html><body><div id=\"el\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.style.setProperty('color', 'red');
              var val = el.style.getPropertyValue('color');
              if (val !== 'red') throw new Error('expected red, got ' + val);"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_StyleCssTextGetterAndSetter()
    {
        var html = "<html><body><div id=\"el\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.style.cssText = 'color: blue; font-size: 14px';
              var color = el.style.getPropertyValue('color');
              if (color !== 'blue') throw new Error('expected blue, got ' + color);
              var size = el.style.getPropertyValue('font-size');
              if (size !== '14px') throw new Error('expected 14px, got ' + size);"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_StyleRemoveProperty()
    {
        var html = "<html><body><div id=\"el\" style=\"color: red\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.style.removeProperty('color');
              var val = el.style.getPropertyValue('color');
              if (val !== '') throw new Error('expected empty string after remove');"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void DomBridge_ParsesInlineStyle()
    {
        var bridge = new DomBridge();
        using var context = new YantraJS.Core.JSContext();
        bridge.Attach(context, "<html><body><div id=\"el\" style=\"color: red; font-size: 12px\">Text</div></body></html>");

        var el = bridge.Elements.FirstOrDefault(e => e.Id == "el");
        Assert.NotNull(el);
        Assert.True(el.Style.ContainsKey("color"));
        Assert.Equal("red", el.Style["color"]);
        Assert.True(el.Style.ContainsKey("font-size"));
    }

    // ------------------------------------------------------------------
    //  element.classList
    // ------------------------------------------------------------------

    [Fact]
    public void Execute_WithHtml_ClassListContains()
    {
        var html = "<html><body><div id=\"el\" class=\"box active\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              if (!el.classList.contains('box')) throw new Error('expected box');
              if (!el.classList.contains('active')) throw new Error('expected active');
              if (el.classList.contains('missing')) throw new Error('unexpected missing');"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_ClassListAdd()
    {
        var html = "<html><body><div id=\"el\" class=\"box\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.classList.add('highlight');
              if (!el.classList.contains('highlight')) throw new Error('expected highlight');
              if (!el.classList.contains('box')) throw new Error('expected box still present');"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_ClassListRemove()
    {
        var html = "<html><body><div id=\"el\" class=\"box active\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.classList.remove('active');
              if (el.classList.contains('active')) throw new Error('active should be removed');
              if (!el.classList.contains('box')) throw new Error('box should remain');"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_ClassListToggleOn()
    {
        var html = "<html><body><div id=\"el\" class=\"box\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              var added = el.classList.toggle('highlight');
              if (!added) throw new Error('expected true (added)');
              if (!el.classList.contains('highlight')) throw new Error('expected highlight');"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_ClassListToggleOff()
    {
        var html = "<html><body><div id=\"el\" class=\"box active\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              var removed = el.classList.toggle('active');
              if (removed) throw new Error('expected false (removed)');
              if (el.classList.contains('active')) throw new Error('active should be removed');"
        }, html);
        Assert.True(result);
    }

    // ------------------------------------------------------------------
    //  element.setAttribute / getAttribute
    // ------------------------------------------------------------------

    [Fact]
    public void Execute_WithHtml_GetAttributeReturnsValue()
    {
        var html = "<html><body><input id=\"field\" type=\"email\"></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('field');
              var t = el.getAttribute('type');
              if (t !== 'email') throw new Error('expected email, got ' + t);"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_GetAttributeReturnsNullForMissing()
    {
        var html = "<html><body><div id=\"el\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              var val = el.getAttribute('data-missing');
              if (val !== null) throw new Error('expected null');"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_SetAttribute()
    {
        var html = "<html><body><div id=\"el\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.setAttribute('data-value', '42');
              var val = el.getAttribute('data-value');
              if (val !== '42') throw new Error('expected 42, got ' + val);"
        }, html);
        Assert.True(result);
    }

    // ------------------------------------------------------------------
    //  Mutable className and innerHTML
    // ------------------------------------------------------------------

    [Fact]
    public void Execute_WithHtml_ClassNameIsMutable()
    {
        var html = "<html><body><div id=\"el\" class=\"old\">Text</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.className = 'new';
              if (el.className !== 'new') throw new Error('expected new');"
        }, html);
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithHtml_InnerHtmlIsMutable()
    {
        var html = "<html><body><div id=\"el\">Old content</div></body></html>";
        var result = _engine.Execute(new[]
        {
            @"var el = document.getElementById('el');
              el.innerHTML = '<span>New</span>';
              if (el.innerHTML !== '<span>New</span>') throw new Error('expected new innerHTML');"
        }, html);
        Assert.True(result);
    }

    // ------------------------------------------------------------------
    //  DomElement.Attributes
    // ------------------------------------------------------------------

    [Fact]
    public void DomBridge_ParsesAttributes()
    {
        var bridge = new DomBridge();
        using var context = new YantraJS.Core.JSContext();
        bridge.Attach(context, "<html><body><input id=\"field\" type=\"checkbox\" checked></body></html>");

        var el = bridge.Elements.FirstOrDefault(e => e.Id == "field");
        Assert.NotNull(el);
        Assert.True(el.Attributes.ContainsKey("type"));
        Assert.Equal("checkbox", el.Attributes["type"]);
    }
}

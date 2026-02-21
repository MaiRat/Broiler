using System.Collections.Generic;
using System.Text.RegularExpressions;
using YantraJS.Core;

namespace Broiler.App.Rendering
{
    /// <summary>
    /// Registers a minimal <c>document</c> object on a <see cref="JSContext"/>
    /// so that JavaScript executed via YantraJS can perform basic DOM queries
    /// against the current page HTML.
    /// </summary>
    public sealed class DomBridge
    {
        private string _title = string.Empty;
        private readonly List<DomElement> _elements = new();

        /// <summary>
        /// The current document title, kept in sync with JavaScript reads/writes.
        /// </summary>
        public string Title => _title;

        /// <summary>
        /// All elements parsed from the HTML source.
        /// </summary>
        public IReadOnlyList<DomElement> Elements => _elements;

        /// <summary>
        /// Parse the supplied <paramref name="html"/> and register a
        /// <c>document</c> global on the given <paramref name="context"/>.
        /// </summary>
        public void Attach(JSContext context, string html)
        {
            ParseHtml(html);
            RegisterDocument(context);
        }

        // ------------------------------------------------------------------
        //  HTML parsing helpers
        // ------------------------------------------------------------------

        private static readonly Regex TitlePattern = new(
            @"<title[^>]*>(?<content>[\s\S]*?)</title>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ElementPattern = new(
            @"<(?<tag>[a-zA-Z][a-zA-Z0-9]*)\b(?<attrs>[^>]*)>(?<inner>[\s\S]*?)</\k<tag>>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex IdPattern = new(
            @"\bid\s*=\s*[""'](?<id>[^""']+)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ClassPattern = new(
            @"\bclass\s*=\s*[""'](?<cls>[^""']+)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private void ParseHtml(string html)
        {
            _elements.Clear();

            // Extract <title>
            var titleMatch = TitlePattern.Match(html);
            _title = titleMatch.Success ? titleMatch.Groups["content"].Value.Trim() : string.Empty;

            // Extract elements with id or class attributes
            foreach (Match m in ElementPattern.Matches(html))
            {
                var tag = m.Groups["tag"].Value.ToLowerInvariant();
                var attrs = m.Groups["attrs"].Value;
                var inner = m.Groups["inner"].Value.Trim();

                var idMatch = IdPattern.Match(attrs);
                var classMatch = ClassPattern.Match(attrs);

                _elements.Add(new DomElement(
                    tag,
                    idMatch.Success ? idMatch.Groups["id"].Value : null,
                    classMatch.Success ? classMatch.Groups["cls"].Value : null,
                    inner));
            }
        }

        // ------------------------------------------------------------------
        //  JavaScript bridge
        // ------------------------------------------------------------------

        private void RegisterDocument(JSContext context)
        {
            var document = new JSObject();

            // document.title (getter / setter)
            document.FastAddProperty(
                (KeyString)"title",
                new JSFunction((in Arguments a) => new JSString(_title), "get title"),
                new JSFunction((in Arguments a) =>
                {
                    _title = a.Length > 0 ? a[0].ToString() : string.Empty;
                    return JSUndefined.Value;
                }, "set title"),
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // document.getElementById(id)
            document.FastAddValue(
                (KeyString)"getElementById",
                new JSFunction((in Arguments a) =>
                {
                    var id = a.Length > 0 ? a[0].ToString() : string.Empty;
                    foreach (var el in _elements)
                    {
                        if (el.Id == id)
                            return ToJSObject(el);
                    }
                    return JSNull.Value;
                }, "getElementById", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // document.getElementsByTagName(tag)
            document.FastAddValue(
                (KeyString)"getElementsByTagName",
                new JSFunction((in Arguments a) =>
                {
                    var tag = a.Length > 0 ? a[0].ToString().ToLowerInvariant() : string.Empty;
                    var results = new List<JSValue>();
                    foreach (var el in _elements)
                    {
                        if (el.TagName == tag)
                            results.Add(ToJSObject(el));
                    }
                    return new JSArray(results);
                }, "getElementsByTagName", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // document.createElement(tag)
            document.FastAddValue(
                (KeyString)"createElement",
                new JSFunction((in Arguments a) =>
                {
                    var tag = a.Length > 0 ? a[0].ToString().ToLowerInvariant() : "div";
                    var el = new DomElement(tag, null, null, string.Empty);
                    _elements.Add(el);
                    return ToJSObject(el);
                }, "createElement", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            context["document"] = document;
        }

        private static JSObject ToJSObject(DomElement element)
        {
            var obj = new JSObject();

            obj.FastAddValue(
                (KeyString)"tagName",
                new JSString(element.TagName.ToUpperInvariant()),
                JSPropertyAttributes.EnumerableConfigurableValue);

            obj.FastAddValue(
                (KeyString)"id",
                element.Id != null ? new JSString(element.Id) : (JSValue)JSNull.Value,
                JSPropertyAttributes.EnumerableConfigurableValue);

            obj.FastAddValue(
                (KeyString)"className",
                element.ClassName != null ? new JSString(element.ClassName) : (JSValue)JSNull.Value,
                JSPropertyAttributes.EnumerableConfigurableValue);

            obj.FastAddValue(
                (KeyString)"innerHTML",
                new JSString(element.InnerHtml),
                JSPropertyAttributes.EnumerableConfigurableValue);

            return obj;
        }
    }

    /// <summary>
    /// Lightweight representation of an HTML element for the DOM bridge.
    /// </summary>
    public sealed class DomElement
    {
        public string TagName { get; }
        public string? Id { get; }
        public string? ClassName { get; }
        public string InnerHtml { get; }

        public DomElement(string tagName, string? id, string? className, string innerHtml)
        {
            TagName = tagName;
            Id = id;
            ClassName = className;
            InnerHtml = innerHtml;
        }
    }
}

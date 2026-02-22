using System.Collections.Generic;
using System.Linq;
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

        private static readonly Regex OpenTagPattern = new(
            @"<(?<tag>[a-zA-Z][a-zA-Z0-9]*)\b(?<attrs>[^>]*)\/?>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly System.Collections.Generic.HashSet<string> SkippedTags = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "html", "head", "body", "title"
        };

        private static readonly System.Collections.Generic.HashSet<string> VoidTags = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        };

        private static readonly Regex IdPattern = new(
            @"\bid\s*=\s*[""'](?<id>[^""']+)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ClassPattern = new(
            @"\bclass\s*=\s*[""'](?<cls>[^""']+)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AttributeSelectorPattern = new(
            @"\[(?<name>[^\]=~*$^|]+?)(?:=(?<value>[^\]]*))?\]",
            RegexOptions.Compiled);

        private void ParseHtml(string html)
        {
            _elements.Clear();

            // Extract <title>
            var titleMatch = TitlePattern.Match(html);
            _title = titleMatch.Success ? titleMatch.Groups["content"].Value.Trim() : string.Empty;

            // Scan all opening tags and collect elements
            foreach (Match m in OpenTagPattern.Matches(html))
            {
                var tag = m.Groups["tag"].Value.ToLowerInvariant();
                if (SkippedTags.Contains(tag)) continue;

                var attrs = m.Groups["attrs"].Value;
                var isSelfClosing = m.Value.EndsWith("/>");

                string inner = string.Empty;
                if (!VoidTags.Contains(tag) && !isSelfClosing)
                {
                    var closeTag = $"</{tag}>";
                    var closeIdx = html.IndexOf(closeTag, m.Index + m.Length, System.StringComparison.OrdinalIgnoreCase);
                    if (closeIdx >= 0)
                    {
                        inner = html.Substring(m.Index + m.Length, closeIdx - (m.Index + m.Length)).Trim();
                    }
                }

                var idMatch = IdPattern.Match(attrs);
                var classMatch = ClassPattern.Match(attrs);

                var attributes = ParseAttributes(attrs);
                var style = attributes.TryGetValue("style", out var styleVal)
                    ? ParseStyle(styleVal)
                    : new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

                _elements.Add(new DomElement(
                    tag,
                    idMatch.Success ? idMatch.Groups["id"].Value : null,
                    classMatch.Success ? classMatch.Groups["cls"].Value : null,
                    inner,
                    style,
                    attributes));
            }
        }

        /// <summary>
        /// Parses all HTML attribute name-value pairs from an attribute string.
        /// Handles quoted values (<c>"…"</c> or <c>'…'</c>), unquoted values,
        /// and boolean attributes.
        /// </summary>
        private static Dictionary<string, string> ParseAttributes(string attrs)
        {
            var result = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            var i = 0;
            while (i < attrs.Length)
            {
                while (i < attrs.Length && char.IsWhiteSpace(attrs[i])) i++;
                if (i >= attrs.Length) break;

                var nameStart = i;
                while (i < attrs.Length && attrs[i] != '=' && !char.IsWhiteSpace(attrs[i]) && attrs[i] != '>') i++;
                if (i == nameStart) { i++; continue; }
                var name = attrs[nameStart..i].Trim('/');

                while (i < attrs.Length && char.IsWhiteSpace(attrs[i])) i++;

                if (i >= attrs.Length || attrs[i] != '=')
                {
                    if (!string.IsNullOrEmpty(name))
                        result[name] = name;
                    continue;
                }
                i++; // skip '='

                while (i < attrs.Length && char.IsWhiteSpace(attrs[i])) i++;

                string value;
                if (i < attrs.Length && (attrs[i] == '"' || attrs[i] == '\''))
                {
                    var quote = attrs[i++];
                    var valueStart = i;
                    while (i < attrs.Length && attrs[i] != quote) i++;
                    value = attrs[valueStart..i];
                    if (i < attrs.Length) i++;
                }
                else
                {
                    var valueStart = i;
                    while (i < attrs.Length && !char.IsWhiteSpace(attrs[i]) && attrs[i] != '>') i++;
                    value = attrs[valueStart..i];
                }

                if (!string.IsNullOrEmpty(name))
                    result[name] = value;
            }
            return result;
        }

        /// <summary>
        /// Parses a CSS inline style string (e.g. <c>"color: red; font-size: 12px"</c>)
        /// into a property→value dictionary.
        /// </summary>
        private static Dictionary<string, string> ParseStyle(string styleValue)
        {
            var result = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var declaration in styleValue.Split(';'))
            {
                var colonIdx = declaration.IndexOf(':');
                if (colonIdx > 0)
                {
                    var prop = declaration[..colonIdx].Trim();
                    var val = declaration[(colonIdx + 1)..].Trim();
                    if (!string.IsNullOrEmpty(prop))
                        result[prop] = val;
                }
            }
            return result;
        }

        // ------------------------------------------------------------------
        //  CSS selector matching
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns <c>true</c> when <paramref name="el"/> matches the given simple
        /// compound CSS selector.  Supported tokens: tag type, <c>#id</c>,
        /// <c>.class</c> (multiple), <c>[attr]</c>, and <c>[attr=value]</c>.
        /// Descendant/sibling combinators are not supported.
        /// </summary>
        private static bool MatchesSelector(DomElement el, string selector)
        {
            selector = selector.Trim();
            if (string.IsNullOrEmpty(selector)) return false;

            // Extract and remove [attr] / [attr=value] tokens
            var attrFilters = new List<(string Name, string? Value)>();
            selector = AttributeSelectorPattern.Replace(selector, m =>
            {
                var name = m.Groups["name"].Value.Trim();
                var value = m.Groups["value"].Success
                    ? m.Groups["value"].Value.Trim().Trim('"', '\'')
                    : null;
                attrFilters.Add((name, value));
                return string.Empty;
            });

            string? tagFilter = null;
            string? idFilter = null;
            var classFilters = new List<string>();

            var pos = 0;
            while (pos < selector.Length)
            {
                char c = selector[pos];
                if (c == '#')
                {
                    pos++;
                    var start = pos;
                    while (pos < selector.Length && selector[pos] != '.' && selector[pos] != '#') pos++;
                    idFilter = selector[start..pos];
                }
                else if (c == '.')
                {
                    pos++;
                    var start = pos;
                    while (pos < selector.Length && selector[pos] != '.' && selector[pos] != '#') pos++;
                    classFilters.Add(selector[start..pos]);
                }
                else if (char.IsLetter(c) || c == '*')
                {
                    var start = pos;
                    while (pos < selector.Length && selector[pos] != '.' && selector[pos] != '#') pos++;
                    var tag = selector[start..pos].ToLowerInvariant();
                    if (tag != "*")
                        tagFilter = tag;
                }
                else
                {
                    pos++;
                }
            }

            if (tagFilter != null && !string.Equals(el.TagName, tagFilter, System.StringComparison.OrdinalIgnoreCase)) return false;
            if (idFilter != null && !string.Equals(el.Id, idFilter, System.StringComparison.Ordinal)) return false;

            if (classFilters.Count > 0)
            {
                var elementClasses = new System.Collections.Generic.HashSet<string>(
                    (el.ClassName ?? string.Empty).Split(' ').Where(s => s.Length > 0),
                    System.StringComparer.Ordinal);
                foreach (var cls in classFilters)
                    if (!elementClasses.Contains(cls)) return false;
            }

            foreach (var (name, value) in attrFilters)
            {
                if (!el.Attributes.TryGetValue(name, out var attrVal)) return false;
                if (value != null && attrVal != value) return false;
            }

            return true;
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

            // document.getElementsByClassName(className)
            document.FastAddValue(
                (KeyString)"getElementsByClassName",
                new JSFunction((in Arguments a) =>
                {
                    var className = a.Length > 0 ? a[0].ToString() : string.Empty;
                    var results = new List<JSValue>();
                    foreach (var el in _elements)
                    {
                        var classes = new System.Collections.Generic.HashSet<string>(
                            (el.ClassName ?? string.Empty).Split(' ').Where(s => s.Length > 0),
                            System.StringComparer.Ordinal);
                        if (classes.Contains(className))
                            results.Add(ToJSObject(el));
                    }
                    return new JSArray(results);
                }, "getElementsByClassName", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // document.querySelector(selector)
            document.FastAddValue(
                (KeyString)"querySelector",
                new JSFunction((in Arguments a) =>
                {
                    var selector = a.Length > 0 ? a[0].ToString() : string.Empty;
                    foreach (var el in _elements)
                    {
                        if (MatchesSelector(el, selector))
                            return ToJSObject(el);
                    }
                    return JSNull.Value;
                }, "querySelector", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // document.querySelectorAll(selector)
            document.FastAddValue(
                (KeyString)"querySelectorAll",
                new JSFunction((in Arguments a) =>
                {
                    var selector = a.Length > 0 ? a[0].ToString() : string.Empty;
                    var results = new List<JSValue>();
                    foreach (var el in _elements)
                    {
                        if (MatchesSelector(el, selector))
                            results.Add(ToJSObject(el));
                    }
                    return new JSArray(results);
                }, "querySelectorAll", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // document.createElement(tag)
            document.FastAddValue(
                (KeyString)"createElement",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length == 0)
                        throw new JSException("Failed to execute 'createElement': 1 argument required, but only 0 present.");
                    var tag = a[0].ToString().ToLowerInvariant();
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

            // className (read/write)
            obj.FastAddProperty(
                (KeyString)"className",
                new JSFunction((in Arguments a) =>
                    element.ClassName != null ? (JSValue)new JSString(element.ClassName) : JSNull.Value,
                    "get className"),
                new JSFunction((in Arguments a) =>
                {
                    element.ClassName = a.Length > 0 ? a[0].ToString() : string.Empty;
                    return JSUndefined.Value;
                }, "set className"),
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // innerHTML (read/write)
            obj.FastAddProperty(
                (KeyString)"innerHTML",
                new JSFunction((in Arguments a) => new JSString(element.InnerHtml), "get innerHTML"),
                new JSFunction((in Arguments a) =>
                {
                    element.InnerHtml = a.Length > 0 ? a[0].ToString() : string.Empty;
                    return JSUndefined.Value;
                }, "set innerHTML"),
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // style object — CSS property access and manipulation
            obj.FastAddValue(
                (KeyString)"style",
                BuildStyleObject(element),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // classList — class list manipulation
            obj.FastAddValue(
                (KeyString)"classList",
                BuildClassListObject(element),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // setAttribute(name, value)
            obj.FastAddValue(
                (KeyString)"setAttribute",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length >= 2)
                        element.Attributes[a[0].ToString()] = a[1].ToString();
                    return JSUndefined.Value;
                }, "setAttribute", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // getAttribute(name)
            obj.FastAddValue(
                (KeyString)"getAttribute",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length == 0) return JSNull.Value;
                    var name = a[0].ToString();
                    return element.Attributes.TryGetValue(name, out var val)
                        ? (JSValue)new JSString(val)
                        : JSNull.Value;
                }, "getAttribute", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            return obj;
        }

        /// <summary>
        /// Builds a <c>style</c> object exposing <c>cssText</c>,
        /// <c>setProperty</c>, <c>getPropertyValue</c>, and <c>removeProperty</c>.
        /// </summary>
        private static JSObject BuildStyleObject(DomElement element)
        {
            var style = new JSObject();

            // style.cssText (getter / setter)
            style.FastAddProperty(
                (KeyString)"cssText",
                new JSFunction((in Arguments a) =>
                {
                    var parts = element.Style.Select(kv => $"{kv.Key}: {kv.Value}");
                    var text = string.Join("; ", parts);
                    return new JSString(text.Length > 0 ? text + ";" : text);
                }, "get cssText"),
                new JSFunction((in Arguments a) =>
                {
                    element.Style.Clear();
                    if (a.Length > 0)
                    {
                        foreach (var kv in ParseStyle(a[0].ToString()))
                            element.Style[kv.Key] = kv.Value;
                    }
                    return JSUndefined.Value;
                }, "set cssText"),
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // style.setProperty(property, value)
            style.FastAddValue(
                (KeyString)"setProperty",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length >= 2)
                        element.Style[a[0].ToString()] = a[1].ToString();
                    return JSUndefined.Value;
                }, "setProperty", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // style.getPropertyValue(property)
            style.FastAddValue(
                (KeyString)"getPropertyValue",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length > 0 && element.Style.TryGetValue(a[0].ToString(), out var val))
                        return new JSString(val);
                    return new JSString(string.Empty);
                }, "getPropertyValue", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // style.removeProperty(property)
            style.FastAddValue(
                (KeyString)"removeProperty",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length > 0)
                    {
                        var prop = a[0].ToString();
                        var removed = element.Style.TryGetValue(prop, out var val) ? val : string.Empty;
                        element.Style.Remove(prop);
                        return new JSString(removed);
                    }
                    return new JSString(string.Empty);
                }, "removeProperty", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            return style;
        }

        /// <summary>
        /// Builds a <c>classList</c> object exposing <c>add</c>, <c>remove</c>,
        /// <c>toggle</c>, and <c>contains</c>.
        /// </summary>
        private static JSObject BuildClassListObject(DomElement element)
        {
            var classList = new JSObject();

            // classList.contains(className)
            classList.FastAddValue(
                (KeyString)"contains",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length == 0) return JSBoolean.False;
                    var cls = a[0].ToString();
                    var classes = new System.Collections.Generic.HashSet<string>(
                        (element.ClassName ?? string.Empty).Split(' ').Where(s => s.Length > 0),
                        System.StringComparer.Ordinal);
                    return classes.Contains(cls) ? JSBoolean.True : JSBoolean.False;
                }, "contains", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // classList.add(...classNames)
            classList.FastAddValue(
                (KeyString)"add",
                new JSFunction((in Arguments a) =>
                {
                    var classes = (element.ClassName ?? string.Empty)
                        .Split(' ').Where(s => s.Length > 0).ToList();
                    var classSet = new System.Collections.Generic.HashSet<string>(classes, System.StringComparer.Ordinal);
                    for (var i = 0; i < a.Length; i++)
                    {
                        var cls = a[i].ToString();
                        if (!string.IsNullOrEmpty(cls) && classSet.Add(cls))
                            classes.Add(cls);
                    }
                    element.ClassName = string.Join(" ", classes);
                    return JSUndefined.Value;
                }, "add"),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // classList.remove(...classNames)
            classList.FastAddValue(
                (KeyString)"remove",
                new JSFunction((in Arguments a) =>
                {
                    var toRemove = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
                    for (var i = 0; i < a.Length; i++)
                        toRemove.Add(a[i].ToString());
                    var classes = (element.ClassName ?? string.Empty)
                        .Split(' ').Where(s => s.Length > 0 && !toRemove.Contains(s)).ToList();
                    element.ClassName = string.Join(" ", classes);
                    return JSUndefined.Value;
                }, "remove"),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // classList.toggle(className[, force])
            classList.FastAddValue(
                (KeyString)"toggle",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length == 0) return JSBoolean.False;
                    var cls = a[0].ToString();
                    var classes = (element.ClassName ?? string.Empty)
                        .Split(' ').Where(s => s.Length > 0).ToList();
                    var classSet = new System.Collections.Generic.HashSet<string>(classes, System.StringComparer.Ordinal);

                    bool shouldAdd = a.Length >= 2 && !(a[1] is JSUndefined)
                        ? a[1].BooleanValue
                        : !classSet.Contains(cls);

                    if (shouldAdd)
                    {
                        if (classSet.Add(cls)) classes.Add(cls);
                        element.ClassName = string.Join(" ", classes);
                        return JSBoolean.True;
                    }
                    else
                    {
                        classes.Remove(cls);
                        element.ClassName = string.Join(" ", classes);
                        return JSBoolean.False;
                    }
                }, "toggle", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            return classList;
        }
    }

    /// <summary>
    /// Lightweight representation of an HTML element for the DOM bridge.
    /// </summary>
    public sealed class DomElement
    {
        public string TagName { get; }
        public string? Id { get; }

        /// <summary>The element's CSS class string; mutable via <c>classList</c> or <c>className</c>.</summary>
        public string? ClassName { get; set; }

        /// <summary>The element's inner HTML content; mutable via the <c>innerHTML</c> setter.</summary>
        public string InnerHtml { get; set; }

        /// <summary>Parsed inline CSS style declarations, keyed case-insensitively by property name.</summary>
        public Dictionary<string, string> Style { get; }

        /// <summary>All HTML attributes of the element, keyed case-insensitively by attribute name.</summary>
        public Dictionary<string, string> Attributes { get; }

        public DomElement(
            string tagName,
            string? id,
            string? className,
            string innerHtml,
            Dictionary<string, string>? style = null,
            Dictionary<string, string>? attributes = null)
        {
            TagName = tagName;
            Id = id;
            ClassName = className;
            InnerHtml = innerHtml;
            Style = style ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            Attributes = attributes ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        // window.location fields
        private string _pageUrl = string.Empty;
        private string _pageProtocol = string.Empty;
        private string _pageHost = string.Empty;
        private string _pageHostName = string.Empty;
        private string _pagePathName = "/";
        private string _pageSearch = string.Empty;
        private string _pageHash = string.Empty;
        private string _pageOrigin = string.Empty;

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

        /// <summary>
        /// Parse the supplied <paramref name="html"/> and register a
        /// <c>document</c> global on the given <paramref name="context"/>,
        /// with the page URL available via <c>window.location</c>.
        /// </summary>
        public void Attach(JSContext context, string html, string url)
        {
            if (System.Uri.TryCreate(url, System.UriKind.Absolute, out var uri))
            {
                _pageUrl = uri.ToString();
                _pageProtocol = uri.Scheme + ":";
                _pageHost = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
                _pageHostName = uri.Host;
                _pagePathName = uri.AbsolutePath;
                _pageSearch = uri.Query;
                _pageHash = uri.Fragment;
                _pageOrigin = $"{uri.Scheme}://{(uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}")}";
            }
            else
            {
                _pageUrl = url;
            }
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
            _jsObjectCache.Clear();

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

            // Extract <style> blocks and apply cascaded styles
            ExtractStyleBlocks(html);
            ApplyCascadedStyles();
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
        //  CSS specificity (Level 3) and <style> / <link> cascading
        // ------------------------------------------------------------------

        private static readonly Regex StyleTagPattern = new(
            @"<style[^>]*>(?<content>[\s\S]*?)</style>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CssRulePattern = new(
            @"(?<selector>[^{}@]+)\{(?<declarations>[^}]*)\}",
            RegexOptions.Compiled);

        private static readonly Regex MediaQueryPattern = new(
            @"@media\s+(?<query>[^{]+)\{(?<content>(?:[^{}]|\{[^}]*\})*)\}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Parsed CSS rules extracted from <c>&lt;style&gt;</c> blocks, stored as
        /// (selector, specificity, declarations) triples.
        /// </summary>
        private readonly List<(string Selector, int Specificity, Dictionary<string, string> Declarations)> _cssRules = new();

        /// <summary>Parsed CSS rules from embedded style blocks.</summary>
        public IReadOnlyList<(string Selector, int Specificity, Dictionary<string, string> Declarations)> CssRules => _cssRules;

        /// <summary>
        /// Calculates CSS Specificity (Level 3) for a simple selector.
        /// Returns a single integer encoding (a, b, c) where a = ID selectors,
        /// b = class / attribute / pseudo-class selectors, c = type selectors.
        /// Inline styles use specificity 1000 (handled externally).
        /// </summary>
        public static int CalculateSpecificity(string selector)
        {
            int a = 0, b = 0, c = 0;
            var s = selector.Trim();

            // Remove attribute selectors and count them
            s = AttributeSelectorPattern.Replace(s, m => { b++; return string.Empty; });

            foreach (var ch in s)
            {
                if (ch == '#') a++;
                else if (ch == '.') b++;
            }

            // Count type selectors: letter-only tokens not preceded by # or .
            var pos = 0;
            while (pos < s.Length)
            {
                if (s[pos] == '#' || s[pos] == '.')
                {
                    pos++;
                    while (pos < s.Length && s[pos] != '.' && s[pos] != '#' && !char.IsWhiteSpace(s[pos])) pos++;
                }
                else if (char.IsLetter(s[pos]))
                {
                    var start = pos;
                    while (pos < s.Length && s[pos] != '.' && s[pos] != '#' && !char.IsWhiteSpace(s[pos])) pos++;
                    var token = s[start..pos].ToLowerInvariant();
                    if (token != "*") c++;
                }
                else
                {
                    pos++;
                }
            }

            return a * 100 + b * 10 + c;
        }

        /// <summary>
        /// Extracts CSS rules from all <c>&lt;style&gt;</c> blocks in the HTML source
        /// and stores them in <see cref="_cssRules"/> ordered by specificity.
        /// </summary>
        private void ExtractStyleBlocks(string html)
        {
            _cssRules.Clear();

            foreach (Match styleMatch in StyleTagPattern.Matches(html))
            {
                var cssText = styleMatch.Groups["content"].Value;
                ParseCssText(cssText);
            }

            _cssRules.Sort((x, y) => x.Specificity.CompareTo(y.Specificity));
        }

        /// <summary>
        /// Parses raw CSS text into rules, handling <c>@media</c> queries.
        /// Rules inside <c>@media screen</c> are included; <c>@media print</c> rules are skipped.
        /// </summary>
        private void ParseCssText(string cssText)
        {
            var remaining = MediaQueryPattern.Replace(cssText, m =>
            {
                var query = m.Groups["query"].Value.Trim();
                var content = m.Groups["content"].Value;

                if (query.Contains("screen", System.StringComparison.OrdinalIgnoreCase) ||
                    query.Equals("all", System.StringComparison.OrdinalIgnoreCase))
                {
                    ExtractRulesFromCss(content);
                }
                return string.Empty;
            });

            ExtractRulesFromCss(remaining);
        }

        private void ExtractRulesFromCss(string css)
        {
            foreach (Match ruleMatch in CssRulePattern.Matches(css))
            {
                var selectorGroup = ruleMatch.Groups["selector"].Value.Trim();
                var declarations = ParseStyle(ruleMatch.Groups["declarations"].Value);

                foreach (var sel in selectorGroup.Split(','))
                {
                    var selector = sel.Trim();
                    if (string.IsNullOrEmpty(selector)) continue;
                    var specificity = CalculateSpecificity(selector);
                    _cssRules.Add((selector, specificity, declarations));
                }
            }
        }

        /// <summary>
        /// Applies cascaded style rules to all parsed elements, following CSS specificity order.
        /// Inline styles (specificity 1000) always win.
        /// </summary>
        private void ApplyCascadedStyles()
        {
            foreach (var el in _elements)
            {
                foreach (var (selector, _, declarations) in _cssRules)
                {
                    if (MatchesSelector(el, selector))
                    {
                        foreach (var kv in declarations)
                        {
                            if (!el.Attributes.TryGetValue("style", out var inlineStyle) ||
                                !inlineStyle.Contains(kv.Key, System.StringComparison.OrdinalIgnoreCase))
                            {
                                el.Style[kv.Key] = kv.Value;
                            }
                        }
                    }
                }
            }
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

        private readonly DomElement _documentElement = new("html", null, null, string.Empty);

        /// <summary>
        /// The element backing <c>document.documentElement</c> (the &lt;html&gt; element).
        /// </summary>
        public DomElement DocumentElement => _documentElement;

        private void RegisterDocument(JSContext context)
        {
            var document = new JSObject();

            // document.documentElement (the <html> element)
            document.FastAddValue(
                (KeyString)"documentElement",
                ToJSObject(_documentElement),
                JSPropertyAttributes.EnumerableConfigurableValue);

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

            // document.createTextNode(text)
            document.FastAddValue(
                (KeyString)"createTextNode",
                new JSFunction((in Arguments a) =>
                {
                    var text = a.Length > 0 ? a[0].ToString() : string.Empty;
                    var el = new DomElement("#text", null, null, string.Empty, isTextNode: true);
                    el.TextContent = text;
                    _elements.Add(el);
                    return ToJSObject(el);
                }, "createTextNode", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            context["document"] = document;

            // window global
            var window = new JSObject();
            window.FastAddValue(
                (KeyString)"document",
                document,
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.localStorage — in-memory stub backed by a plain JSObject
            window.FastAddValue(
                (KeyString)"localStorage",
                BuildLocalStorageObject(),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.matchMedia(query) — stub that always returns { matches: false }
            window.FastAddValue(
                (KeyString)"matchMedia",
                new JSFunction((in Arguments a) =>
                {
                    var result = new JSObject();
                    result.FastAddValue(
                        (KeyString)"matches",
                        JSBoolean.False,
                        JSPropertyAttributes.EnumerableConfigurableValue);
                    result.FastAddValue(
                        (KeyString)"media",
                        a.Length > 0 ? (JSValue)new JSString(a[0].ToString()) : new JSString(string.Empty),
                        JSPropertyAttributes.EnumerableConfigurableValue);
                    return result;
                }, "matchMedia", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.location (read-only)
            var location = new JSObject();
            location.FastAddValue((KeyString)"href", new JSString(_pageUrl), JSPropertyAttributes.EnumerableConfigurableValue);
            location.FastAddValue((KeyString)"protocol", new JSString(_pageProtocol), JSPropertyAttributes.EnumerableConfigurableValue);
            location.FastAddValue((KeyString)"host", new JSString(_pageHost), JSPropertyAttributes.EnumerableConfigurableValue);
            location.FastAddValue((KeyString)"hostname", new JSString(_pageHostName), JSPropertyAttributes.EnumerableConfigurableValue);
            location.FastAddValue((KeyString)"pathname", new JSString(_pagePathName), JSPropertyAttributes.EnumerableConfigurableValue);
            location.FastAddValue((KeyString)"search", new JSString(_pageSearch), JSPropertyAttributes.EnumerableConfigurableValue);
            location.FastAddValue((KeyString)"hash", new JSString(_pageHash), JSPropertyAttributes.EnumerableConfigurableValue);
            location.FastAddValue((KeyString)"origin", new JSString(_pageOrigin), JSPropertyAttributes.EnumerableConfigurableValue);
            window.FastAddValue(
                (KeyString)"location",
                location,
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.setTimeout(fn, delay) — single-threaded; invokes callback immediately
            var timerIdCounter = 0;
            window.FastAddValue(
                (KeyString)"setTimeout",
                new JSFunction((in Arguments a) =>
                {
                    var id = ++timerIdCounter;
                    if (a.Length > 0 && a[0] is JSFunction fn)
                    {
                        try { fn.InvokeFunction(new Arguments(JSUndefined.Value)); }
                        catch { /* swallow timer callback errors */ }
                    }
                    return new JSNumber(id);
                }, "setTimeout", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.clearTimeout(id) — no-op (timers fire immediately)
            window.FastAddValue(
                (KeyString)"clearTimeout",
                new JSFunction((in Arguments a) => JSUndefined.Value, "clearTimeout", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.setInterval(fn, delay) — returns id; single invocation
            window.FastAddValue(
                (KeyString)"setInterval",
                new JSFunction((in Arguments a) =>
                {
                    var id = ++timerIdCounter;
                    if (a.Length > 0 && a[0] is JSFunction fn)
                    {
                        try { fn.InvokeFunction(new Arguments(JSUndefined.Value)); }
                        catch { /* swallow timer callback errors */ }
                    }
                    return new JSNumber(id);
                }, "setInterval", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.clearInterval(id) — no-op
            window.FastAddValue(
                (KeyString)"clearInterval",
                new JSFunction((in Arguments a) => JSUndefined.Value, "clearInterval", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // window.alert(msg) — logs to debug output
            window.FastAddValue(
                (KeyString)"alert",
                new JSFunction((in Arguments a) =>
                {
                    var msg = a.Length > 0 ? a[0].ToString() : string.Empty;
                    System.Diagnostics.Debug.WriteLine($"[alert] {msg}");
                    return JSUndefined.Value;
                }, "alert", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // console object (shared between window.console and global console)
            var console = BuildConsoleObject();
            window.FastAddValue(
                (KeyString)"console",
                console,
                JSPropertyAttributes.EnumerableConfigurableValue);

            // fetch(url, options) — basic polyfill backed by HttpClient
            var fetchFn = new JSFunction((in Arguments a) =>
            {
                if (a.Length == 0)
                    throw new JSException("Failed to execute 'fetch': 1 argument required.");

                var fetchUrl = a[0].ToString();
                var responseObj = new JSObject();

                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    var response = httpClient.GetAsync(fetchUrl).GetAwaiter().GetResult();
                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var statusCode = (int)response.StatusCode;

                    responseObj.FastAddValue((KeyString)"ok", response.IsSuccessStatusCode ? JSBoolean.True : JSBoolean.False, JSPropertyAttributes.EnumerableConfigurableValue);
                    responseObj.FastAddValue((KeyString)"status", new JSNumber(statusCode), JSPropertyAttributes.EnumerableConfigurableValue);
                    responseObj.FastAddValue((KeyString)"statusText", new JSString(response.ReasonPhrase ?? string.Empty), JSPropertyAttributes.EnumerableConfigurableValue);

                    // response.text() — returns a thenable with the body text
                    responseObj.FastAddValue((KeyString)"text", new JSFunction((in Arguments _) =>
                    {
                        var thenable = new JSObject();
                        thenable.FastAddValue((KeyString)"then", new JSFunction((in Arguments thenArgs) =>
                        {
                            if (thenArgs.Length > 0 && thenArgs[0] is JSFunction cb)
                            {
                                try { cb.InvokeFunction(new Arguments(cb, new JSString(body))); }
                                catch { /* swallow */ }
                            }
                            return thenable;
                        }, "then", 1), JSPropertyAttributes.EnumerableConfigurableValue);
                        return thenable;
                    }, "text", 0), JSPropertyAttributes.EnumerableConfigurableValue);

                    // response.json() — returns a thenable with parsed JSON
                    responseObj.FastAddValue((KeyString)"json", new JSFunction((in Arguments jsonArgs) =>
                    {
                        var thenable = new JSObject();
                        thenable.FastAddValue((KeyString)"then", new JSFunction((in Arguments thenArgs) =>
                        {
                            if (thenArgs.Length > 0 && thenArgs[0] is JSFunction cb)
                            {
                                try
                                {
                                    var escaped = body.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
                                    var parsed = context.Eval($"JSON.parse(\"{escaped}\")");
                                    cb.InvokeFunction(new Arguments(cb, parsed));
                                }
                                catch { /* swallow parse errors */ }
                            }
                            return thenable;
                        }, "then", 1), JSPropertyAttributes.EnumerableConfigurableValue);
                        return thenable;
                    }, "json", 0), JSPropertyAttributes.EnumerableConfigurableValue);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[fetch] Error: {ex.Message}");
                    responseObj.FastAddValue((KeyString)"ok", JSBoolean.False, JSPropertyAttributes.EnumerableConfigurableValue);
                    responseObj.FastAddValue((KeyString)"status", new JSNumber(0), JSPropertyAttributes.EnumerableConfigurableValue);
                    responseObj.FastAddValue((KeyString)"statusText", new JSString(ex.Message), JSPropertyAttributes.EnumerableConfigurableValue);
                }

                // Return a thenable (Promise-like) that resolves immediately
                var promise = new JSObject();
                promise.FastAddValue((KeyString)"then", new JSFunction((in Arguments thenArgs) =>
                {
                    if (thenArgs.Length > 0 && thenArgs[0] is JSFunction cb)
                    {
                        try { cb.InvokeFunction(new Arguments(cb, responseObj)); }
                        catch { /* swallow */ }
                    }
                    return promise;
                }, "then", 1), JSPropertyAttributes.EnumerableConfigurableValue);
                promise.FastAddValue((KeyString)"catch", new JSFunction((in Arguments _) => promise, "catch", 1), JSPropertyAttributes.EnumerableConfigurableValue);
                return promise;
            }, "fetch", 1);

            window.FastAddValue((KeyString)"fetch", fetchFn, JSPropertyAttributes.EnumerableConfigurableValue);

            // XMLHttpRequest — basic polyfill backed by HttpClient
            RegisterXMLHttpRequest(context);

            context["window"] = window;
            context["console"] = console;
            context["fetch"] = fetchFn;
        }

        /// <summary>
        /// Registers a basic <c>XMLHttpRequest</c> constructor on the context.
        /// Supports <c>open</c>, <c>send</c>, <c>setRequestHeader</c>,
        /// <c>onreadystatechange</c>, <c>readyState</c>, <c>status</c>, and <c>responseText</c>.
        /// </summary>
        private static void RegisterXMLHttpRequest(JSContext context)
        {
            context.Eval(@"
                function XMLHttpRequest() {
                    this.readyState = 0;
                    this.status = 0;
                    this.statusText = '';
                    this.responseText = '';
                    this.onreadystatechange = null;
                    this._method = 'GET';
                    this._url = '';
                    this._headers = {};
                    this.UNSENT = 0;
                    this.OPENED = 1;
                    this.HEADERS_RECEIVED = 2;
                    this.LOADING = 3;
                    this.DONE = 4;
                }
                XMLHttpRequest.prototype.open = function(method, url) {
                    this._method = method;
                    this._url = url;
                    this.readyState = 1;
                };
                XMLHttpRequest.prototype.setRequestHeader = function(name, value) {
                    this._headers[name] = value;
                };
                XMLHttpRequest.prototype.send = function(body) {
                    var self = this;
                    try {
                        fetch(self._url).then(function(response) {
                            self.status = response.status;
                            self.statusText = response.statusText;
                            self.readyState = 2;
                            response.text().then(function(text) {
                                self.responseText = text;
                                self.readyState = 4;
                                if (typeof self.onreadystatechange === 'function') {
                                    self.onreadystatechange();
                                }
                            });
                        });
                    } catch(e) {
                        self.readyState = 4;
                        self.status = 0;
                        if (typeof self.onreadystatechange === 'function') {
                            self.onreadystatechange();
                        }
                    }
                };
            ");
        }

        /// <summary>
        /// Builds a <c>console</c> object exposing <c>log</c>, <c>warn</c>,
        /// <c>error</c>, and <c>info</c>.
        /// </summary>
        private static JSObject BuildConsoleObject()
        {
            var console = new JSObject();

            console.FastAddValue(
                (KeyString)"log",
                new JSFunction((in Arguments a) =>
                {
                    var parts = new List<string>();
                    for (var i = 0; i < a.Length; i++)
                        parts.Add(a[i]?.ToString() ?? "undefined");
                    System.Diagnostics.Debug.WriteLine($"[console.log] {string.Join(" ", parts)}");
                    return JSUndefined.Value;
                }, "log"),
                JSPropertyAttributes.EnumerableConfigurableValue);

            console.FastAddValue(
                (KeyString)"warn",
                new JSFunction((in Arguments a) =>
                {
                    var parts = new List<string>();
                    for (var i = 0; i < a.Length; i++)
                        parts.Add(a[i]?.ToString() ?? "undefined");
                    System.Diagnostics.Debug.WriteLine($"[console.warn] {string.Join(" ", parts)}");
                    return JSUndefined.Value;
                }, "warn"),
                JSPropertyAttributes.EnumerableConfigurableValue);

            console.FastAddValue(
                (KeyString)"error",
                new JSFunction((in Arguments a) =>
                {
                    var parts = new List<string>();
                    for (var i = 0; i < a.Length; i++)
                        parts.Add(a[i]?.ToString() ?? "undefined");
                    System.Diagnostics.Debug.WriteLine($"[console.error] {string.Join(" ", parts)}");
                    return JSUndefined.Value;
                }, "error"),
                JSPropertyAttributes.EnumerableConfigurableValue);

            console.FastAddValue(
                (KeyString)"info",
                new JSFunction((in Arguments a) =>
                {
                    var parts = new List<string>();
                    for (var i = 0; i < a.Length; i++)
                        parts.Add(a[i]?.ToString() ?? "undefined");
                    System.Diagnostics.Debug.WriteLine($"[console.info] {string.Join(" ", parts)}");
                    return JSUndefined.Value;
                }, "info"),
                JSPropertyAttributes.EnumerableConfigurableValue);

            return console;
        }

        private readonly Dictionary<DomElement, JSObject> _jsObjectCache = new();

        private JSObject ToJSObject(DomElement element)
        {
            if (_jsObjectCache.TryGetValue(element, out var cached))
                return cached;

            var obj = new JSObject();
            _jsObjectCache[element] = obj;

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

            // textContent (read/write)
            obj.FastAddProperty(
                (KeyString)"textContent",
                new JSFunction((in Arguments a) =>
                    element.TextContent != null ? (JSValue)new JSString(element.TextContent) : new JSString(element.InnerHtml),
                    "get textContent"),
                new JSFunction((in Arguments a) =>
                {
                    element.TextContent = a.Length > 0 ? a[0].ToString() : string.Empty;
                    return JSUndefined.Value;
                }, "set textContent"),
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

            // -- DOM tree navigation --

            // parentNode (read-only, dynamic)
            obj.FastAddProperty(
                (KeyString)"parentNode",
                new JSFunction((in Arguments a) =>
                    element.Parent != null ? (JSValue)ToJSObject(element.Parent) : JSNull.Value,
                    "get parentNode"),
                null,
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // childNodes (read-only, dynamic)
            obj.FastAddProperty(
                (KeyString)"childNodes",
                new JSFunction((in Arguments a) =>
                {
                    var children = new List<JSValue>();
                    foreach (var child in element.Children)
                        children.Add(ToJSObject(child));
                    return new JSArray(children);
                }, "get childNodes"),
                null,
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // firstChild (read-only, dynamic)
            obj.FastAddProperty(
                (KeyString)"firstChild",
                new JSFunction((in Arguments a) =>
                    element.Children.Count > 0 ? (JSValue)ToJSObject(element.Children[0]) : JSNull.Value,
                    "get firstChild"),
                null,
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // lastChild (read-only, dynamic)
            obj.FastAddProperty(
                (KeyString)"lastChild",
                new JSFunction((in Arguments a) =>
                    element.Children.Count > 0 ? (JSValue)ToJSObject(element.Children[^1]) : JSNull.Value,
                    "get lastChild"),
                null,
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // nextSibling (read-only, dynamic)
            obj.FastAddProperty(
                (KeyString)"nextSibling",
                new JSFunction((in Arguments a) =>
                {
                    if (element.Parent == null) return JSNull.Value;
                    var siblings = element.Parent.Children;
                    var idx = siblings.IndexOf(element);
                    return idx >= 0 && idx + 1 < siblings.Count
                        ? (JSValue)ToJSObject(siblings[idx + 1])
                        : JSNull.Value;
                }, "get nextSibling"),
                null,
                JSPropertyAttributes.EnumerableConfigurableProperty);

            // -- DOM manipulation methods --

            // appendChild(child)
            obj.FastAddValue(
                (KeyString)"appendChild",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length == 0) return JSUndefined.Value;
                    var childObj = a[0] as JSObject;
                    if (childObj == null) return JSUndefined.Value;

                    // Find the DomElement for this child JSObject
                    var childEl = FindDomElementByJSObject(childObj);
                    if (childEl == null) return a[0];

                    // Remove from old parent if any
                    childEl.Parent?.Children.Remove(childEl);
                    childEl.Parent = element;
                    element.Children.Add(childEl);
                    return a[0];
                }, "appendChild", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // removeChild(child)
            obj.FastAddValue(
                (KeyString)"removeChild",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length == 0) return JSUndefined.Value;
                    var childObj = a[0] as JSObject;
                    if (childObj == null) return JSUndefined.Value;

                    var childEl = FindDomElementByJSObject(childObj);
                    if (childEl == null || !element.Children.Remove(childEl))
                        return a[0];
                    childEl.Parent = null;
                    return a[0];
                }, "removeChild", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // replaceChild(newChild, oldChild)
            obj.FastAddValue(
                (KeyString)"replaceChild",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length < 2) return JSUndefined.Value;
                    var newChildObj = a[0] as JSObject;
                    var oldChildObj = a[1] as JSObject;
                    if (newChildObj == null || oldChildObj == null) return JSUndefined.Value;

                    var newEl = FindDomElementByJSObject(newChildObj);
                    var oldEl = FindDomElementByJSObject(oldChildObj);
                    if (newEl == null || oldEl == null) return a[1];

                    var idx = element.Children.IndexOf(oldEl);
                    if (idx < 0) return a[1];

                    oldEl.Parent = null;
                    newEl.Parent?.Children.Remove(newEl);
                    newEl.Parent = element;
                    element.Children[idx] = newEl;
                    return a[1]; // returns the old child
                }, "replaceChild", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // -- DOM events --

            // addEventListener(type, listener)
            obj.FastAddValue(
                (KeyString)"addEventListener",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length < 2) return JSUndefined.Value;
                    var type = a[0].ToString();
                    var listener = a[1];
                    if (!element.EventListeners.TryGetValue(type, out var listeners))
                    {
                        listeners = new List<JSValue>();
                        element.EventListeners[type] = listeners;
                    }
                    listeners.Add(listener);
                    return JSUndefined.Value;
                }, "addEventListener", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // removeEventListener(type, listener)
            obj.FastAddValue(
                (KeyString)"removeEventListener",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length < 2) return JSUndefined.Value;
                    var type = a[0].ToString();
                    var listener = a[1];
                    if (element.EventListeners.TryGetValue(type, out var listeners))
                        listeners.Remove(listener);
                    return JSUndefined.Value;
                }, "removeEventListener", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            return obj;
        }

        /// <summary>
        /// Finds the <see cref="DomElement"/> corresponding to a given <see cref="JSObject"/>
        /// by looking up the JS object cache.
        /// </summary>
        private DomElement? FindDomElementByJSObject(JSObject jsObj)
        {
            foreach (var kvp in _jsObjectCache)
            {
                if (ReferenceEquals(kvp.Value, jsObj))
                    return kvp.Key;
            }
            return null;
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

        /// <summary>
        /// Builds an in-memory <c>localStorage</c> stub exposing <c>getItem</c>,
        /// <c>setItem</c>, <c>removeItem</c>, and <c>clear</c>.
        /// Bracket-notation access (e.g. <c>localStorage["key"]</c>) naturally
        /// falls through to JSObject property lookup.
        /// </summary>
        private static JSObject BuildLocalStorageObject()
        {
            var storage = new JSObject();
            var store = new Dictionary<string, string>();

            // localStorage.getItem(key)
            storage.FastAddValue(
                (KeyString)"getItem",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length == 0) return JSNull.Value;
                    var key = a[0].ToString();
                    return store.TryGetValue(key, out var val) ? (JSValue)new JSString(val) : JSNull.Value;
                }, "getItem", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // localStorage.setItem(key, value)
            storage.FastAddValue(
                (KeyString)"setItem",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length >= 2)
                    {
                        var key = a[0].ToString();
                        var val = a[1].ToString();
                        store[key] = val;
                        storage[(KeyString)key] = new JSString(val);
                    }
                    return JSUndefined.Value;
                }, "setItem", 2),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // localStorage.removeItem(key)
            storage.FastAddValue(
                (KeyString)"removeItem",
                new JSFunction((in Arguments a) =>
                {
                    if (a.Length > 0)
                    {
                        var key = a[0].ToString();
                        store.Remove(key);
                        storage.Delete((KeyString)key);
                    }
                    return JSUndefined.Value;
                }, "removeItem", 1),
                JSPropertyAttributes.EnumerableConfigurableValue);

            // localStorage.clear()
            storage.FastAddValue(
                (KeyString)"clear",
                new JSFunction((in Arguments a) =>
                {
                    foreach (var key in store.Keys.ToList())
                        storage.Delete((KeyString)key);
                    store.Clear();
                    return JSUndefined.Value;
                }, "clear", 0),
                JSPropertyAttributes.EnumerableConfigurableValue);

            return storage;
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

        /// <summary>Parent element in the DOM tree.</summary>
        public DomElement? Parent { get; set; }

        /// <summary>Ordered child elements in the DOM tree.</summary>
        public List<DomElement> Children { get; } = new();

        /// <summary>Whether this element represents a text node created via <c>document.createTextNode</c>.</summary>
        public bool IsTextNode { get; }

        /// <summary>Text content for text nodes.</summary>
        public string? TextContent { get; set; }

        /// <summary>Registered event listeners keyed by event type (e.g. "click", "input", "submit").</summary>
        public Dictionary<string, List<JSValue>> EventListeners { get; } = new(System.StringComparer.OrdinalIgnoreCase);

        public DomElement(
            string tagName,
            string? id,
            string? className,
            string innerHtml,
            Dictionary<string, string>? style = null,
            Dictionary<string, string>? attributes = null,
            bool isTextNode = false)
        {
            TagName = tagName;
            Id = id;
            ClassName = className;
            InnerHtml = innerHtml;
            Style = style ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            Attributes = attributes ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            IsTextNode = isTextNode;
        }
    }
}

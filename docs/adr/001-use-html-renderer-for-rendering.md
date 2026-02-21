# ADR-001: Use HTML-Renderer for HTML/CSS Rendering

## Status

Accepted

## Context

Broiler needs a rendering engine to display HTML and CSS content. Options include embedded Chromium (CefSharp/WebView2), custom rendering, or managed HTML rendering libraries.

## Decision

Use [HTML-Renderer](https://github.com/ArtOfSettling/HTML-Renderer) (v1.5.2) as the HTML/CSS rendering engine.

## Rationale

- **100% managed code**: No native dependencies, ActiveX, or MSHTML required
- **WPF native controls**: Provides `HtmlPanel` and `HtmlLabel` controls for WPF
- **Lightweight**: ~300KB total, low memory footprint
- **Extensible**: Adapter pattern allows customization of rendering behavior
- **HTML 4.01 and CSS Level 2 support**: Covers core web standards
- **High performance**: Efficient rendering pipeline with text selection and context menu support

## Consequences

- Limited to HTML 4.01 and CSS Level 2 (no CSS3 flexbox/grid, HTML5 Canvas, etc.)
- No built-in JavaScript execution (handled separately by YantraJS)
- Need to update target framework from net462 to support modern .NET alongside .NET Framework

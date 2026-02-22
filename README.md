# Broiler

A WPF-based web browser built with [HTML-Renderer](https://github.com/ArtOfSettling/HTML-Renderer) for HTML/CSS rendering and [YantraJS](https://github.com/yantrajs/yantra) for JavaScript execution.

## Overview

Broiler is a lightweight, extensible web browser for Windows built entirely in managed C#. It combines:

- **HTML-Renderer** — a high-performance, 100% managed HTML/CSS rendering engine for WPF
- **YantraJS** — a .NET Standard JavaScript engine supporting ES2020+ features

## Architecture

```
┌─────────────────────────────────────────────┐
│              Broiler WPF Shell              │
│  ┌───────────────────────────────────────┐  │
│  │     Navigation Bar (URL, Controls)    │  │
│  ├───────────────────────────────────────┤  │
│  │                                       │  │
│  │          HtmlPanel (Renderer)         │  │
│  │      ┌─────────────────────────┐      │  │
│  │      │    HtmlRenderer.WPF     │      │  │
│  │      │    (HTML/CSS Engine)     │      │  │
│  │      └──────────┬──────────────┘      │  │
│  │                 │                     │  │
│  │      ┌──────────▼──────────────┐      │  │
│  │      │   YantraJS (JSContext)  │      │  │
│  │      │   (JavaScript Engine)   │      │  │
│  │      └─────────────────────────┘      │  │
│  │                                       │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

### Key Components

| Component | Description |
|-----------|-------------|
| `Broiler.App` | WPF application entry point and main window |
| `Broiler.App.Rendering` | Modular rendering pipeline (page loading, script extraction, JS execution) |
| `HtmlRenderer.WPF` | WPF adapter for the HTML rendering engine |
| `HtmlRenderer.Core` | Cross-platform HTML/CSS parsing and rendering |
| `YantraJS.Core` | JavaScript engine with ES2020+ support |

## Building

### Prerequisites

- .NET 8.0 SDK or later
- Windows (WPF requires Windows)

### Build

```bash
dotnet build Broiler.slnx
```

### Run

```bash
dotnet run --project src/Broiler.App
```

## Project Structure

```
Broiler/
├── src/
│   ├── Broiler.App/              # WPF browser application
│   │   └── Rendering/            # Modular rendering pipeline
│   └── Broiler.App.Tests/        # Unit tests
├── docs/
│   └── adr/                      # Architecture Decision Records
├── HTML-Renderer-1.5.2/          # HTML/CSS rendering engine
├── yantra-1.2.295/                # JavaScript engine
└── Broiler.slnx                   # Solution file
```

## Roadmap

See [Issue #1](https://github.com/MaiRat/Broiler/issues/1) for the full development roadmap.

### Current Phase: Project Initialization

- [x] Define project goals and design requirements
- [x] Establish project directory structure
- [x] Set up solution and source control
- [x] Document architectural decisions (ADR)
- [x] Create initial WPF project skeleton
- [x] Integrate html-renderer and yantra as project references
- [x] Implement navigation history (back/forward/refresh)
- [x] Implement rendering pipeline
- [x] Enable DOM interaction via yantra
- [ ] Support advanced HTML/CSS features

## DOM Interaction

Broiler exposes a minimal `document` object to JavaScript executed via YantraJS,
enabling scripts embedded in HTML pages to interact with the DOM.

### Available APIs

| API | Description |
|-----|-------------|
| `document.title` | Read or write the page title |
| `document.getElementById(id)` | Find an element by its `id` attribute |
| `document.getElementsByTagName(tag)` | Find all elements with the given tag name |
| `document.createElement(tag)` | Create a new element |

Each element object returned by these methods exposes `tagName`, `id`,
`className`, and `innerHTML` properties.

### Example

Given the following HTML page:

```html
<html>
<head><title>Demo</title></head>
<body>
  <div id="greeting" class="box">Hello</div>
  <script>
    var el = document.getElementById('greeting');
    // el.tagName  → "DIV"
    // el.id       → "greeting"
    // el.className → "box"
    // el.innerHTML → "Hello"
    var t = document.title; // "Demo"
  </script>
</body>
</html>
```

### Architecture

The `DomBridge` class parses the page HTML and registers a `document` global on
the YantraJS `JSContext` before scripts execute.  This enables bidirectional
communication: JavaScript can query the DOM, and property changes (e.g. setting
`document.title`) are reflected back to the bridge.

```
PageContent (HTML + Scripts)
       │
       ▼
┌──────────────┐
│ ScriptEngine │
│  ┌─────────┐ │
│  │DomBridge│──▶ Parses HTML → registers document object
│  └─────────┘ │
│  ┌─────────┐ │
│  │JSContext │──▶ Executes scripts with document available
│  └─────────┘ │
└──────────────┘
```

## License

See individual component licenses:
- HTML-Renderer: BSD License
- YantraJS: Apache-2.0 License
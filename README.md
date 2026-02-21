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
│   └── Broiler.App/          # WPF browser application
├── docs/
│   └── adr/                  # Architecture Decision Records
├── HTML-Renderer-1.5.2/      # HTML/CSS rendering engine
├── yantra-1.2.295/            # JavaScript engine
└── Broiler.slnx               # Solution file
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
- [ ] Implement rendering pipeline
- [ ] Enable DOM interaction via yantra
- [ ] Support advanced HTML/CSS features

## License

See individual component licenses:
- HTML-Renderer: BSD License
- YantraJS: Apache-2.0 License
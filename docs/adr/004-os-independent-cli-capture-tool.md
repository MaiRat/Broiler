# ADR-004: OS-Independent CLI Tool for Website Capture

## Status

Accepted

## Context

Broiler currently runs as a WPF desktop application, which limits it to Windows.
There is a need for a cross-platform command-line utility that can capture a
screenshot of any website given a URL. The tool must be usable by non-developers
on all major operating systems (Windows, macOS, Linux) and should also allow
testing the embedded engines and libraries used by the Broiler backend.

Additionally, the tool should integrate into the Copilot agent workflow so that
after every GitHub issue resolution a website capture is automatically triggered
against a reference URL (https://www.heise.de/) to verify that the rendering
pipeline is functional.

## Decision

Implement the CLI tool as a .NET console application (`Broiler.Cli`) targeting
`net8.0` (not `net8.0-windows`) and use **Microsoft Playwright for .NET** to
perform headless browser-based website capture.

## Rationale

- **Cross-platform**: Playwright supports Windows, macOS, and Linux with a
  single codebase. Using `net8.0` instead of `net8.0-windows` removes the WPF
  dependency.
- **Headless capture**: Playwright launches a real browser engine (Chromium,
  Firefox, or WebKit) in headless mode, producing accurate screenshots that
  include JavaScript-rendered content.
- **Simple CLI**: A `dotnet tool` or self-contained executable with a command
  like `broiler-capture --url <URL> --output <file>` is intuitive for
  non-technical users.
- **Engine testing**: The CLI project can reference the existing rendering
  and script engine assemblies, enabling integration tests of HTML-Renderer
  and YantraJS alongside the Playwright-based capture.
- **Mature ecosystem**: Playwright is actively maintained by Microsoft, has
  excellent documentation, and supports automatic browser installation via
  `playwright install`.

## Consequences

- Adds a runtime dependency on Playwright and a browser binary (~150–300 MB
  one-time download via `playwright install`).
- The CLI project will live alongside the WPF project in the solution but
  targets `net8.0` so it can build and run on any OS.
- CI will need a step to install the Playwright browser before running
  capture-related tests.
- Non-technical users will need .NET 8 runtime or a self-contained publish
  to avoid SDK installation.

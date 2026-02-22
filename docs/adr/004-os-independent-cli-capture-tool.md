# ADR-004: OS-Independent CLI Tool for Website Capture

## Status

Accepted (Updated)

## Context

Broiler currently runs as a WPF desktop application, which limits it to Windows.
There is a need for a cross-platform command-line utility that can capture
website content given a URL. The tool must be usable by non-developers
on all major operating systems (Windows, macOS, Linux) and should also allow
testing the embedded engines and libraries used by the Broiler backend.

Additionally, the tool should integrate into the Copilot agent workflow so that
after every GitHub issue resolution a website capture is automatically triggered
against a reference URL (https://www.heise.de/) to verify that the rendering
pipeline is functional.

## Decision

Implement the CLI tool as a .NET console application (`Broiler.Cli`) targeting
`net8.0` (not `net8.0-windows`) and use the local rendering engines
(**HTML-Renderer** for CSS processing and **YantraJS** for JavaScript execution)
along with `HttpClient` for fetching web content.

## Rationale

- **Cross-platform**: Using `net8.0` instead of `net8.0-windows` removes
  platform-specific dependencies. HTML-Renderer core and YantraJS are both
  cross-platform.
- **No external browser dependency**: Unlike Playwright/Chromium, the local
  engines do not require downloading large browser binaries (~150–300 MB),
  simplifying setup and CI pipelines.
- **Content capture**: The CLI fetches HTML via `HttpClient`, processes CSS
  with HTML-Renderer, and executes inline scripts with YantraJS — producing
  processed HTML or text output.
- **Simple CLI**: A `dotnet tool` or self-contained executable with a command
  like `broiler-capture --url <URL> --output <file>` is intuitive for
  non-technical users.
- **Engine testing**: The CLI project references the same rendering and script
  engine assemblies, enabling integration tests of HTML-Renderer and YantraJS.
- **Consistent stack**: Using the same engines as the main Broiler.App ensures
  consistent behaviour across the desktop and CLI tools.

## Consequences

- The capture output is HTML/text rather than visual screenshots, since the
  local engines do not include a full layout engine for bitmap rendering.
- The CLI project lives alongside the WPF project in the solution but targets
  `net8.0` so it can build and run on any OS.
- CI no longer needs a step to install browser binaries.
- Non-technical users need only the .NET 8 runtime or a self-contained publish
  to run the tool.

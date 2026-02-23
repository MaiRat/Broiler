# ADR-005: Comprehensive Logging for HTML-Renderer and YantraJS

## Status

Accepted

## Context

The Broiler rendering pipeline comprises two major subsystems:

1. **HTML-Renderer** — parses and renders HTML/CSS.
2. **YantraJS** — executes JavaScript via its JS engine.

Both modules previously caught exceptions in many locations but silently
swallowed them (empty `catch { }` blocks) or logged only to
`System.Diagnostics.Debug.WriteLine` without structured context. This made it
difficult to:

- Diagnose rendering or script failures in production and CI environments.
- Distinguish whether a problem originated in HTML rendering or JavaScript
  execution.
- Correlate exceptions with timestamps, subsystem context, and stack traces.

## Decision

Introduce a lightweight, in-memory **`RenderLogger`** (static class) in
`Broiler.App.Rendering` that all rendering subsystems use to report exceptions
and diagnostic messages. The logger:

- Captures entries with **timestamp**, **category** (`HtmlRenderer` or
  `JavaScript`), **severity level** (`Debug`, `Info`, `Warning`, `Error`),
  **context string** (e.g. `ScriptEngine.Execute`, `DomBridge.fetch`),
  **message**, and optional **exception** (including stack trace).
- Stores entries in a thread-safe in-memory list accessible via
  `RenderLogger.GetEntries()`.
- Also forwards every entry to `Debug.WriteLine` to preserve existing
  diagnostics workflows.
- Supports a configurable **minimum level** (`RenderLogger.MinimumLevel`) to
  control verbosity at runtime.

All empty catch blocks across `ScriptEngine`, `DomBridge`, `MicroTaskQueue`,
`CaptureService`, and `EngineTestService` have been updated to log through
`RenderLogger`. Additionally, empty catch blocks inside the vendor
**html-renderer** and **yantra** source trees have been updated to log via
`Debug.WriteLine` with a `[HtmlRenderer]` or `[YantraJS]` prefix so that
previously hidden exceptions are now visible.

### Log Categories

| Category        | Description                                              |
|-----------------|----------------------------------------------------------|
| `HtmlRenderer`  | CSS parsing, font resolution, image download, layout     |
| `JavaScript`    | Script execution, DOM bridge, timers, fetch, microtasks  |

### Log Levels

| Level     | Usage                                                       |
|-----------|-------------------------------------------------------------|
| `Debug`   | Polyfill installation, console.log/info, submit prevention  |
| `Info`    | General informational messages                              |
| `Warning` | Non-fatal callback errors, JSON parse failures              |
| `Error`   | Script execution failures, fetch errors, engine test fails  |

## Consequences

- **Positive**: All exceptions are now captured and visible. Troubleshooting is
  significantly easier because logs include timestamp, context, and full
  exception details.
- **Positive**: The `LogCategory` enum makes it straightforward to filter logs
  by subsystem.
- **Positive**: The in-memory store allows tests and UI code to inspect log
  entries programmatically.
- **Positive**: `MinimumLevel` allows tuning verbosity without code changes.
- **Trade-off**: The logger is static/global, which simplifies adoption but
  means tests that inspect log entries must call `RenderLogger.Clear()` to
  avoid cross-test pollution.
- **Trade-off**: No external logging framework (Serilog, NLog) is introduced,
  keeping dependencies minimal. If structured log shipping (e.g. to files or
  remote services) is needed in the future, the `RenderLogger` can be extended
  to delegate to an `ILogger` sink.

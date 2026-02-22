# Copilot Instructions for Broiler

## Project Overview

Broiler is a WPF-based web browser built with HTML-Renderer and YantraJS. It
also includes a cross-platform CLI tool (`Broiler.Cli`) for headless website
capture using Playwright.

## Build & Test

```bash
# Build the entire solution
dotnet build Broiler.slnx

# Run all tests
dotnet test Broiler.slnx

# Run only CLI tests (cross-platform)
dotnet test src/Broiler.Cli.Tests/
```

## Post-Issue-Resolution Workflow

After resolving **any** GitHub issue, Copilot **must** perform the following
steps before considering the task complete:

### 1. Build and Test

```bash
dotnet build Broiler.slnx
dotnet test Broiler.slnx
```

All tests must pass. Fix any failures introduced by the change.

### 2. Website Capture Verification

Capture a screenshot of the reference website to verify the rendering pipeline:

```bash
dotnet run --project src/Broiler.Cli -- --url https://www.heise.de/ --output capture.png
```

- If the capture **succeeds**, attach or reference the screenshot in the PR.
- If the capture **fails**, investigate and resolve the error before completing
  the issue. Common failure causes:
  - **Browser not installed**: Run `dotnet playwright install chromium` first.
  - **Network timeout**: Increase timeout with `--timeout 60`.
  - **Missing .NET runtime**: Ensure .NET 8.0 SDK is installed.

### 3. Engine Smoke Test

Run the embedded engine tests to verify HTML-Renderer and YantraJS:

```bash
dotnet run --project src/Broiler.Cli -- --test-engines
```

Both engines must report success.

### 4. Error Resolution

If **any** errors or exceptions occur during the above steps:

1. Read the error message carefully — the CLI provides actionable guidance.
2. Fix the root cause in the codebase.
3. Re-run the failing step to confirm the fix.
4. Document the error and fix in the PR description.

## Code Conventions

- Follow existing C# code style (XML doc comments on public members).
- Place new rendering logic in `src/Broiler.App/Rendering/`.
- Place CLI logic in `src/Broiler.Cli/`.
- Write xUnit tests for all new functionality.
- Use Architecture Decision Records (ADRs) in `docs/adr/` for significant
  design decisions.

## Architecture

See `docs/adr/` for Architecture Decision Records and `docs/roadmap/` for
development roadmaps.

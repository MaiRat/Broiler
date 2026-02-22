# Copilot Instructions for Broiler

## Project Overview

Broiler is a WPF-based web browser built with HTML-Renderer and YantraJS. It
also includes a cross-platform CLI tool (`Broiler.Cli`) for headless website
capture using the local rendering engines (HTML-Renderer and YantraJS).

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

Capture content from the reference website to verify the rendering pipeline.
Both HTML and image outputs must be produced successfully.

#### HTML Capture

```bash
dotnet run --project src/Broiler.Cli -- --url https://www.heise.de/ --output capture.html
```

#### Image Capture (PNG)

```bash
dotnet run --project src/Broiler.Cli -- --capture-image https://www.heise.de/ --output capture.png
```

- If the captures **succeed**, attach or reference the outputs in the PR.
- If a capture **fails**, investigate and resolve the error before completing
  the issue. Common failure causes:
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

## Test Workflow: Heise.de Capture & Exception Handling

Every test run **must** include the `HeiseCaptureTests` suite, which captures
`https://www.heise.de/` as HTML, PNG, and JPEG. These tests exercise the full
rendering pipeline against a live website.

### Retry Strategy

All live-site capture tests use a built-in retry mechanism
(`ExecuteWithRetryAsync`) to handle transient failures:

- **Maximum retries**: 3 attempts per test.
- **Back-off**: Linear delay (2 s × attempt number).
- **Retried exception types**:
  - `HttpRequestException` — network/DNS failures.
  - `TaskCanceledException` — HTTP timeout.
  - `TimeoutException` — general timeout.
  - `IOException` — file-system or stream errors.
- On the final attempt, exceptions propagate and fail the test.

### Logging

Each caught exception is recorded in an in-memory log. If the test ultimately
fails, the log is included in the assertion message so the failure report
contains the full history of transient errors.

### Workflow for Test Developers

1. Run the full test suite: `dotnet test Broiler.slnx`.
2. If a `HeiseCaptureTests` test fails, check the assertion message for the
   exception log—it documents every transient error and retry.
3. For persistent failures, increase `TimeoutSeconds` (default 60) or
   `MaxRetries` (default 3) in `HeiseCaptureTests.cs`.
4. Document any new exception types encountered during testing and add them to
   the retry filter in `ExecuteWithRetryAsync` if they are transient.

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

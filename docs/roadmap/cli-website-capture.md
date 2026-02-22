# Roadmap: OS-Independent Command-Line Tool for Website Capture

## Goal

Deliver a cross-platform command-line utility that captures a website screenshot
from any URL and integrates with the Copilot agent workflow for automated
verification after issue resolution.

---

## Phase 1 — Project Scaffolding

- [x] Create `src/Broiler.Cli/Broiler.Cli.csproj` targeting `net8.0`
- [x] Add the new project to `Broiler.slnx`
- [x] Add `Microsoft.Playwright` NuGet dependency
- [x] Implement a minimal `Program.cs` that accepts `--url` and `--output` arguments
- [x] Verify the project builds on Windows, macOS, and Linux

## Phase 2 — Website Capture Implementation

- [x] Implement `CaptureService` that uses Playwright to:
  - Launch a headless Chromium browser
  - Navigate to the specified URL
  - Wait for the page to fully load (network idle)
  - Save a full-page screenshot to the output path
- [x] Add robust error handling:
  - Invalid URL validation
  - Navigation timeout with configurable duration
  - File I/O errors (permissions, disk space)
  - Browser launch failures with helpful messages
- [x] Support output formats: PNG (default), JPEG
- [x] Add `--full-page` flag for full-page vs. viewport-only capture
- [x] Add `--timeout` option (default: 30 seconds)

## Phase 3 — Engine Testing Integration

- [ ] Reference `Broiler.App` rendering assemblies from `Broiler.Cli`
- [ ] Add a `--test-engines` command that:
  - Runs a basic HTML-Renderer parse/render cycle
  - Runs a basic YantraJS script execution cycle
  - Reports pass/fail for each engine
- [ ] Create `src/Broiler.Cli.Tests/` with xUnit tests for:
  - Successful capture of a local HTML file
  - Error handling for invalid URLs
  - Engine test command output

## Phase 4 — Copilot Workflow Integration

- [ ] Update `.github/copilot-instructions.md` to document:
  - Post-issue-resolution capture step
  - Expected error handling behavior
  - Reference URL (`https://www.heise.de/`)
- [ ] Add CI workflow step that runs after tests:
  - `dotnet run --project src/Broiler.Cli -- --url https://www.heise.de/ --output capture.png`
  - Upload the screenshot as a build artifact
- [ ] Document the workflow in the project README

## Phase 5 — Distribution & Packaging

- [ ] Publish as a .NET global tool (`dotnet tool install -g broiler-capture`)
- [ ] Provide self-contained builds for users without .NET SDK:
  - `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`
- [ ] Add installation instructions to README
- [ ] Write user-facing documentation with usage examples

---

## Usage Examples (Target CLI)

```bash
# Capture a website screenshot
broiler-capture --url https://www.heise.de/ --output heise.png

# Full-page capture with custom timeout
broiler-capture --url https://example.com --output page.png --full-page --timeout 60

# Test embedded engines
broiler-capture --test-engines

# Show help
broiler-capture --help
```

---

## Acceptance Criteria

1. The CLI tool runs on Windows, macOS, and Linux without modification.
2. Given a valid URL, the tool produces a PNG screenshot of the website.
3. Errors (invalid URL, timeout, browser failure) are reported with clear,
   actionable messages.
4. The Copilot instruction file documents the post-issue capture workflow.
5. CI automatically captures `https://www.heise.de/` after each successful
   build and test run.

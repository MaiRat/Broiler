# ADR-003: Use WPF as the Application Framework

## Status

Accepted

## Context

Broiler needs a desktop application framework for its browser shell. Options include WPF, WinForms, MAUI, Avalonia, or Electron.

## Decision

Use Windows Presentation Foundation (WPF) on .NET 8+ as the application framework.

## Rationale

- **Rich UI toolkit**: XAML-based declarative UI with data binding, styling, and templating
- **HTML-Renderer compatibility**: HTML-Renderer already provides native WPF controls (`HtmlPanel`, `HtmlLabel`)
- **Windows native**: Full access to Windows APIs and system integration
- **Mature ecosystem**: Well-documented with extensive community support
- **.NET 8+ support**: Long-term support with modern .NET runtime performance

## Consequences

- Windows-only: Broiler will only run on Windows
- Requires .NET 8 SDK or later for building
- WPF-specific knowledge needed for UI development

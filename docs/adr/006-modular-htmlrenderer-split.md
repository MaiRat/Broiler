# ADR-006: Modular HtmlRenderer Project Split

## Status

Accepted

## Context

The `HtmlRenderer` library was a single monolithic project containing all
rendering logic, DOM parsing, CSS handling, adapter abstractions, utility
classes, and entity types. While this structure worked, it conflated multiple
concerns in one assembly, making it harder to reason about dependencies and
limiting opportunities for independent versioning or lightweight references.

A dependency analysis of the existing codebase identified the following
internal structure:

| Namespace Group          | Dependencies                                                       |
|--------------------------|--------------------------------------------------------------------|
| `Adapters.Entities`      | None (pure value types)                                            |
| `Core.Utils` (pure)      | `Adapters.Entities` only                                           |
| `Core.Utils` (coupled)   | `Adapters`, `Core.Dom`, `Core.Parse`, `Core.Entities`              |
| `Adapters` (abstracts)   | `Adapters.Entities`, `Core.Utils`                                  |
| `Adapters.RAdapter`      | `Core`, `Core.Handlers`, `Core.Utils`                              |
| `Core.Dom`               | `Adapters`, `Core.Utils`, `Core.Parse`, `Core.Entities`, `Core.Handlers` |
| `Core.Parse`             | `Adapters`, `Core.Utils`, `Core.Dom`, `Core.Entities`, `Core.Handlers`   |
| `Core.Handlers`          | `Adapters`, `Core.Dom`, `Core.Utils`, `Core.Entities`              |

Key finding: Circular dependencies between `Core.Dom`, `Core.Parse`, and
`Core.Handlers` prevent a clean five-way split into Dom/Css/Adapters/Core/Utils
without significant interface extraction.

## Decision

Split the `HtmlRenderer` project into **three assemblies** that follow the
natural layering identified in the dependency analysis:

```
HtmlRenderer.Primitives   (Layer 0 – zero dependencies)
        ↑
HtmlRenderer.Utils         (Layer 1 – depends on Primitives)
        ↑
HtmlRenderer               (Layer 2 – depends on Primitives + Utils)
```

### HtmlRenderer.Primitives

Contains only the `Adapters/Entities` value types:
`RColor`, `RDashStyle`, `RFontStyle`, `RKeyEvent`, `RMouseEvent`, `RPoint`,
`RRect`, `RSize`.

These types have zero internal dependencies and form the foundation that
every other module builds upon.

### HtmlRenderer.Utils

Contains pure utility classes that depend only on Primitives:
`ArgChecker`, `CommonUtils`, `CssConstants`, `HtmlConstants`, `HtmlUtils`,
`SubString`.

Utility classes with heavier dependencies (`CssUtils`, `DomUtils`,
`RenderUtils`) remain in the core `HtmlRenderer` assembly because they
reference DOM and Parse types.

`InternalsVisibleTo` attributes expose internal types to `HtmlRenderer`,
`HtmlRenderer.Image`, and `HtmlRenderer.Image.Tests`.

### HtmlRenderer (Core)

Retains all remaining code: abstract adapter classes, `RAdapter`, DOM,
CSS parsing, layout engine, handlers, entities, and coupled utility classes.
References both `HtmlRenderer.Primitives` and `HtmlRenderer.Utils`.

## Rationale

- **Minimal disruption**: Namespaces are preserved (`TheArtOfDev.HtmlRenderer.*`),
  so downstream code does not need source changes.
- **Clean layering**: Each layer has a strict unidirectional dependency.
- **Extensibility**: Future refactoring (e.g., extracting `Core.Dom` and
  `Core.Parse` once circular dependencies are resolved) can build on this
  foundation.
- **Lightweight references**: Projects that only need primitive types (e.g.,
  custom adapter implementations) can reference `HtmlRenderer.Primitives`
  without pulling in the full rendering engine.

## Consequences

- Consumer projects (`HtmlRenderer.WPF`, `HtmlRenderer.Image`, `Broiler.Cli`,
  `Broiler.App`) continue to reference `HtmlRenderer` and receive transitive
  access to `Primitives` and `Utils`.
- `InternalsVisibleTo` is required for `HtmlRenderer.Utils` → `HtmlRenderer`
  because several utility classes are `internal`.
- The solution file (`Broiler.slnx`) includes the two new projects.
- Further modularization (splitting DOM, CSS, Adapters from Core) would require
  breaking the circular dependency between `Core.Dom` and `Core.Parse`, likely
  through interface extraction or architectural changes.

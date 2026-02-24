# ADR-007: Advanced HtmlRenderer Modularization

## Status

Accepted

## Context

Following ADR-006 which split HtmlRenderer into three assemblies (Primitives,
Utils, HtmlRenderer), this ADR documents the next phase of modularization that
creates additional well-defined modules with clear dependency boundaries.

The original HtmlRenderer monolith has been analysed for internal dependencies:

- **Entities** (event args, CSS blocks) have minimal dependencies on Utils and
  Primitives.
- **CSS parsing** (CssParser, CssValueParser) depends on Adapters for colour
  resolution but has NO dependency on DOM types.
- **Abstract adapters** (RBrush, RPen, RImage, etc.) depend only on Primitives.
- **DOM tree** (CssBox, CssLayoutEngine, handlers) is tightly coupled with
  HtmlContainerInt (the orchestration layer) and RAdapter.

### Before

ADR-006 produced three assemblies:

| # | Assembly                 | Layer |
|---|--------------------------|-------|
| 1 | HtmlRenderer.Primitives  | L0    |
| 2 | HtmlRenderer.Utils       | L1    |
| 3 | HtmlRenderer             | L2    |

### After

This phase adds three more assemblies, bringing the total to **six**:

| # | Assembly                 | Layer | Contents                                         |
|---|--------------------------|-------|--------------------------------------------------|
| 1 | HtmlRenderer.Primitives  | L0    | Value types (RColor, RPoint, RRect, RSize, …)    |
| 2 | HtmlRenderer.Utils       | L1    | Pure utilities (ArgChecker, CommonUtils, …)       |
| 3 | HtmlRenderer.Adapters    | L2a   | Abstract adapters (RBrush, RPen, RImage, …)      |
| 4 | HtmlRenderer.Core        | L2b   | Entities, CssData, CssDefaults, CssUnit, Border  |
| 5 | HtmlRenderer.CSS         | L3    | CssParser, CssValueParser, CssLength, RegexParser |
| 6 | HtmlRenderer             | L4    | DOM, handlers, orchestration (façade)             |

## Decision

### Extracted Modules

#### HtmlRenderer.Adapters (Layer 2a)

Contains platform-independent abstract adapter base classes that depend only on
Primitives and Utils:

- `Adapters/RBrush` — Abstract brush
- `Adapters/RPen` — Abstract pen
- `Adapters/RImage` — Abstract image with dispose semantics
- `Adapters/RFontFamily` — Abstract font family
- `Adapters/RGraphicsPath` — Abstract graphics path
- `Adapters/RFont` — Abstract font (moved from HtmlRenderer)
- `Adapters/RGraphics` — Abstract graphics context (moved from HtmlRenderer,
  uses `IResourceFactory` instead of `RAdapter`)
- `Adapters/IResourceFactory` — Interface for creating rendering resources
  (pens, brushes); decouples `RGraphics` from `RAdapter`
- `Adapters/IFontCreator` — Interface for creating fonts; decouples
  `FontsHandler` from `RAdapter`

Dependencies: **HtmlRenderer.Primitives**, **HtmlRenderer.Utils**.

These adapters are consumed by both the CSS layer (for colour resolution) and
the main HtmlRenderer assembly (for concrete adapter implementations). Placing
them in their own project removes the forced dependency on DOM or parsing code
for projects that only need to implement rendering primitives.

#### HtmlRenderer.Core (Layer 2b)

Contains entities and shared type definitions:

- `Core/Entities/` — All event-argument types (CssBlock, CssBlockSelectorItem,
  HtmlImageLoadEventArgs, HtmlLinkClickedEventArgs,
  HtmlRenderErrorEventArgs, HtmlScrollEventArgs, HtmlStylesheetLoadEventArgs)
- `Core/Dom/CssUnit` — CSS unit enumeration
- `Core/Dom/Border` — Border enumeration
- `Core/CssData` — CSS stylesheet data structure
- `Core/CssDefaults` — Default CSS stylesheet constant
- `Core/IColorResolver` — Interface breaking the CSS↔Adapter circular
  dependency
- `Core/IHtmlContainerInt` — Interface breaking the CssBox↔HtmlContainerInt
  bidirectional dependency
- `Core/IImageLoadHandler` — Interface decoupling CssBox from ImageLoadHandler
- `Core/IBordersDrawHandler` — Interface decoupling CssBox from
  BordersDrawHandler (with `IBorderRenderData`)
- `Core/IBackgroundImageDrawHandler` — Interface decoupling CssBox from
  BackgroundImageDrawHandler (with `IBackgroundRenderData`)

Dependencies: **HtmlRenderer.Primitives**, **HtmlRenderer.Utils**,
**HtmlRenderer.Adapters**

The critical design element is `IColorResolver`:

```csharp
/// <summary>
/// Resolves colour names and checks font availability without depending
/// on the concrete RAdapter type.
/// </summary>
public interface IColorResolver
{
    RColor GetColor(string colorName);
    bool IsFontExists(string family);
}
```

This interface lives in Core so that both CssParser (in the CSS module) and
RAdapter (in the main assembly) can reference it without creating a cycle.
RAdapter implements `IColorResolver`.

#### HtmlRenderer.CSS (Layer 3)

Contains CSS parsing and value resolution:

- `Core/Parse/CssParser` — CSS stylesheet parser (refactored to accept
  `IColorResolver` instead of `RAdapter`)
- `Core/Parse/CssValueParser` — CSS value parser (refactored to accept
  `double emHeight` instead of `CssBoxProperties`)
- `Core/Parse/RegexParserHelper` — Regex-based CSS parsing utilities
- `Core/Parse/RegexParserUtils` — Regex patterns for CSS parsing
- `Core/Dom/CssLength` — CSS length value type
- `Core/CssDataParser` — Factory for creating `CssData` from stylesheets

Dependencies: **HtmlRenderer.Primitives**, **HtmlRenderer.Utils**,
**HtmlRenderer.Core**

### Remaining in HtmlRenderer (Layer 4, Façade)

The following remain in the main HtmlRenderer assembly due to tight coupling:

**Adapters** (depend on RAdapter which orchestrates everything):

- `RAdapter` — Central adapter with FontsHandler, CssData caching; implements
  `IColorResolver`, `IResourceFactory`, `IFontCreator`
- `RControl` — Abstract control (references RAdapter)
- `RContextMenu` — Abstract context menu (references RControl)

**DOM & Layout** (depend on IHtmlContainerInt):

- `CssBox` and subclasses — DOM tree nodes; uses `ContainerInt`
  (`IHtmlContainerInt`) for all container access
- `CssBoxProperties` — CSS property bag; implements `IBorderRenderData` and
  `IBackgroundRenderData`
- `CssLayoutEngine` — Layout computation
- `HtmlTag`, `HoverBoxBlock` — Tag representation

**Handlers** (depend on HtmlContainerInt and/or CssBox):

- Drawing: `BackgroundImageDrawHandler`, `BordersDrawHandler`
- Interaction: `SelectionHandler`, `ContextMenuHandler`
- Loading: `ImageLoadHandler`, `ImageDownloader`,
  `StylesheetLoadHandler`, `FontsHandler`

**Utils** (depend on DOM and Adapter types):

- `CssUtils`, `DomUtils`, `RenderUtils`

**Orchestration**:

- `HtmlContainerInt`, `DomParser`, `HtmlParser`, `HtmlRendererUtils`

### Circular Dependencies Resolved

1. **CssParser ↔ RAdapter**: CssParser previously took `RAdapter` as a
   constructor parameter. `RAdapter.DefaultCssData` created CssParser.

   **Resolution**: Introduced `IColorResolver` interface in Core. CssParser now
   accepts `IColorResolver` (which provides `GetColor` and `IsFontExists`).
   RAdapter implements `IColorResolver`. `CssData.Parse` moved to
   `CssDataParser`, accepting `IColorResolver` instead of `RAdapter`.

2. **CssValueParser ↔ CssBoxProperties**: `CssValueParser.ParseLength` took
   `CssBoxProperties` as a parameter (for `GetEmHeight()`).
   `CssBoxProperties` called `CssValueParser.ParseLength` extensively.

   **Resolution**: Changed `ParseLength` overloads to accept `double emHeight`
   instead of `CssBoxProperties`. All callers updated to pass
   `GetEmHeight()` directly.

### Circular Dependencies Remaining (Future Work)

3. **CssBox ↔ HtmlContainerInt**: ~~`CssBox` stores `HtmlContainerInt` as a
   field (`protected HtmlContainerInt _htmlContainer`) and accesses ~15
   members.~~

   **Resolved**: Defined `IHtmlContainerInt` in Core with all members CssBox
   needs (`ReportError`, `ScrollOffset`, `RootLocation`, `ActualSize`,
   `PageSize`, `AvoidGeometryAntialias`, `SelectionForeColor`,
   `SelectionBackColor`, `RequestRefresh`, `GetFont`, `ParseColor`,
   `AvoidAsyncImagesLoading`, `AvoidImagesLateLoading`, `MarginTop`,
   `ConvertImage`, `ImageFromStream`, `GetLoadingImage`,
   `GetLoadingFailedImage`, `DownloadImage`, `RaiseHtmlImageLoadEvent`).
   `CssBox` now stores `IHtmlContainerInt` and accesses it through
   `ContainerInt`. The backward-compatible `HtmlContainer` property (concrete
   type) remains for L4 orchestration code.

4. **CssBox ↔ Handlers**: ~~`CssBox` directly instantiates `ImageLoadHandler`
   and calls static methods on `BordersDrawHandler` and
   `BackgroundImageDrawHandler`. Handlers in turn reference `CssBox`.~~

   **Resolved**: Defined `IImageLoadHandler`, `IBordersDrawHandler` (with
   `IBorderRenderData`), and `IBackgroundImageDrawHandler` (with
   `IBackgroundRenderData`) in Core. `CssBoxProperties` implements
   `IBorderRenderData` and `IBackgroundRenderData`. Handler classes implement
   interfaces and no longer depend on `CssBox` directly (they accept
   `IBorderRenderData`/`IBackgroundRenderData`/`IImageLoadHandler`).
   `ImageLoadHandler` accepts `IHtmlContainerInt` instead of
   `HtmlContainerInt`.

5. **CssBox ↔ RAdapter/RGraphics/RFont**: ~~`CssBox` painting methods accept
   `RGraphics` and create `RFont` instances through `RAdapter`. These types
   currently live in the main HtmlRenderer assembly.~~

   **Resolved**: Moved `RGraphics` and `RFont` into HtmlRenderer.Adapters.
   `RGraphics` now uses `IResourceFactory` (pens/brushes factory) instead of
   depending on `RAdapter` directly. `RAdapter` implements `IResourceFactory`.
   `CssBox` accesses fonts through `IHtmlContainerInt.GetFont(…)` instead of
   `Adapter.GetFont(…)`.

6. **RAdapter ↔ FontsHandler**: ~~`RAdapter` instantiates `FontsHandler` in its
   constructor (`new FontsHandler(this)`).~~

   **Resolved**: Defined `IFontCreator` interface in Adapters module.
   `FontsHandler` uses `IFontCreator` instead of `RAdapter` directly.
   `RAdapter` implements `IFontCreator`.

## Dependency Graph

```
              HtmlRenderer.Primitives     (L0 — value types)
                       ↑
              HtmlRenderer.Utils          (L1 — pure utilities)
                  ↑        ↑
  HtmlRenderer.Adapters   HtmlRenderer.Core     (L2 — adapters & entities)
          ↑       ↑            ↑
          ↑       └────────────┘
          ↑             HtmlRenderer.CSS        (L3 — CSS parsing)
          ↑                    ↑
          └──── HtmlRenderer ──┘                (L4 — DOM, handlers, orchestration)
```

> Note: HtmlRenderer.Core now depends on HtmlRenderer.Adapters so that
> `IHtmlContainerInt` can reference `RFont`, `RImage`, and `RGraphics` types.

## Naming Convention

All assemblies use the `HtmlRenderer.*` prefix. The naming mirrors the
conceptual layer each module occupies:

| Suffix       | Meaning                                    |
|--------------|--------------------------------------------|
| `.Primitives`| Value types with zero dependencies         |
| `.Utils`     | Pure helper functions                      |
| `.Adapters`  | Platform-abstraction base classes          |
| `.Core`      | Shared entities, interfaces, constants     |
| `.CSS`       | CSS parsing and value resolution           |
| *(none)*     | Façade: DOM, rendering, orchestration      |

## Rationale

- **Incremental progress**: Extracting Adapters, Core, and CSS delivers
  immediate benefits (reduced coupling, faster builds for leaf modules,
  clearer dependency boundaries) without requiring the high-risk refactoring
  needed to separate DOM from HtmlContainerInt.
- **Interface-first decoupling**: `IColorResolver` demonstrates the pattern
  that will be reused for `IHtmlContainerInt` and handler interfaces when the
  DOM extraction becomes feasible.
- **Namespace preservation**: All files retain their original
  `TheArtOfDev.HtmlRenderer.*` namespaces. Consumer code requires no source
  changes; only project references need updating.
- **InternalsVisibleTo**: `HtmlRenderer.Utils` grants internal access to all
  six assemblies so that shared utility internals remain accessible across
  module boundaries without making them public.
- **Logical vs physical separation**: DOM and Rendering are logically
  distinct subsystems within the HtmlRenderer assembly. Creating physical
  projects for them today would require extracting a 15+ member interface
  (`IHtmlContainerInt`) and refactoring every handler. The current phase
  documents the exact steps needed so this work can be done incrementally.

## Consequences

- The solution grows from three to six HtmlRenderer assemblies.
- `InternalsVisibleTo` in `HtmlRenderer.Utils` now lists all six assemblies
  plus the test projects.
- Future extraction of HtmlRenderer.Dom and HtmlRenderer.Rendering is
  well-documented with concrete interface definitions and dependency tables.
- Consumer projects (`HtmlRenderer.WPF`, `HtmlRenderer.Image`, `Broiler.Cli`,
  `Broiler.App`) continue to reference `HtmlRenderer` and receive transitive
  access to all lower-layer assemblies.

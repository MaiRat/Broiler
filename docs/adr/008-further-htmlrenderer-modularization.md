# ADR-008: Further HtmlRenderer Modularization Plan

## Status

Proposed

## Context

ADR-006 and ADR-007 have progressively split the HtmlRenderer monolith into six
assemblies:

| # | Assembly                 | Layer | Contents                                          |
|---|--------------------------|-------|---------------------------------------------------|
| 1 | HtmlRenderer.Primitives  | L0    | Value types (RColor, RPoint, RRect, RSize, …)     |
| 2 | HtmlRenderer.Utils       | L1    | Pure utilities (ArgChecker, CommonUtils, …)        |
| 3 | HtmlRenderer.Adapters    | L2a   | Abstract adapters (RBrush, RPen, RImage, …)        |
| 4 | HtmlRenderer.Core        | L2b   | Entities, CssData, CssDefaults, interfaces         |
| 5 | HtmlRenderer.CSS         | L3    | CssParser, CssValueParser, RegexParser             |
| 6 | HtmlRenderer             | L4    | DOM, handlers, orchestration (façade)              |

The L4 façade still bundles four logically distinct subsystems:

1. **DOM tree** — `CssBox` hierarchy, layout engine, CSS properties, line
   boxes, word rectangles.
2. **Rendering handlers** — Drawing handlers (borders, background images),
   interaction handlers (selection, context menu), loading handlers (images,
   stylesheets, fonts).
3. **Orchestration** — `HtmlContainerInt`, `DomParser`, `HtmlParser`,
   `HtmlRendererUtils`.
4. **Heavy utilities** — `CssUtils`, `DomUtils`, `RenderUtils`.

This ADR documents the dependency analysis for each subsystem and proposes a
concrete plan for further extraction, including the interface decoupling
required to break remaining circular dependencies.

## Dependency Analysis of Remaining L4 Components

### DOM (Core/Dom/)

| File                    | Depends on HtmlContainerInt | Via interface? | Depends on RAdapter | Other L4 deps              |
|-------------------------|-----------------------------|----------------|---------------------|----------------------------|
| CssBox.cs               | Yes (field)                 | IHtmlContainerInt ✅ | No              | Handlers (ImageLoadHandler), Utils (CssUtils, DomUtils, RenderUtils) |
| CssBoxProperties.cs     | No                          | —              | No                  | CssValueParser (L3)        |
| CssBoxHelper.cs         | No                          | —              | No                  | CssBox                     |
| CssBoxHr.cs             | No                          | —              | No                  | CssBox                     |
| CssBoxImage.cs          | Yes                         | IHtmlContainerInt ✅ | No              | ImageLoadHandler           |
| CssLayoutEngine.cs      | Yes                         | IHtmlContainerInt ✅ | No              | CssUtils, DomUtils         |
| CssLayoutEngineTable.cs | No                          | —              | No                  | CssBox, CssLayoutEngine    |
| CssLineBox.cs           | No                          | —              | No                  | CssBox, CssRect            |
| CssRect.cs              | No                          | —              | No                  | Adapters only              |
| CssRectImage.cs         | No                          | —              | No                  | CssRect                    |
| CssRectWord.cs          | No                          | —              | No                  | CssRect                    |
| CssSpacingBox.cs        | No                          | —              | No                  | CssBox                     |
| HoverBoxBlock.cs        | No                          | —              | No                  | CssBox                     |
| HtmlTag.cs              | No                          | —              | No                  | None                       |

**Key finding**: DOM types access `HtmlContainerInt` exclusively through the
`IHtmlContainerInt` interface (resolved in ADR-007). No DOM type references
`RAdapter` directly. The remaining coupling is to handler implementations
(`ImageLoadHandler`) and heavy utilities (`CssUtils`, `DomUtils`,
`RenderUtils`).

### Handlers (Core/Handlers/)

| Handler                    | Depends on CssBox | Depends on HtmlContainerInt | Via interface? | Depends on RAdapter |
|----------------------------|-------------------|-----------------------------|----------------|---------------------|
| BackgroundImageDrawHandler | No                | No                          | —              | No                  |
| BordersDrawHandler         | No (IBorderRenderData) | No                     | —              | No                  |
| ImageLoadHandler           | No                | Yes                         | IHtmlContainerInt ✅ | No             |
| ImageDownloader            | No                | No                          | —              | No                  |
| FontsHandler               | No                | No                          | IFontCreator ✅ | No               |
| StylesheetLoadHandler      | No                | **Yes (direct)**            | **No** ❌      | No                  |
| SelectionHandler           | **Yes (direct)**  | Through CssBox              | —              | No                  |
| ContextMenuHandler         | **Yes (direct)**  | **Yes (direct)**            | **No** ❌      | No                  |

**Key finding**: Handlers divide into two categories:

- **Already decoupled** (interface-based): `BackgroundImageDrawHandler`,
  `BordersDrawHandler`, `ImageLoadHandler`, `ImageDownloader`, `FontsHandler`.
  These depend only on interfaces and adapter abstractions.
- **Tightly coupled**: `StylesheetLoadHandler` (takes `HtmlContainerInt`
  directly), `SelectionHandler` (stores `CssBox` root), `ContextMenuHandler`
  (stores both `CssBox` and `HtmlContainerInt` directly).

### Parse / Orchestration

| File                | Depends on CssBox | Depends on HtmlContainerInt | Via interface? |
|---------------------|-------------------|-----------------------------|----------------|
| DomParser.cs        | Yes (creates)     | **Yes (direct)**            | **No** ❌      |
| HtmlParser.cs       | Yes (creates)     | No                          | —              |
| HtmlContainerInt.cs | Yes (Root field)  | —                           | —              |
| HtmlRendererUtils.cs| No                | **Yes (direct)**            | **No** ❌      |

**Key finding**: `DomParser` assigns `HtmlContainerInt` to the root CssBox via
the concrete `HtmlContainer` property. `HtmlRendererUtils` calls
`HtmlContainerInt.PerformLayout()` directly.

### Adapters (RAdapter, RControl, RContextMenu)

| File           | Depends on Handlers | Depends on DOM | Depends on Parse |
|----------------|---------------------|----------------|------------------|
| RAdapter.cs    | FontsHandler        | No             | CssDataParser    |
| RControl.cs    | No                  | No             | No               |
| RContextMenu.cs| No                  | No             | No               |

**Key finding**: `RAdapter` depends on `FontsHandler` (which is already
interface-decoupled via `IFontCreator`) and `CssDataParser` (in L3 CSS module).
`RControl` and `RContextMenu` have no L4 dependencies.

### Heavy Utilities

| Utility       | Depends on CssBox | Depends on HtmlContainerInt | Via interface? |
|---------------|-------------------|-----------------------------|----------------|
| CssUtils      | Yes (CssBoxProperties) | No                      | —              |
| DomUtils      | **Yes (direct)**  | No                          | —              |
| RenderUtils   | **Yes (direct)**  | Yes                         | IHtmlContainerInt ✅ |

**Key finding**: All three utilities depend on `CssBox`/`CssBoxProperties`.
They are logically part of the DOM subsystem and cannot be separated from it.

## Proposed Target Modules

### Phase 1 — HtmlRenderer.Rendering (Low Risk)

Extract the five already-decoupled handlers and drawing utilities into a
dedicated assembly:

**Contents**:

- `BackgroundImageDrawHandler` — uses `IBackgroundRenderData`, no CssBox
- `BordersDrawHandler` — uses `IBorderRenderData`, no CssBox
- `ImageLoadHandler` — uses `IHtmlContainerInt` (interface)
- `ImageDownloader` — pure utility, no DOM/orchestration deps
- `FontsHandler` — uses `IFontCreator` (interface)
- Embedded images: `ImageLoad.png`, `ImageError.png` (currently in
  `Core/Utils/`)

**Dependencies**: HtmlRenderer.Primitives, HtmlRenderer.Utils,
HtmlRenderer.Adapters, HtmlRenderer.Core.

**Why feasible now**: These handlers already communicate through interfaces
defined in HtmlRenderer.Core (`IImageLoadHandler`, `IBordersDrawHandler`,
`IBackgroundImageDrawHandler`, `IFontCreator`, `IHtmlContainerInt`). No
circular dependencies exist.

**Remaining in L4**: `SelectionHandler`, `ContextMenuHandler`,
`StylesheetLoadHandler` stay in the façade due to direct `CssBox` and
`HtmlContainerInt` dependencies.

### Phase 2 — HtmlRenderer.Dom (Medium Risk)

Extract DOM tree types and their tightly coupled utilities into a dedicated
assembly:

**Contents**:

- All `Core/Dom/` files: `CssBox`, `CssBoxHelper`, `CssBoxHr`, `CssBoxImage`,
  `CssBoxProperties`, `CssLayoutEngine`, `CssLayoutEngineTable`, `CssLineBox`,
  `CssRect`, `CssRectImage`, `CssRectWord`, `CssSpacingBox`, `HoverBoxBlock`,
  `HtmlTag`
- `Core/Utils/CssUtils` — depends on `CssBoxProperties` (DOM type)
- `Core/Utils/DomUtils` — depends on `CssBox` (DOM type)
- `Core/Utils/RenderUtils` — depends on `CssBox` (DOM type)
- `Core/Parse/HtmlParser` — creates `CssBox` instances, no orchestration deps

**Dependencies**: HtmlRenderer.Primitives, HtmlRenderer.Utils,
HtmlRenderer.Adapters, HtmlRenderer.Core, HtmlRenderer.CSS,
HtmlRenderer.Rendering.

**Prerequisites** (interface decoupling needed):

1. **`IStylesheetLoader` interface** — `DomParser` calls
   `StylesheetLoadHandler.LoadStylesheet(htmlContainer, …)`. To break this
   dependency, define `IStylesheetLoader` in Core:

   ```csharp
   internal interface IStylesheetLoader
   {
       void LoadStylesheet(string src, Dictionary<string, string> attributes,
                           out string stylesheet, out CssData stylesheetData);
   }
   ```

   `StylesheetLoadHandler` (remaining in L4) implements this interface.
   `DomParser` accepts `IStylesheetLoader` instead of calling the static method
   directly.

2. **Move `CssBox.HtmlContainer` concrete property behind factory** — The
   concrete `HtmlContainer` property on `CssBox` (returning
   `HtmlContainerInt`) is only used by orchestration code. Refactor the
   assignment in `DomParser` to use `IHtmlContainerInt`:

   ```csharp
   // Before (DomParser.cs):
   root.HtmlContainer = htmlContainer;

   // After:
   root.ContainerInt = htmlContainer;  // uses IHtmlContainerInt
   ```

   Then remove the concrete `HtmlContainer` property from `CssBox`.
   Orchestration code (`SelectionHandler`, `ContextMenuHandler`) that needs the
   concrete type can cast from `ContainerInt` when required.

3. **Decouple `ImageLoadHandler` instantiation** — `CssBoxImage` creates
   `ImageLoadHandler` directly. After Phase 1 moves `ImageLoadHandler` to
   HtmlRenderer.Rendering, `CssBoxImage` should receive it via
   `IHtmlContainerInt`:

   ```csharp
   internal interface IHtmlContainerInt
   {
       // ... existing members ...
       IImageLoadHandler CreateImageLoadHandler(
           ActionInt<RImage, RRect, bool> loadCompleteCallback);
   }
   ```

**Why medium risk**: Requires three interface decoupling changes. Each is
localised and follows the established `IColorResolver` / `IHtmlContainerInt`
pattern.

### Phase 3 — HtmlRenderer.Orchestration (High Risk)

Extract orchestration into its own assembly, leaving the top-level façade as a
thin wiring layer:

**Contents**:

- `HtmlContainerInt` — The central orchestrator
- `DomParser` — DOM tree construction from HTML
- `HtmlRendererUtils` — Layout measurement helpers
- `StylesheetLoadHandler` — Stylesheet loading (depends on `HtmlContainerInt`)

**Dependencies**: HtmlRenderer.Primitives, HtmlRenderer.Utils,
HtmlRenderer.Adapters, HtmlRenderer.Core, HtmlRenderer.CSS,
HtmlRenderer.Rendering, HtmlRenderer.Dom.

**Prerequisites** (additional interface decoupling):

1. **`ISelectionHandler` interface** — `HtmlContainerInt` creates and drives
   `SelectionHandler`. Define an interface in Core:

   ```csharp
   internal interface ISelectionHandler : IDisposable
   {
       void HandleMouseDown(RControl parent, RPoint loc, bool isShiftDown);
       void HandleMouseUp(RControl parent, RPoint loc);
       void HandleMouseMove(RControl parent, RPoint loc);
       void HandleMouseDoubleClick(RControl parent, RPoint loc);
       void HandleKeyDown(RControl parent, bool isControlDown);
       string GetSelectedText();
       void SelectAll();
       void ClearSelection();
   }
   ```

2. **`IContextMenuHandler` interface** — `SelectionHandler` creates
   `ContextMenuHandler`. Define an analogous interface in Core.

3. **Factory pattern for handler creation** — `HtmlContainerInt` instantiates
   `SelectionHandler`, `ImageDownloader`, and other handlers. Introduce a
   factory interface so the orchestrator does not depend on concrete handler
   constructors:

   ```csharp
   internal interface IHandlerFactory
   {
       ISelectionHandler CreateSelectionHandler(CssBox root);
       IImageLoadHandler CreateImageLoadHandler(
           IHtmlContainerInt container,
           ActionInt<RImage, RRect, bool> callback);
   }
   ```

**Why high risk**: `HtmlContainerInt` is the central hub connecting DOM,
handlers, adapters, and parsers. Extracting it requires interfaces for every
subsystem it touches. The factory-pattern refactoring is extensive but follows
established patterns.

**Remaining in HtmlRenderer (Thin Façade)**:

After all three phases, the L4 façade retains only:

- `RAdapter` — Implements `IColorResolver`, `IResourceFactory`, `IFontCreator`;
  references `FontsHandler` via `IFontCreator`
- `RControl` — Abstract control (references `RAdapter`)
- `RContextMenu` — Abstract context menu
- `SelectionHandler` — Direct `CssBox` access for selection
- `ContextMenuHandler` — Direct `CssBox` and `HtmlContainerInt` access
- `IHandlerFactory` implementation — Wires concrete handlers

### Target Dependency Graph (After All Phases)

```
                 HtmlRenderer.Primitives           (L0)
                          ↑
                 HtmlRenderer.Utils                (L1)
                    ↑         ↑
    HtmlRenderer.Adapters   HtmlRenderer.Core      (L2)
            ↑       ↑           ↑
            ↑       └───────────┘
            ↑           HtmlRenderer.CSS           (L3)
            ↑                ↑
            ↑       HtmlRenderer.Rendering         (L3a — decoupled handlers)
            ↑         ↑          ↑
            ↑     HtmlRenderer.Dom                 (L4a — DOM tree + layout)
            ↑              ↑
            ↑     HtmlRenderer.Orchestration       (L5  — wiring + parsing)
            ↑              ↑
            └── HtmlRenderer ──┘                   (L6  — thin façade)
```

## Prioritised Incremental Steps

### Priority 1 — Interface Groundwork (No New Projects)

These steps prepare for physical extraction without creating new assemblies.
They can be done incrementally and merged independently:

1. **Define `IStylesheetLoader`** in HtmlRenderer.Core. Refactor `DomParser`
   to accept `IStylesheetLoader` as a constructor parameter instead of calling
   `StylesheetLoadHandler` statically.
2. **Add `CreateImageLoadHandler`** to `IHtmlContainerInt`. Refactor
   `CssBoxImage` to use the factory method instead of instantiating
   `ImageLoadHandler` directly.
3. **Remove concrete `HtmlContainer` property** from `CssBox`. Replace all
   usages in L4 orchestration code with casts from `ContainerInt` where the
   concrete type is genuinely needed.

### Priority 2 — Extract HtmlRenderer.Rendering

4. Create `HtmlRenderer.Rendering.csproj` with the five decoupled handlers and
   embedded resource images.
5. Update `HtmlRenderer.csproj` to reference `HtmlRenderer.Rendering`.
6. Add `HtmlRenderer.Rendering` to `InternalsVisibleTo` in
   `HtmlRenderer.Utils`.
7. Move embedded images (`ImageLoad.png`, `ImageError.png`) to the new project;
   update `RAdapter.GetLoadingImage()`/`GetLoadingFailedImage()` to reference
   the new assembly.

### Priority 3 — Extract HtmlRenderer.Dom

8. Create `HtmlRenderer.Dom.csproj` with DOM types, heavy utilities, and
   `HtmlParser`.
9. Update `HtmlRenderer.csproj` to reference `HtmlRenderer.Dom`.
10. Add `HtmlRenderer.Dom` to `InternalsVisibleTo` in `HtmlRenderer.Utils`.
11. Verify that `SelectionHandler` and `ContextMenuHandler` (remaining in L4)
    can still access `CssBox` through the new project reference.

### Priority 4 — Extract HtmlRenderer.Orchestration (Future)

12. Define `ISelectionHandler` and `IContextMenuHandler` in Core.
13. Define `IHandlerFactory` in Core.
14. Create `HtmlRenderer.Orchestration.csproj` with `HtmlContainerInt`,
    `DomParser`, `HtmlRendererUtils`, `StylesheetLoadHandler`.
15. Implement `IHandlerFactory` in the thin façade.
16. Update all consumer projects.

## Constraints and Risks

### Circular Dependency: SelectionHandler ↔ ContextMenuHandler

`SelectionHandler` creates `ContextMenuHandler` in its constructor.
`ContextMenuHandler` receives `SelectionHandler` as a parameter. This mutual
dependency means both must reside in the same assembly. If both are extracted
to HtmlRenderer.Dom or kept in the façade, no cycle occurs. Splitting them
across different assemblies is not feasible without introducing a mediator or
event-based decoupling.

### InternalsVisibleTo Growth

Each new assembly must be added to `InternalsVisibleTo` in
`HtmlRenderer.Utils` (and potentially in other projects). The attribute list
will grow; consider consolidating into a shared `.targets` file when it exceeds
eight entries.

### Namespace Preservation

All files retain their original `TheArtOfDev.HtmlRenderer.*` namespaces even
when moved to new projects. This ensures binary compatibility for consumers
that reference types by fully qualified name.

### Test Impact

Existing test projects (`HtmlRenderer.Image.Tests`, `Broiler.Cli.Tests`,
`Broiler.App.Tests`) reference `HtmlRenderer` and receive transitive access to
lower layers. No test changes are required for Phase 1 or Phase 2. Phase 3
(Orchestration extraction) may require updating test project references if
tests directly instantiate `HtmlContainerInt`.

## Decision

Adopt the phased approach described above:

1. **Phase 1** (HtmlRenderer.Rendering) is low-risk and delivers immediate
   modularity benefits for the five already-decoupled handlers.
2. **Phase 2** (HtmlRenderer.Dom) requires targeted interface additions
   (`IStylesheetLoader`, `CreateImageLoadHandler` on `IHtmlContainerInt`) but
   follows established patterns from ADR-007.
3. **Phase 3** (HtmlRenderer.Orchestration) is deferred until Phases 1–2 are
   stable and the factory-pattern refactoring is validated.

Each phase is designed to be independently mergeable. Interface decoupling
(Priority 1) should proceed first, even before physical project extraction, to
validate the design and eliminate risk.

## Consequences

- The solution will grow from six to nine HtmlRenderer assemblies at full
  completion.
- Each extracted module can be independently versioned and referenced.
- Consumer projects continue to reference `HtmlRenderer` (façade) and receive
  transitive access to all layers.
- The interface-first approach ensures that each physical split is preceded by
  a validated logical separation, minimising the risk of rework.
- Future contributors can identify which phase each component belongs to using
  this ADR as a reference map.

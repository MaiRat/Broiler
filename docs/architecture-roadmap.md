# Architecture Roadmap: Engine Separation (Style / Layout / Paint / Raster)

> **Decision**: The full separation requires touching nearly every method in
> `CssBox` (≈900 lines of layout + ≈100 lines of paint) plus `CssLayoutEngine`
> (≈600 lines), `CssBoxImage`, `CssBoxHr`, `CssBoxHelper`, and
> `HtmlContainerInt`. A big-bang refactor carries high regression risk.
>
> This roadmap defines **four incremental phases**, each independently
> shippable and testable.
>
> All effort estimates are **person-days for a single developer** familiar
> with the codebase.

---

## Phase 0 — Preparation (current PR)

**Goal**: Document the current pipeline, target pipeline, and concrete
violations so every future PR has clear acceptance criteria.

**Deliverables** (this PR):
- `docs/architecture-separation.md` — current vs target pipeline, IR
  pseudo-interfaces, and five concrete leak-check violations.
- `docs/architecture-roadmap.md` — this file.

**Effort**: Small (documentation only).
**Risk**: None — no code changes.

---

## Phase 1 — Introduce IR Structs Without Changing Behavior

**Goal**: Add the new IR types alongside existing code. Existing code paths
are unchanged; new types are populated in parallel as "shadow" data.

### Steps

1. **Add `ComputedStyle` record** to `HtmlRenderer.Core`.
   - Read-only, init-only properties for every CSS property that layout
     and paint currently access on `CssBoxProperties`.
   - Add a factory method `ComputedStyle.From(CssBoxProperties box)` that
     snapshots the current lazy-parsed values.

2. **Add `Fragment` / `LineFragment` / `InlineFragment`** records to
   `HtmlRenderer.Core`.
   - After `CssBox.PerformLayout()` completes, build a `Fragment` tree
     by walking the `CssBox` tree and copying geometry.
   - This is a read-only snapshot; no code consumes it yet.

3. **Add `DisplayList` / `DisplayItem` types** to `HtmlRenderer.Core`.
   - A recording `RGraphics` adapter (`RecordingGraphics`) that implements
     `RGraphics` but appends `DisplayItem` entries instead of drawing.
   - Wire it into `HtmlContainerInt` as an optional second paint pass.

4. **Add `IRasterBackend`** interface to `HtmlRenderer.Core`.
   - Single method `Render(DisplayList, surface)`.
   - Implement `SkiaRasterBackend` that replays `DisplayItem` entries
     onto an `SKCanvas`.

### Verification

- All existing tests pass unchanged.
- New integration test: render reference HTML through both the old path
  and the new IR path → assert pixel-identical output.

**Effort**: ~2–3 days.
**Risk**: Low — no existing behavior changes; purely additive types.

---

## Phase 2 — Layout Consumes Only LayoutNode + ComputedStyle

**Goal**: Decouple layout from raw DOM. Layout methods receive `LayoutNode`
(or a wrapper) instead of calling `HtmlTag.GetAttribute()` or checking
tag names.

### Steps

1. **Add `BoxKind` enum** (`Block`, `Inline`, `ReplacedImage`,
   `ReplacedIframe`, `TableCell`, `ListItem`, …) to `ComputedStyle`.
   - Populate in `DomParser.CascadeApplyStyles()` based on tag name.
   - This replaces all `tag.Name == HtmlConstants.Img` checks.

2. **Move list-attribute reads to style phase.**
   - Add `ListStart`, `ListReversed` to `ComputedStyle`.
   - `DomParser` reads `<ol start="…" reversed>` and sets these.
   - `CssBox.GetIndexForList()` reads from `ComputedStyle` instead of
     `GetAttribute()`.

3. **Move image / background-image loading to a resource-resolution pass**
   before layout.
   - New method `HtmlContainerInt.ResolveResources()` called between
     `GenerateCssTree()` and `PerformLayout()`.
   - `CssBoxImage` and `CssBox.MeasureWordsSize()` receive pre-loaded
     dimensions rather than triggering async loads.

4. **Audit remaining `GetAttribute()` / `HtmlTag` accesses in layout code.**
   - Replace each with a property on `ComputedStyle` or `LayoutNode`.

### Verification

- All existing rendering tests pass (especially float / clear / table
  tests).
- New unit test: construct a `LayoutNode` tree manually → run layout →
  verify geometry output matches expected values.

**Effort**: ~3–5 days.
**Risk**: Medium — touches core layout; requires careful regression testing.
Float collision and table layout are the highest-risk areas.

---

## Phase 3 — Paint Consumes Only FragmentTree

**Goal**: `PaintImp()` reads from `Fragment` records instead of `CssBox`
fields. The `DisplayList` becomes the sole output of paint.

### Steps

1. **Replace `CssBox.PaintImp()`** with a standalone `PaintWalker` class
   that receives a `Fragment` tree.
   - `PaintWalker` produces `DisplayItem` entries.
   - Stacking context sorting is done on `Fragment.StackLevel`.

2. **Replace direct `RGraphics` calls** in `BordersDrawHandler` and
   `BackgroundImageDrawHandler` with `DisplayItem` emission.
   - The handlers already receive interfaces (`IBorderRenderData`,
     `IBackgroundRenderData`); map these to `Fragment` + `ComputedStyle`.

3. **Remove `CssBox.Paint()` / `PaintImp()`** methods.
   - Paint is now fully separate from the DOM.

4. **Wire the `IRasterBackend`** as the sole rendering path.
   - `HtmlContainerInt.PerformPaint()` becomes:
     ```
     var fragments = BuildFragmentTree(root);
     var displayList = painter.Paint(fragments);
     backend.Render(displayList, surface);
     ```

### Verification

- Pixel-identical output for all existing test cases.
- New test: serialize `DisplayList` to JSON → snapshot-test against
  expected output for a set of reference HTML files.

**Effort**: ~5–8 days.
**Risk**: High — replaces the entire paint path. Must be done behind a
feature flag or with a parallel old-path fallback until fully validated.

---

## Phase 4 — Optional: Incremental Invalidation & Caching

**Goal**: Exploit the clean IR boundaries to cache and incrementally update
each phase.

### Steps

1. **Style caching**: If only a CSS class changes, recompute
   `ComputedStyle` for affected subtree only.

2. **Layout caching**: Cache `Fragment` subtrees keyed by
   `(LayoutNode, available-width)`. Skip re-layout for unchanged subtrees.

3. **Display list diffing**: Compare old and new `DisplayList`; repaint
   only changed items.

4. **Raster tiling**: Split the surface into tiles; re-raster only tiles
   whose display-list region changed.

### Verification

- Benchmark: measure layout + paint time for large documents before/after
  caching.
- Correctness: randomized mutation tests (change one CSS property → verify
  output matches full re-render).

**Effort**: ~2–4 weeks.
**Risk**: Medium — performance optimization; functional correctness is the
main concern.

---

## Summary

| Phase | Scope | Effort | Risk | Behavior Change |
|-------|-------|--------|------|-----------------|
| 0 | Documentation | ½ day | None | No |
| 1 | Add IR types (shadow) | 2–3 days | Low | No |
| 2 | Layout decoupled from DOM | 3–5 days | Medium | No (same output) |
| 3 | Paint decoupled from CssBox | 5–8 days | High | No (same output) |
| 4 | Incremental caching | 2–4 weeks | Medium | No (same output) |

Each phase is independently shippable. Phases 1–3 must produce
pixel-identical output for all existing tests (no behavior regressions).

### Key Files per Phase

| File | Phase 1 | Phase 2 | Phase 3 | Phase 4 |
|------|:-------:|:-------:|:-------:|:-------:|
| `HtmlRenderer.Core/ComputedStyle.cs` | ✦ new | ✎ | | |
| `HtmlRenderer.Core/Fragment.cs` | ✦ new | | ✎ | ✎ |
| `HtmlRenderer.Core/DisplayList.cs` | ✦ new | | ✎ | ✎ |
| `HtmlRenderer.Core/IRasterBackend.cs` | ✦ new | | ✎ | |
| `HtmlRenderer.Dom/CssBox.cs` | | ✎ | ✎ remove paint | |
| `HtmlRenderer.Dom/CssBoxProperties.cs` | | ✎ | | |
| `HtmlRenderer.Dom/CssBoxImage.cs` | | ✎ | ✎ | |
| `HtmlRenderer.Dom/CssBoxHelper.cs` | | ✎ | | |
| `HtmlRenderer.Dom/CssLayoutEngine.cs` | | ✎ | | ✎ |
| `HtmlRenderer.Orchestration/HtmlContainerInt.cs` | ✎ | ✎ | ✎ | ✎ |
| `HtmlRenderer.Orchestration/DomParser.cs` | | ✎ | | |
| `HtmlRenderer.Rendering/BordersDrawHandler.cs` | | | ✎ | |
| `HtmlRenderer.Rendering/BackgroundImageDrawHandler.cs` | | | ✎ | |
| `HtmlRenderer.Image/SkiaRasterBackend.cs` | ✦ new | | ✎ | |

✦ = new file, ✎ = modified

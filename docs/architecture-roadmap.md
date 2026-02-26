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

**Status**: ✅ Complete (IR types and shadow building implemented)

**Goal**: Add the new IR types alongside existing code. Existing code paths
are unchanged; new types are populated in parallel as "shadow" data.

### Steps

1. ✅ **Add `ComputedStyle` record** to `HtmlRenderer.Core`.
   - Read-only, init-only properties for every CSS property that layout
     and paint currently access on `CssBoxProperties`.
   - Factory builder `ComputedStyleBuilder.FromBox(CssBoxProperties)` in
     `HtmlRenderer.Orchestration` snapshots the current lazy-parsed values.

2. ✅ **Add `Fragment` / `LineFragment` / `InlineFragment`** records to
   `HtmlRenderer.Core`.
   - `FragmentTreeBuilder.Build(CssBox)` in `HtmlRenderer.Orchestration`
     walks the `CssBox` tree after layout and copies geometry.
   - This is a read-only snapshot; no code consumes it yet.

3. ✅ **Add `DisplayList` / `DisplayItem` types** to `HtmlRenderer.Core`.
   - Type definitions for all display-list primitives (`FillRectItem`,
     `DrawBorderItem`, `DrawTextItem`, `DrawImageItem`, `ClipItem`,
     `RestoreItem`, `OpacityItem`).
   - `RecordingGraphics` adapter deferred to Phase 3 (paint decoupling).

4. ✅ **Add `IRasterBackend`** interface to `HtmlRenderer.Core`.
   - Single method `Render(DisplayList, surface)`.
   - Concrete `SkiaRasterBackend` implementation deferred to Phase 3.

5. ✅ **Wire shadow fragment-tree building** into `HtmlContainerInt`.
   - `PerformLayout()` now builds a `Fragment` tree after layout completes.
   - Stored as `LatestFragmentTree` for validation; not consumed by paint.

### Verification

- ✅ All existing tests pass unchanged.
- ✅ 20 new IR-type unit tests pass (BoxEdges, ComputedStyle, Fragment,
  DisplayList, shadow building integration).

### New Files

| File | Project |
|------|---------|
| `Core/IR/BoxEdges.cs` | `HtmlRenderer.Core` |
| `Core/IR/ComputedStyle.cs` | `HtmlRenderer.Core` |
| `Core/IR/Fragment.cs` | `HtmlRenderer.Core` |
| `Core/IR/DisplayList.cs` | `HtmlRenderer.Core` |
| `Core/IR/IRasterBackend.cs` | `HtmlRenderer.Core` |
| `Core/IR/ComputedStyleBuilder.cs` | `HtmlRenderer.Orchestration` |
| `Core/IR/FragmentTreeBuilder.cs` | `HtmlRenderer.Orchestration` |
| `IRTypesTests.cs` | `HtmlRenderer.Image.Tests` |

**Effort**: ~2–3 days.
**Risk**: Low — no existing behavior changes; purely additive types.

---

## Phase 2 — Layout Consumes Only LayoutNode + ComputedStyle

**Status**: 🚧 In Progress (BoxKind, list attributes, image source implemented)

**Goal**: Decouple layout from raw DOM. Layout methods receive `LayoutNode`
(or a wrapper) instead of calling `HtmlTag.GetAttribute()` or checking
tag names.

### Steps

1. ✅ **Add `BoxKind` enum** (`Block`, `Inline`, `ReplacedImage`,
   `ReplacedIframe`, `TableCell`, `ListItem`, …) to `ComputedStyle`.
   - Populated in `DomParser.CascadeApplyStyles()` based on tag name.
   - This replaces all `tag.Name == HtmlConstants.Img` checks.

2. ✅ **Move list-attribute reads to style phase.**
   - Added `ListStart`, `ListReversed` to `ComputedStyle` and `CssBoxProperties`.
   - `DomParser` reads `<ol start="…" reversed>` and sets these.
   - `CssBox.GetIndexForList()` reads from `CssBoxProperties` instead of
     `GetAttribute()`.

3. ✅ **Add image source to style phase.**
   - Added `ImageSource` to `ComputedStyle` and `CssBoxProperties`.
   - `DomParser` reads `<img src="…">` and sets `ImageSource`.
   - Full image/background-image resource-resolution pass deferred to
     follow-up (requires deeper `CssBoxImage` changes).

4. **Audit remaining `GetAttribute()` / `HtmlTag` accesses in layout code.**
   - Replace each with a property on `ComputedStyle` or `LayoutNode`.
   - Remaining accesses: background-image load in `CssBox.MeasureWordsSize()`,
     image load in `CssBoxImage.PaintImp()` / `MeasureWordsSize()`.

### New/Modified Files

| File | Change |
|------|--------|
| `HtmlRenderer.Core/Core/IR/BoxKind.cs` | ✦ new — enum classifying element roles |
| `HtmlRenderer.Core/Core/IR/ComputedStyle.cs` | ✎ added Kind, ListStart, ListReversed, ImageSource |
| `HtmlRenderer.Dom/Core/Dom/CssBoxProperties.cs` | ✎ added Kind, ListStart, ListReversed, ImageSource |
| `HtmlRenderer.Dom/Core/Dom/CssBox.cs` | ✎ GetIndexForList() reads from CssBoxProperties |
| `HtmlRenderer.Orchestration/Core/Parse/DomParser.cs` | ✎ AssignBoxKindAndAttributes() |
| `HtmlRenderer.Orchestration/Core/IR/ComputedStyleBuilder.cs` | ✎ snapshots new properties |
| `HtmlRenderer.Image.Tests/IRTypesTests.cs` | ✎ 27 new Phase 2 tests |

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

**Status**: ✅ Complete (PaintWalker handles background images, replaced images, selection; old path retained as fallback)

**Goal**: `PaintImp()` reads from `Fragment` records instead of `CssBox`
fields. The `DisplayList` becomes the sole output of paint.

### Steps

1. ✅ **Replace `CssBox.PaintImp()`** with a standalone `PaintWalker` class
   that receives a `Fragment` tree.
   - `PaintWalker` produces `DisplayItem` entries.
   - Stacking context sorting is done on `Fragment.StackLevel`.
   - Handles background colors, borders, text, text decoration,
     overflow clipping, and child ordering.

2. ✅ **Create `RGraphicsRasterBackend`** implementing `IRasterBackend`.
   - Bridges `DisplayList` back to `RGraphics` for rendering.
   - Handles `FillRectItem`, `DrawBorderItem`, `DrawTextItem`,
     `DrawImageItem`, `DrawLineItem`, `ClipItem`, `RestoreItem`.

3. ✅ **Wire new paint path** with feature flag (`UseNewPaintPath`).
   - `HtmlContainerInt.PerformPaint()` supports both old and new paths.

4. ✅ **Extend IR types** for paint requirements.
   - `DrawBorderItem`: per-side styles, corner radii.
   - `DrawTextItem`: `FontHandle`, `IsRtl`.
   - `InlineFragment`: `FontHandle` captured during fragment building.
   - Added `DrawLineItem` for text decoration.
   - `DisplayItem`: `JsonDerivedType` attributes for snapshot serialization.

5. ✅ **Handle background images** in `PaintWalker`.
   - `CssBox.LoadedBackgroundImage` exposes the loaded background image handle.
   - `Fragment.BackgroundImageHandle` captures it during fragment building.
   - `PaintWalker.EmitBackgroundImage()` emits `DrawImageItem` for background images.

6. ✅ **Handle replaced images** (e.g. `<img>` elements) in `PaintWalker`.
   - `Fragment.ImageHandle` and `Fragment.ImageSourceRect` capture the loaded
     image from `CssBoxImage` during fragment building.
   - `PaintWalker.EmitReplacedImage()` emits `DrawImageItem` for replaced images.

7. ✅ **Handle selection rendering** in `PaintWalker`.
   - `InlineFragment.Selected`, `SelectedStartOffset`, `SelectedEndOffset`
     capture selection state from `CssRect` during fragment building.
   - `PaintWalker.EmitSelection()` emits `FillRectItem` for selection highlights.

8. **Remove `CssBox.Paint()` / `PaintImp()`** methods.
   - Deferred: old paint path retained as fallback until full validation.

9. **Replace direct `RGraphics` calls** in `BordersDrawHandler` and
   `BackgroundImageDrawHandler` with `DisplayItem` emission.
   - Deferred: handlers still used by old paint path; new path bypasses
     them via `PaintWalker` → `DisplayItem` emission.

### New/Modified Files

| File | Change |
|------|--------|
| `HtmlRenderer.Orchestration/Core/IR/PaintWalker.cs` | ✦ new — background images, replaced images, selection |
| `HtmlRenderer.Orchestration/Core/IR/RGraphicsRasterBackend.cs` | ✦ new |
| `HtmlRenderer.Core/Core/IR/DisplayList.cs` | ✎ extended |
| `HtmlRenderer.Core/Core/IR/Fragment.cs` | ✎ BackgroundImageHandle, ImageHandle, ImageSourceRect, selection props |
| `HtmlRenderer.Orchestration/Core/IR/FragmentTreeBuilder.cs` | ✎ captures images and selection |
| `HtmlRenderer.Dom/Core/Dom/CssBox.cs` | ✎ LoadedBackgroundImage property |
| `HtmlRenderer.Orchestration/Core/HtmlContainerInt.cs` | ✎ new paint path |
| `HtmlRenderer.Image.Tests/IRTypesTests.cs` | ✎ 37 new tests |

### Verification

- ✅ All 218 tests pass (181 existing + 37 new Phase 1–3 tests).
- ✅ DisplayList JSON serialization with polymorphic type discriminators.
- ✅ Snapshot stability test.
- Old paint path unaffected (feature flag defaults to off).

**Effort**: ~5–8 days.
**Risk**: High — replaces the entire paint path. Feature flag
(`UseNewPaintPath`) provides parallel old-path fallback until fully validated.

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

| Phase | Scope | Effort | Risk | Behavior Change | Status |
|-------|-------|--------|------|-----------------|--------|
| 0 | Documentation | ½ day | None | No | ✅ Complete |
| 1 | Add IR types (shadow) | 2–3 days | Low | No | ✅ Complete |
| 2 | Layout decoupled from DOM | 3–5 days | Medium | No (same output) | 🚧 In Progress |
| 3 | Paint decoupled from CssBox | 5–8 days | High | No (same output) | ✅ Complete |
| 4 | Incremental caching | 2–4 weeks | Medium | No (same output) | |

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

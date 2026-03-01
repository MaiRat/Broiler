# Roadmap: Resolve Differences Between html-renderer and Chromium

> **Scope:** Fix all rendering discrepancies between the html-renderer engine
> (Broiler) and headless Chromium (Playwright) documented in
> [`css2-differential-verification.md`](../css2-differential-verification.md).
>
> **Tracking Issue:** [#191](https://github.com/MaiRat/Broiler/issues/191)
>
> **Previous Work:**
> - [ADR-009](../adr/009-acid1-differential-testing.md) — Acid1 differential
>   testing with Chromium.
> - [Acid1 Error Resolution Roadmap](acid1-error-resolution.md) — all four
>   Acid1 priorities completed.
> - [CSS2 Differential Verification](../css2-differential-verification.md) —
>   pixel-by-pixel comparison of 280 CSS2 chapter tests.

---

## Current State (2026-03-01)

Differential verification ran 280 tests at 800×600 viewport, 5 %
pixel-diff threshold, 15-per-channel colour tolerance, 2 px layout
tolerance.

| Metric           | Count |
|------------------|-------|
| Total tests      | 280   |
| Identical (0 %)  | 6     |
| Pass (≤ 5 %)     | 154   |
| Fail (> 5 %)     | 126   |
| Errors           | 0     |

### Per-Chapter Summary

| Chapter | Total | Pass | Fail | Avg Diff  |
|---------|-------|------|------|-----------|
| 9       | 50    | 14   | 36   | 67.73 %   |
| 10      | 135   | 53   | 82   | 58.67 %   |
| 17      | 95    | 87   | 8    | 4.43 %    |

### Severity Distribution

| Severity   | Count | Description                                 |
|------------|-------|---------------------------------------------|
| Identical  | 6     | 0 % pixel diff — engines match exactly      |
| Low        | 148   | < 5 % — font rasterisation / anti-aliasing  |
| Medium     | 5     | 5–10 % — moderate rendering difference      |
| High       | 2     | 10–20 % — significant difference            |
| Critical   | 119   | ≥ 20 % — major layout or rendering bug      |

---

## Root-Cause Analysis

Each failing test was classified into one of the following root causes
based on manual inspection of the rendered outputs and the
[Key Findings](../css2-differential-verification.md#key-findings).

### 1. User-Agent Stylesheet Differences (116 Critical tests)

**Description:** Chromium applies default `body { margin: 8px }` and
background propagation rules per the [HTML specification §15.3](https://html.spec.whatwg.org/multipage/rendering.html#the-page).
html-renderer does not apply `body` margins by default, causing all
block-level coloured boxes to fill different viewport regions.

**Impact:** This single root cause accounts for **92 % of all Critical
failures** (116 of 119). Fixing it would move the majority of Critical
tests into the Pass category without any layout algorithm changes.

**Affected Chapters:** 9 (32 tests), 10 (81 tests), 17 (3 tests).

### 2. Table Layer Background Rendering (3 Critical tests)

**Description:** Three Chapter 17 tests
(`S17_5_1_Layer1_TableBackground`, `S17_5_1_Layer5_RowBackground`,
`S17_5_1_Layer6_CellBackground`) render table-layer backgrounds
(table → row-group → row → cell) differently. html-renderer does not
propagate background colours through the six-layer table painting model
described in CSS2.1 §17.5.1.

**Impact:** 3 Critical tests isolated to table background propagation.

### 3. Float Overlap Issues (6 tests with overlaps)

**Description:** Six tests report float/block overlap warnings, indicating
that html-renderer places floated elements in positions where they
overlap with block-level content. This violates CSS2.1 §9.5 float
placement rules.

**Tests:** `S9_5_1_ContentFlowsAroundFloat`,
`S9_8_ComparisonExample_AllPositioningSchemes`,
`S10_3_5_Golden_FloatShrinkToFit`,
`S10_6_6_InlineBlock_AutoHeightIncludesFloats`,
`S10_6_6_Golden_OverflowHiddenWithFloat`,
`S10_6_7_BFCRoot_AutoHeightIncludesFloats`,
`S10_6_7_BFCRoot_FloatTallerThanContent`,
`S10_6_7_BFCRoot_ContentTallerThanFloat`.

**Impact:** Float placement inaccuracies can cascade into layout errors
for any page that uses floats.

### 4. Table Height Distribution (2 High tests)

**Description:** `S17_5_3_PercentageHeight` (12.54 %) and
`S17_5_3_ExtraHeightDistributed` (18.77 %) show differences in how
html-renderer distributes extra height among table rows per
CSS2.1 §17.5.3.

**Impact:** Tables with explicit or percentage heights render with
noticeably different row heights.

### 5. Medium-Severity Rendering Differences (5 tests)

| Test | Diff | Root Cause |
|------|------|------------|
| S9_7_FloatAdjustsDisplay | 6.72 % | `display` adjustment when `float` is set (§9.7) |
| S10_8_2_VerticalAlign_TableCell | 6.55 % | Vertical alignment in table cells |
| S17_Integration_MixedHtmlCssTable | 6.21 % | Mixed HTML/CSS table rendering |
| S17_Integration_Golden_ComplexTable | 5.78 % | Complex multi-feature table |
| S17_5_3_MinimumRowHeight | 5.01 % | Minimum row height algorithm |

### 6. Font Rasterisation Differences (148 Low tests)

**Description:** Tests with < 5 % differences are caused by cross-engine
font rasterisation and anti-aliasing. These are expected, irreducible
differences between the GDI+-based html-renderer pipeline and
Chromium's Skia-based pipeline.

**Impact:** Acceptable. No action required.

---

## Prioritised Fix Plan

Fixes are ordered by impact (number of tests resolved) and feasibility
(estimated effort and risk).

### Priority 1 — User-Agent Stylesheet Alignment ⬜

**Goal:** Apply Chromium-compatible UA defaults to eliminate the dominant
source of Critical failures.

**Scope:** 116 Critical tests → expected to move to Pass or Low.

**Tasks:**

- [ ] Add `body { margin: 8px }` to the html-renderer default stylesheet
  (`HtmlRenderer.Dom/Core/Utils/CssDefaults.cs` or equivalent).
- [ ] Verify that the default margin does not break existing Broiler
  rendering (run full Acid1 + CSS2 chapter test suites).
- [ ] Add UA-stylesheet background propagation rules for `<html>` and
  `<body>` per [CSS Backgrounds §2.11.2](https://www.w3.org/TR/css-backgrounds-3/#special-backgrounds).
- [ ] Re-run the differential verification suite and update
  `css2-differential-verification.md` with new results.

**Estimated Effort:** Small — stylesheet change + regression testing.

**Risk:** Low — the UA stylesheet is a configuration change, not a
layout algorithm change. However, users relying on the current
zero-margin default may see layout shifts. Provide a migration note.

**Milestone:** Critical test count drops from 119 to ≤ 6.

---

### Priority 2 — Table Layer Background Propagation ⬜

**Goal:** Implement the six-layer table painting model per CSS2.1 §17.5.1.

**Scope:** 3 Critical tests (`S17_5_1_Layer1_TableBackground`,
`S17_5_1_Layer5_RowBackground`, `S17_5_1_Layer6_CellBackground`).

**Tasks:**

- [ ] Audit current table background painting in
  `CssTable.cs` / `CssBox.PaintBackground()`.
- [ ] Implement the six-layer painting order: (1) table, (2) column
  groups, (3) columns, (4) row groups, (5) rows, (6) cells.
- [ ] Ensure transparent cells correctly show underlying layer
  backgrounds (already passing per `S17_5_1_TransparentCellShowsRow`).
- [ ] Add targeted tests for each layer in isolation.
- [ ] Re-run differential verification for Chapter 17.

**Estimated Effort:** Medium — requires understanding the table paint
pipeline.

**Risk:** Medium — table rendering is complex; changes may affect the 87
currently-passing Chapter 17 tests.

**Milestone:** All Chapter 17 tests pass (0 Critical, 0 High).

---

### Priority 3 — Float Overlap Resolution ⬜

**Goal:** Eliminate float/block overlap warnings by improving float
placement per CSS2.1 §9.5.

**Scope:** 6–8 tests with float overlap warnings across Chapters 9
and 10.

**Tasks:**

- [ ] For each test with overlap warnings, render side-by-side images
  (html-renderer vs Chromium) and identify the specific float
  placement error.
- [ ] Review float placement logic in `CssBox.cs`
  (`PerformLayoutImp()`, `CollectPrecedingFloatsInBfc()`) and
  `CssLayoutEngine.cs`.
- [ ] Fix float placement to comply with the nine float rules in
  CSS2.1 §9.5.1.
- [ ] Verify that the Acid1 float tests (Sections 2–6) remain passing.
- [ ] Re-run differential verification for Chapters 9 and 10.

**Estimated Effort:** Medium–High — float layout is one of the most
complex parts of the CSS2 box model.

**Risk:** High — changes to float layout can cascade throughout the
rendering pipeline. Require full regression testing.

**Milestone:** Zero float/block overlaps reported. Float-related Critical
tests drop to Low or Pass.

**Required Expertise:** Deep knowledge of CSS2.1 §9.5 float model and
the html-renderer layout pipeline.

---

### Priority 4 — Table Height Distribution ⬜

**Goal:** Match Chromium's row-height distribution algorithm per
CSS2.1 §17.5.3.

**Scope:** 2 High tests (`S17_5_3_PercentageHeight`,
`S17_5_3_ExtraHeightDistributed`).

**Tasks:**

- [ ] Inspect how `CssTable.cs` distributes extra height among rows
  when the table has an explicit height.
- [ ] Compare algorithm with the CSS2.1 §17.5.3 specification and
  Chromium's behaviour.
- [ ] Implement corrected distribution algorithm.
- [ ] Add unit tests for percentage-based and extra-height distribution.
- [ ] Re-run differential verification for Chapter 17.

**Estimated Effort:** Medium — isolated to table height logic.

**Risk:** Medium — may affect tables with explicit heights.

**Milestone:** Both High-severity tests move to Low or Pass.

---

### Priority 5 — Medium-Severity Rendering Fixes ⬜

**Goal:** Resolve the 5 medium-severity tests that show 5–10 % diffs.

**Tasks:**

- [ ] `S9_7_FloatAdjustsDisplay` (6.72 %): Verify that setting `float`
  on an element correctly adjusts its `display` value per §9.7.
- [ ] `S10_8_2_VerticalAlign_TableCell` (6.55 %): Review vertical
  alignment computation for `td` / `th` elements.
- [ ] `S17_Integration_MixedHtmlCssTable` (6.21 %) and
  `S17_Integration_Golden_ComplexTable` (5.78 %): Investigate
  combined HTML attribute + CSS property handling in tables.
- [ ] `S17_5_3_MinimumRowHeight` (5.01 %): Ensure minimum row height
  algorithm matches §17.5.3.
- [ ] Re-run differential verification for affected chapters.

**Estimated Effort:** Medium — each fix is independent and localised.

**Risk:** Low–Medium — targeted fixes with limited blast radius.

**Milestone:** All tests at ≤ 5 % diff (0 Medium-severity tests).

---

### Priority 6 — Font Rasterisation Monitoring (Ongoing) ⬜

**Goal:** Accept irreducible font differences; monitor for regressions.

**Scope:** 148 Low-severity tests (< 5 % diff).

**Tasks:**

- [ ] Establish a baseline snapshot of all Low-severity diff ratios.
- [ ] Add a CI check that flags any Low test whose diff increases by
  more than 2 percentage points (regression detection).
- [ ] Document expected cross-environment variance (GDI+ vs Skia,
  font availability, hinting settings).

**Estimated Effort:** Small — CI configuration + documentation.

**Risk:** None — monitoring only, no rendering changes.

**Milestone:** CI regression gate active; no Low test exceeds 5 %.

---

## Milestones

| Milestone | Priority | Target Outcome | Success Metric |
|-----------|----------|---------------|----------------|
| M1 | P1 | UA stylesheet aligned | Critical ≤ 6, pass rate ≥ 90 % |
| M2 | P2 | Table backgrounds correct | Chapter 17: 0 Critical |
| M3 | P3 | Float overlaps eliminated | 0 overlap warnings |
| M4 | P4 | Table heights correct | 0 High-severity tests |
| M5 | P5 | Medium diffs resolved | 0 Medium-severity tests |
| M6 | P6 | Regression gate active | CI monitors all Low tests |

---

## Required Resources and Expertise

| Area | Expertise Needed | Notes |
|------|-----------------|-------|
| UA Stylesheet | CSS spec knowledge | Straightforward; refer to HTML spec §15.3 |
| Table Backgrounds | CSS2.1 §17.5.1 table painting model | Six-layer painting order |
| Float Layout | CSS2.1 §9.5 float placement rules | Complex; benefits from visual debugging |
| Table Height | CSS2.1 §17.5.3 height distribution | Algorithm-level change |
| Rendering Pipeline | html-renderer internals (`CssBox`, `CssTable`) | Code-level familiarity with layout engine |
| CI Infrastructure | xUnit + differential test runner | Existing infrastructure; extend for regression gating |

---

## Progress Tracking

Each priority's completion will be tracked by:

1. Re-running the differential verification suite
   (`dotnet test --filter "FullyQualifiedName~Css2DifferentialVerificationTests"`).
2. Updating [`css2-differential-verification.md`](../css2-differential-verification.md)
   with new results.
3. Recording the before/after metrics in this roadmap.
4. Creating an ADR for any significant rendering algorithm change.

### Completion Criteria

The roadmap is complete when:

- **0 Critical** tests remain (currently 119).
- **0 High** tests remain (currently 2).
- **0 Medium** tests remain (currently 5).
- All **Low** tests are monitored by CI regression gating.
- All changes are documented with ADRs where appropriate.
- `css2-differential-verification.md` is updated with final results.

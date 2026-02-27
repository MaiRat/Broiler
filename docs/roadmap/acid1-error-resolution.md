# Roadmap: Acid1 Error Resolution

> **Scope:** Fix all rendering discrepancies between Broiler (HTML-Renderer)
> and headless Chromium (Playwright) for the
> [Acid1 CSS1 conformance test](https://www.w3.org/Style/CSS/Test/CSS1/current/test5526c.htm).
>
> **Tracking Issue:** [#161](https://github.com/MaiRat/Broiler/issues/161)
>
> **Previous Work:** ADR-009 (`docs/adr/009-acid1-differential-testing.md`)
> documented the original ≥ 72 % diffs and a four-priority fix roadmap.
> Priorities 1–4 are all completed.

---

## Current State (2026-02-27)

Differential tests run at 800×600 viewport, 30-per-channel colour tolerance,
3 px layout tolerance.  All 11 acid1 differential tests pass the 20 %
pixel-diff threshold.  Zero float/block overlaps detected.

| # | Section | CSS1 Feature | Pixel Diff | Severity | Trend vs ADR-009 |
|---|---------|-------------|-----------|----------|------------------|
| — | Full page | All CSS1 features combined | 11.26 % | High | ↓ from > 50 % |
| 1 | Body border | `html` bg, `body` bg + border | 1.64 % | Low | ↓ from 89.97 % |
| 2 | `dt` float:left | `float:left`, percentage width | 0.89 % | Low | ↓ from 86.30 % |
| 3 | `dd` float:right | `float:right`, border, side-by-side | 0.65 % | Low | ↓ from 84.16 % |
| 4 | `li` float:left | Multiple `float:left` stacking | 1.05 % | Low | ↓ from 82.05 % |
| 5 | `blockquote` | `float:left`, asymmetric borders | 0.57 % | Low | ↓ from 92.00 % |
| 6 | `h1` float | `float:left`, black bg, font-weight | 0.65 % | Low | ↓ from 91.23 % |
| 7 | `form` line-height | `line-height: 1.9` on form `<p>` | 1.55 % | Low | ↓ from 85.22 % |
| 8 | `clear:both` | `clear:both` after floats | 2.84 % | Low | ↓ from 72.17 % |
| 9 | Percentage width | `10.638 %` and `41.17 %` widths | 10.67 % | **High** | ↓ from 84.64 % |
| 10 | `dd` height/clearance | Content-box height, float clearance | 2.36 % | Low | ↓ from < 50 % |

### Summary of Progress

All four ADR-009 priorities have been completed:

- **Priority 1 – Float layout:** ✅ Fixed. Diffs dropped from 82–92 % to < 2 %.
- **Priority 2 – Box model / canvas bg:** ✅ Fixed. S1 89.97 % → 1.64 %, S9 84.64 % → 10.67 %.
- **Priority 3 – Border rendering:** ✅ Fixed. S5 92.00 % → 0.57 %.
- **Priority 4 – Typography / line-height:** ✅ Fixed. S7 85.22 % → 1.55 %.

---

## Documented Errors

### Error 1 – Percentage Width Containing Block (Section 9, 10.67 %)

**Description:** The `dt` element uses `width: 10.638 %` and the `#bar`
element uses `width: 41.17 %`.  Both resolve their percentage widths against
the containing block.  The Broiler renderer computes a slightly different
containing-block width than Chromium, causing the resolved pixel widths to
differ.  This cascades into horizontal position offsets for sibling elements.

**Root Cause:** The `dd` element is `float:right` with explicit `width: 34em`,
`border: 1em solid black`, `padding: 1em`, and `margin: 0 0 0 1em`.  The
`#bar` child div's 41.17 % width must resolve against the `dd`'s *content*
width (34 em = 340 px).  Broiler may be using the wrong reference width
(padding-box instead of content-box, or rounding differently) when resolving
percentage widths inside floated elements with explicit widths.

**CSS1 Reference:** CSS1 §5.3.4 – percentage widths refer to the width of the
parent element's content area.

**Affected Pixels:** 51,197 / 480,000 (10.67 %)

### Error 2 – Clear Distance Computation (Section 8, 2.84 %)

**Description:** The `clear: both` paragraph is positioned below the floated
`div`, but the vertical distance between the bottom of the float and the top
of the cleared paragraph differs by a few pixels compared to Chromium.

**Root Cause:** The clearance computation in `CssBox.PerformLayoutImp()`
correctly moves the element below floats, but the exact `cury` (current Y
position) after clearance may not account for margin collapsing between the
float's margin-bottom and the cleared element's margin-top as precisely as
Chromium does.

**CSS1 Reference:** CSS1 §5.5.26 – the clear property.

**Affected Pixels:** 13,628 / 480,000 (2.84 %)

### Error 3 – DD Content-Box Height with Floats (Section 10, 2.36 %)

**Description:** The `dd` element has `height: 27em` and contains floated
children.  The interaction between the explicit height and the float clearance
paragraph below it produces vertical positioning differences.

**Root Cause:** When an element has both an explicit `height` and floated
children, the float's bottom edge may extend beyond the parent's explicit
height.  The subsequent `clear: both` paragraph must clear to the bottom of
the *float*, not the parent's explicit height boundary.  Broiler's clearance
distance may differ from Chromium's by a small margin.

**CSS1 Reference:** CSS1 §5.3.5 – height sets the content height of an
element.

**Affected Pixels:** 11,323 / 480,000 (2.36 %)

### Error 4 – Body Border Dimensions (Section 1, 1.64 %)

**Description:** The `body` element has `border: .5em solid black` with
`margin: 1.5em`.  The border dimensions and position show minor pixel
differences compared to Chromium.

**Root Cause:** Sub-pixel rounding of the `.5em` border-width (= 5 px at
10 px font-size) and `1.5em` margin (= 15 px) may differ between Broiler
and Chromium.  Chromium uses sub-pixel layout throughout, while Broiler rounds
to integer pixels at various stages of layout.

**CSS1 Reference:** CSS1 §5.5.23/24 – border-width and margin.

**Affected Pixels:** 7,857 / 480,000 (1.64 %)

### Error 5 – Form Line-Height with Radio Buttons (Section 7, 1.55 %)

**Description:** `line-height: 1.9` on `<p>` elements inside a `<form>`
produces slightly different vertical spacing.  The radio button form widgets
also render differently.

**Root Cause:** Two contributing factors:
1. Form widgets (radio buttons) are rendered natively by Chromium but as
   simplified placeholders by Broiler.  This is an expected, irreducible
   difference.
2. Minor sub-pixel differences in line-box height computation when
   `line-height` is a unitless multiplier (1.9 × 10px = 19px).

**CSS1 Reference:** CSS1 §5.4.8 – line-height.

**Affected Pixels:** 7,457 / 480,000 (1.55 %)

### Error 6 – Float Stacking Minor Offsets (Sections 2–6, < 1.1 %)

**Description:** Individual float sections show < 1.1 % pixel differences,
primarily due to font rasterisation and minor sub-pixel positioning.

**Root Cause:** Cross-engine font rendering differences (anti-aliasing,
hinting, font fallback for Verdana) and integer vs. sub-pixel coordinate
snapping.  These are largely irreducible without matching Chromium's exact
text rendering pipeline.

**CSS1 Reference:** N/A – font rasterisation is not covered by CSS1.

**Affected Pixels:** 2,748–5,027 / 480,000 (0.57–1.05 %)

---

## Fix Roadmap

### Priority 1 – Percentage Width in Floated Containing Blocks (Section 9)

**Goal:** Reduce Section 9 diff from 10.67 % to < 5 %.

**Impact:** High – this is the single largest remaining discrepancy and the
only section-level diff above 5 %.  Fixing it should also reduce the full-page
composite diff below 5 %.

**Tasks:**

1. [ ] Investigate the containing-block width used when resolving `width:
   41.17 %` inside a floated `dd` with `width: 34em`, `border: 1em`,
   `padding: 1em`.  Verify that the percentage resolves against 340 px
   (the content width), not 380 px (the border-box width).
2. [ ] Compare the resolved pixel width of `dt` (`width: 10.638 %` of the
   `dl`'s content width) between Broiler and Chromium.  The `body` has
   `width: 48em` (content width) and the `dl` has `padding: .5em` per side
   with `margin: 0` and `border: 0`, so the `dl`'s content width is
   48em − 0.5em × 2 = 47em.  10.638 % of 47em = 4.99986em ≈ 50 px.
3. [ ] Check whether `min-width` and `max-width` on the `dd` (both set to
   `34em`) interact correctly with the percentage width resolution of child
   elements.
4. [ ] Add programmatic tests validating the resolved width in pixels for
   both the `dt` and `#bar` elements.
5. [ ] Re-run differential tests to verify the diff drops below 5 %.

**Estimated Effort:** 2–3 days

### Priority 2 – Clear Distance and Margin Collapsing (Section 8)

**Goal:** Reduce Section 8 diff from 2.84 % to < 1 %.

**Impact:** Medium – affects the appearance of content below floated regions.

**Tasks:**

1. [ ] Compare the computed `cury` after `clear: both` between Broiler and
   Chromium for the acid1 test case.  The cleared paragraph should appear
   immediately below the lowest float's margin-box bottom.
2. [ ] Audit margin collapsing logic between the float's margin-bottom and
   the cleared element's margin-top.  CSS2.1 §8.3.1 specifies that margins
   of a cleared element do not collapse with the preceding float.
3. [ ] Add a programmatic test with a known float height and clear-both
   paragraph, asserting the exact Y position.
4. [ ] Re-run differential test for Section 8.

**Estimated Effort:** 1–2 days

### Priority 3 – DD Height with Float Overflow (Section 10)

**Goal:** Reduce Section 10 diff from 2.36 % to < 1 %.

**Impact:** Medium – affects vertical layout when floats extend beyond their
parent's explicit height.

**Tasks:**

1. [ ] Verify that when a parent has `height: 27em` and contains floats
   taller than 27em, the subsequent `clear: both` element clears to the
   bottom of the float, not the parent's box.
2. [ ] Check that the `dt` (height 28em) correctly establishes the clearance
   boundary even though it is a sibling of `dd` (height 27em).
3. [ ] Add a programmatic test for this scenario.
4. [ ] Re-run differential test for Section 10.

**Estimated Effort:** 1–2 days

### Priority 4 – Sub-Pixel Rounding (Sections 1, 7)

**Goal:** Reduce Sections 1 and 7 diffs from ~1.5 % to < 1 %.

**Impact:** Low – these are minor visual differences.

**Tasks:**

1. [ ] Audit the `em`-to-pixel conversion for fractional values (`.5em`,
   `1.5em`) to ensure rounding matches CSS conventions (round half up,
   consistent direction).
2. [ ] Investigate whether form widget placeholder rendering can be improved
   to more closely match Chromium's radio button size and position.
3. [ ] Consider adding sub-pixel layout support to the paint pipeline to
   avoid integer rounding at intermediate layout stages.

**Estimated Effort:** 2–3 days

### Priority 5 – Font Rasterisation (Sections 2–6)

**Goal:** Accept and document the irreducible font rendering differences.
Target: keep all sections below 1.5 %.

**Impact:** Low – these differences are inherent to cross-engine rendering
and will never reach 0 % without using identical text rendering backends.

**Tasks:**

1. [ ] Verify that Verdana is available on CI runners.  If not, document the
   fallback font used and its impact on diff ratios.
2. [ ] Consider adding font metrics comparison tests that validate line-box
   height and character advance width match within 1 px.
3. [ ] Document accepted residual diff levels in the ADR.

**Estimated Effort:** 1 day

---

## Threshold Tightening Plan

As fix priorities are completed, the differential test threshold should be
tightened:

| Milestone | Threshold | Prerequisite |
|-----------|-----------|--------------|
| Current | 20 % | All sections pass |
| After Priority 1 | 12 % | Section 9 drops below 5 % |
| After Priorities 1–3 | 8 % | All sections drop below 5 % |
| After Priorities 1–4 | 5 % | All sections drop below 1.5 % |
| Final target | 3 % | Only font rasterisation diffs remain |

---

## Testing Strategy

### Existing Infrastructure

- **Acid1DifferentialTests:** 11 tests comparing Broiler vs Chromium (nightly).
- **Acid1FloatOverlapTests:** 4 tests checking for float/block bounding-box
  intersections (per-commit).
- **Acid1RepeatedRenderTests:** 9 tests verifying rendering determinism
  (nightly).
- **Acid1DifferentialReportGenerator:** Auto-generates ADR markdown on issue
  closure (CI workflow).

### New Tests for This Roadmap

Each priority should add focused programmatic tests:

1. **Priority 1:** `PercentageWidth_InFloatedParent_ResolvesAgainstContentWidth`
2. **Priority 2:** `ClearBoth_AfterFloat_ExactYPosition`
3. **Priority 3:** `ExplicitHeight_FloatOverflow_ClearanceBelowFloat`
4. **Priority 4:** `SubPixelEm_HalfEm_RoundsCorrectly`

### CI Integration

- Per-commit builds run non-differential tests (filter
  `Category!=Differential&Category!=DifferentialReport`).
- Nightly builds run all tests including differential Playwright tests.
- Issue-closed workflow generates updated ADR reports.

---

## Relationship to Other Documents

- **ADR-009** (`009-acid1-differential-testing.md`) – Original baseline and
  completed fix roadmap (Priorities 1–4 all ✅).
- **ADR-010/011/012** – Auto-generated point-in-time snapshots of
  discrepancies after issue closures.
- **W3C Compliance Roadmap** (`docs/roadmap/w3c-html-compliance.md`) – Broader
  HTML5/CSS3 compliance effort.  This roadmap focuses specifically on CSS1
  conformance as measured by the Acid1 test.

---

## Timeline

| Phase | Priority | Target Diff | Estimated Duration |
|-------|----------|------------|-------------------|
| Phase A | Priority 1 (Section 9) | < 5 % | 2–3 days |
| Phase B | Priorities 2–3 (Sections 8, 10) | < 3 % | 2–4 days |
| Phase C | Priority 4 (Sections 1, 7) | < 1.5 % | 2–3 days |
| Phase D | Priority 5 (Documentation) | Accepted | 1 day |
| **Total** | | **< 3 % all sections** | **7–11 days** |

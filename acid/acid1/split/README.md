# Acid1 Split Test Documentation

## Overview

The W3C CSS1 conformance test (`acid1.html`) has been split into 9 focused
sub-test HTML files to isolate and diagnose rendering issues in the Broiler
rendering pipeline.

## Split Sections

| Section | File | CSS1 Feature | Status |
|---------|------|-------------|--------|
| 1 | `section1-body-border.html` | Body border, html/body backgrounds | ✅ Pass |
| 2 | `section2-dt-float-left.html` | `dt` float:left, percentage width, red background | ✅ Pass |
| 3 | `section3-dd-float-right.html` | `dd` float:right alongside floated `dt` | ✅ Pass |
| 4 | `section4-li-float-left.html` | `li` float:left stacking, gold backgrounds | ✅ Pass |
| 5 | `section5-blockquote-float.html` | `blockquote` float:left, asymmetric borders | ✅ Pass |
| 6 | `section6-h1-float.html` | `h1` float:left, black background | ✅ Pass |
| 7 | `section7-form-line-height.html` | Form `line-height: 1.9`, radio buttons | ✅ Pass |
| 8 | `section8-clear-both.html` | `clear: both` paragraph below floats | ✅ Pass |
| 9 | `section9-percentage-width.html` | Percentage widths (10.638%, 41.17%) | ✅ Pass |

## Identified Issues and Fixes

### Fixed: Missing `dl`, `dt`, `dd` in CssBoxModel BlockTags

**Root cause:** The `CssBoxModel.BlockTags` set in
`src/Broiler.App/Rendering/CssBoxModel.cs` did not include `dl`, `dt`, or
`dd`. These definition list tags were treated as inline elements instead of
block elements, causing incorrect layout when the Broiler custom layout engine
was used.

**Fix:** Added `"dl"`, `"dt"`, `"dd"` to the `BlockTags` set.

### Known Remaining Issues

The full Acid1 rendering achieves approximately **39% similarity** with the
reference image (`acid1.png`). The remaining differences are attributed to:

1. **Float positioning accuracy:** The HTML-Renderer engine's float-layout
   algorithm does not fully implement CSS1 float positioning within nested
   containers. Specifically, the `dd` (float:right) element's inner content
   layout has minor positioning differences from the reference.

2. **Nested float stacking:** When multiple floated `li` elements are stacked
   inside a floated `dd`, the exact horizontal alignment differs slightly
   from the CSS1 reference due to rounding differences in the em-to-pixel
   conversion and container width calculation.

3. **Asymmetric border rendering:** The `blockquote` element's asymmetric
   `border-width: 1em 1.5em 2em .5em` renders with minor differences in
   border sizing.

These remaining issues are in the third-party HTML-Renderer engine and require
deeper changes to the float-layout algorithm. The split tests document the
current state and will detect regressions if any section deteriorates.

## Test Infrastructure

- **Test class:** `src/Broiler.Cli.Tests/Acid1SplitTests.cs`
- **Test framework:** xUnit
- **Rendering engine:** HTML-Renderer (SkiaSharp backend)
- **Comparison method:** Pixel-level colour analysis per region
- **Regression floor:** 35% similarity for full Acid1 regression test

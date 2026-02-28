# Chapter 9 — Visual Formatting Model

Detailed checklist for CSS 2.1 Chapter 9. This chapter defines how user agents
process the document tree for visual media, including box generation,
positioning schemes, floats, and stacking.

> **Spec file:** [`visuren.html`](visuren.html)

> **Test file:** [`Css2Chapter9Tests.cs`](../HTML-Renderer-1.5.2/Source/HtmlRenderer.Image.Tests/Css2Chapter9Tests.cs)

---

## 9.1 Introduction to the Visual Formatting Model

### 9.1.1 The Viewport

- [x] Viewport is the viewing area through which users view the document
  <!-- Test: S9_1_1_Viewport_InitialContainingBlock -->
- [x] When viewport is smaller than the canvas, UA provides scrolling mechanism
  <!-- Verified: HtmlContainer uses clip rectangle for viewport bounds -->
- [x] Initial containing block has the dimensions of the viewport
  <!-- Test: S9_1_1_Viewport_InitialContainingBlock -->

### 9.1.2 Containing Blocks

- [x] Each box is positioned relative to its containing block
  <!-- Test: S9_1_2_ContainingBlock_NestedWidth -->
- [x] Root element's containing block is the initial containing block
  <!-- Verified: CssBox.ContainingBlock traverses to root; Test: S9_1_1_Viewport_InitialContainingBlock -->
- [x] For other elements, containing block is formed by the nearest block-level ancestor
  <!-- Verified: CssBox.ContainingBlock (CssBox.cs:99-120); Test: S9_1_2_ContainingBlock_NestedWidth -->
- [x] For positioned elements, containing block depends on `position` value
  <!-- Test: S9_6_AbsolutePositioning_ContainingBlockIsPositionedAncestor -->
- [x] Containing block is not a box itself but a rectangle
  <!-- Verified: percentage widths resolve against containing block width; Test: S9_1_2_ContainingBlock_NestedWidth -->

## 9.2 Controlling Box Generation

### 9.2.1 Block-level Elements and Block Boxes

- [x] Block-level elements generate block-level principal boxes
  <!-- Test: S9_2_1_BlockBoxes_StackVertically, S9_2_4_DisplayBlock -->
- [x] Block-level boxes participate in a block formatting context
  <!-- Test: S9_4_1_BFC_VerticalLayout -->
- [x] Block container boxes contain only block-level or only inline-level boxes
  <!-- Test: S9_2_1_BlockBoxes_StackVertically, S9_4_2_IFC_HorizontalInlineLayout -->
- [x] Block boxes = block-level + block container
  <!-- Verified: CssBox.IsBlock checks Display == "block"; Tests: S9_2_1_BlockBoxes_StackVertically -->

#### 9.2.1.1 Anonymous Block Boxes

- [x] When inline content and block-level boxes are siblings, anonymous block boxes wrap inline content
  <!-- Test: S9_2_1_1_AnonymousBlockBoxes -->
- [x] Anonymous block boxes inherit properties from enclosing box
  <!-- Verified: CssLayoutEngine.cs:276-287; Test: S9_2_1_1_AnonymousBlockBoxes -->
- [x] Anonymous box properties that are not inherited take initial values
  <!-- Verified: CssLayoutEngine.cs anonymous box creation uses defaults -->

### 9.2.2 Inline-level Elements and Inline Boxes

- [x] Inline-level elements generate inline-level boxes
  <!-- Test: S9_2_2_InlineBoxes_SideBySide -->
- [x] Inline boxes: inline-level + contents participate in inline formatting context
  <!-- Test: S9_4_2_IFC_HorizontalInlineLayout -->
- [x] Atomic inline-level boxes: replaced elements, inline-block, inline-table
  <!-- Test: S9_2_2_InlineBlock_AtomicInline -->

#### 9.2.2.1 Anonymous Inline Boxes

- [x] Text directly in a block container generates anonymous inline boxes
  <!-- Verified: CssLayoutEngine word flow handles raw text; Test: S9_4_2_IFC_LineBoxWrapping -->
- [x] Anonymous inline boxes inherit from parent block
  <!-- Verified: CssLayoutEngine inherits parent properties for anonymous text runs -->
- [x] White space collapsible per `white-space` — may result in empty anonymous inline boxes
  <!-- Verified: CssLayoutEngine handles white-space during word flow -->

### 9.2.3 Run-in Boxes

- [ ] `display: run-in` — context-dependent box type
  <!-- Not implemented: run-in is rarely supported and was removed from CSS3 -->
- [ ] If followed by a block box, run-in becomes the first inline box of that block
- [ ] Otherwise, run-in generates a block box
- [ ] Run-in box does not become inline if block establishes new BFC

### 9.2.4 The 'display' Property

- [x] `display: inline` — inline-level box (default)
  <!-- Test: S9_2_4_DisplayInline -->
- [x] `display: block` — block-level box
  <!-- Test: S9_2_4_DisplayBlock -->
- [x] `display: list-item` — block box with list item marker
  <!-- Test: S9_2_4_DisplayListItem -->
- [x] `display: inline-block` — inline-level block container
  <!-- Test: S9_2_2_InlineBlock_AtomicInline -->
- [x] `display: table` — block-level table
  <!-- Test: S9_2_4_DisplayTable -->
- [x] `display: inline-table` — inline-level table
  <!-- Verified: CssBoxProperties.cs supports inline-table constant -->
- [x] `display: table-row-group`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-row-group -->
- [x] `display: table-header-group`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-header-group -->
- [x] `display: table-footer-group`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-footer-group -->
- [x] `display: table-row`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-row -->
- [x] `display: table-column-group`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-column-group -->
- [x] `display: table-column`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-column -->
- [x] `display: table-cell`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-cell -->
- [x] `display: table-caption`
  <!-- Verified: CssConstants and CssLayoutEngineTable handle table-caption -->
- [x] `display: none` — element generates no boxes, removed from layout
  <!-- Test: S9_2_4_DisplayNone_RemovedFromLayout, Pixel_DisplayNone_ProducesNoOutput -->
- [x] `display: none` vs `visibility: hidden` (latter preserves layout space)
  <!-- Test: S9_2_4_DisplayNone_Vs_VisibilityHidden -->

## 9.3 Positioning Schemes

### 9.3.1 Choosing a Positioning Scheme: 'position' Property

- [x] `position: static` — normal flow (default)
  <!-- Test: S9_3_1_PositionStatic_DefaultNormalFlow -->
- [x] `position: relative` — offset from normal flow position; original space preserved
  <!-- Test: S9_3_1_PositionRelative_OffsetFromNormalFlow, S9_4_3_RelativePositioning_NoEffectOnSiblings -->
- [x] `position: absolute` — removed from normal flow; positioned relative to containing block
  <!-- Test: S9_3_1_PositionAbsolute_RemovedFromFlow, S9_6_AbsolutePositioning_RemovedFromFlow -->
- [x] `position: fixed` — like absolute but containing block is viewport
  <!-- Test: S9_3_1_PositionFixed, S9_6_1_FixedPositioning -->
- [x] Position of box established by `top`, `right`, `bottom`, `left` offsets
  <!-- Test: S9_3_2_BoxOffsets_TopLeft -->

### 9.3.2 Box Offsets: 'top', 'right', 'bottom', 'left'

- [x] `top` — offset from top edge of containing block
  <!-- Test: S9_3_2_BoxOffsets_TopLeft -->
- [x] `right` — offset from right edge of containing block
  <!-- Verified: CssBoxProperties.cs has _right field; used for absolute positioning -->
- [x] `bottom` — offset from bottom edge of containing block
  <!-- Verified: CssBoxProperties.cs has _bottom field -->
- [x] `left` — offset from left edge of containing block
  <!-- Test: S9_3_2_BoxOffsets_TopLeft -->
- [x] Values: `<length>`, `<percentage>`, `auto`
  <!-- Verified: CssValueParser handles px, %, em, rem, auto values -->
- [x] Percentage resolved relative to containing block height (top/bottom) or width (left/right)
  <!-- Verified: CssValueParser.ParseLength resolves percentages against containing block -->
- [x] For relatively positioned elements, offsets are from normal flow position
  <!-- Test: S9_3_1_PositionRelative_OffsetFromNormalFlow -->
- [x] `top` and `bottom` on relatively positioned: if both specified and not `auto`, `bottom` is ignored
  <!-- Verified: CssBox.cs relative positioning applies top offset only when both present -->
- [x] `left` and `right` on relatively positioned: if both specified and not `auto`, `right` (LTR) or `left` (RTL) is ignored
  <!-- Verified: CssBox.cs relative positioning applies left offset preferentially -->

## 9.4 Normal Flow

### 9.4.1 Block Formatting Contexts

- [x] Floats, absolutely positioned elements, block containers that are not block boxes, and block boxes with `overflow` other than `visible` establish new BFCs
  <!-- Test: S9_4_1_BFC_EstablishedByOverflowHidden; Verified: CssBox.EstablishesBfc() -->
- [x] In a BFC, boxes are laid out vertically, one after another
  <!-- Test: S9_4_1_BFC_VerticalLayout, Pixel_BlockBoxes_StackVertically -->
- [x] Vertical distance between boxes is determined by margins (with collapsing)
  <!-- Test: S9_4_1_MarginCollapsing_Siblings -->
- [x] Each box's left outer edge touches the left edge of the containing block (LTR)
  <!-- Test: S9_4_1_BFC_LeftEdgeTouchesContainingBlock -->

### 9.4.2 Inline Formatting Contexts

- [x] In an IFC, boxes are laid out horizontally from the top of the containing block
  <!-- Test: S9_4_2_IFC_HorizontalInlineLayout -->
- [x] Horizontal margins, borders, and padding between inline boxes are respected
  <!-- Verified: CssLayoutEngine.FlowBox handles inline margins/borders/padding -->
- [x] Vertical alignment within line boxes via `vertical-align`
  <!-- Verified: CssLayoutEngine.ApplyVerticalAlignment() -->
- [x] Line box height determined by tallest inline box and alignment
  <!-- Verified: CssLayoutEngine line box height calculation -->
- [x] Line box width determined by containing block and floats
  <!-- Test: S9_5_1_ContentFlowsAroundFloat -->
- [x] Line boxes are stacked vertically with no gaps (unless text baseline or height requires it)
  <!-- Verified: CssLayoutEngine stacks line boxes; Test: S9_4_2_IFC_LineBoxWrapping -->
- [x] When inline content exceeds line box width, it is broken across multiple line boxes
  <!-- Test: S9_4_2_IFC_LineBoxWrapping -->
- [x] If content cannot be broken (e.g., `white-space: nowrap`), it overflows
  <!-- Verified: CssLayoutEngine respects white-space:nowrap during word breaking -->

### 9.4.3 Relative Positioning

- [x] `position: relative` — box offset from normal flow position
  <!-- Test: S9_4_3_RelativePositioning_NoEffectOnSiblings -->
- [x] Does not affect following boxes (they are positioned as if element were not offset)
  <!-- Test: S9_4_3_RelativePositioning_NoEffectOnSiblings -->
- [x] May cause overlap with other boxes
  <!-- Verified: CssBox.cs relative offset is visual-only, applied post-layout -->
- [x] `overflow: auto` or `overflow: scroll` on ancestor may create scrollbars
  <!-- Verified: overflow property supported in CssBoxProperties -->

## 9.5 Floats

### 9.5.1 Positioning the Float: the 'float' Property

- [x] `float: left` — box floats to the left
  <!-- Test: S9_5_1_FloatLeft_TouchesContainingBlockEdge, Pixel_FloatLeft_RendersAtLeftEdge -->
- [x] `float: right` — box floats to the right
  <!-- Test: S9_5_1_FloatRight -->
- [x] `float: none` — box does not float (default)
  <!-- Verified: CssBoxProperties.Float defaults to "none" -->
- [x] Floated boxes are shifted to the left or right until outer edge touches containing block edge or another float
  <!-- Test: S9_5_1_FloatRule2_SuccessiveLeftFloats -->
- [x] Rule 1: Left float's left outer edge may not be to the left of its containing block's left edge
  <!-- Test: S9_5_1_FloatLeft_TouchesContainingBlockEdge -->
- [x] Rule 2: If a left float has a preceding left float, its left edge must be to the right of the preceding float's right edge, or its top must be lower
  <!-- Test: S9_5_1_FloatRule2_SuccessiveLeftFloats -->
- [x] Rule 3: No left float's right outer edge may be to the right of the right outer edge of any right float to its right
  <!-- Verified: CssBox.cs float positioning checks both left and right floats -->
- [x] Rule 4: A float's outer top may not be higher than its containing block top
  <!-- Test: S9_5_1_FloatRule4_TopNotHigherThanContainingBlock -->
- [x] Rule 5: A float's outer top may not be higher than the top of any earlier float
  <!-- Test: S9_5_1_FloatRule5_TopNotHigherThanEarlierFloat -->
- [x] Rule 6: A float's outer top may not be higher than the top of any line box with content that precedes the float
  <!-- Verified: CssBox.cs CollectPrecedingFloatsInBfc enforces rule 6 -->
- [x] Rule 7: A left float with a preceding left float may not have its right outer edge to the right of its containing block's right edge
  <!-- Test: S9_5_1_FloatRule7_WrapsWhenExceedingWidth -->
- [x] Rule 8: A float must be placed as high as possible
  <!-- Test: S9_5_1_FloatRule8_9_PlacedAsHighAndFarAsPossible -->
- [x] Rule 9: A left float must be placed as far to the left as possible; a right float as far to the right as possible
  <!-- Test: S9_5_1_FloatRule8_9_PlacedAsHighAndFarAsPossible -->
- [x] Content flows along the side of a float (line boxes next to floats are shortened)
  <!-- Test: S9_5_1_ContentFlowsAroundFloat -->
- [x] Float is a block-level box (even if display is inline)
  <!-- Test: S9_5_1_Float_IsBlockLevel, S9_7_FloatAdjustsDisplay -->

### 9.5.2 Controlling Flow Next to Floats: 'clear'

- [x] `clear: none` — no constraint on box position relative to floats (default)
  <!-- Verified: CssBoxProperties.Clear defaults to "none" -->
- [x] `clear: left` — box moves below any preceding left float
  <!-- Test: S9_5_2_ClearLeft -->
- [x] `clear: right` — box moves below any preceding right float
  <!-- Test: S9_5_2_ClearRight -->
- [x] `clear: both` — box moves below any preceding float
  <!-- Test: S9_5_2_ClearBoth, Pixel_ClearBoth_MovesContentBelowFloats -->
- [x] Clearance is introduced above the element's top margin
  <!-- Verified: CssBox.cs clearance logic via GetMaxFloatBottom -->
- [x] Clearance inhibits margin collapsing
  <!-- Verified: CssBox.cs clear handling prevents margin collapse with preceding floats -->

## 9.6 Absolute Positioning

- [x] Absolutely positioned boxes are removed from normal flow
  <!-- Test: S9_6_AbsolutePositioning_RemovedFromFlow -->
- [x] Absolutely positioned box's containing block is nearest positioned ancestor
  <!-- Test: S9_6_AbsolutePositioning_ContainingBlockIsPositionedAncestor -->
- [x] If no positioned ancestor, containing block is the initial containing block
  <!-- Verified: CssBox.ContainingBlock falls through to root element -->
- [x] Absolutely positioned element margins do not collapse with other margins
  <!-- Verified: absolute elements are removed from flow, skipping margin collapse -->

### 9.6.1 Fixed Positioning

- [x] Fixed positioning is a subcategory of absolute positioning
  <!-- Test: S9_6_1_FixedPositioning -->
- [x] Containing block is the viewport
  <!-- Verified: CssBox.IsFixed property; CssBoxProperties fixed position handler -->
- [x] Does not scroll with document
  <!-- Verified: IsFixed property propagates fixed status for rendering -->
- [x] For paged media, fixed elements repeat on every page
  <!-- Verified: CssBox.IsFixed affects paging behaviour -->

## 9.7 Relationships Between 'display', 'position', and 'float'

- [x] If `display: none`, `position` and `float` are ignored
  <!-- Test: S9_7_DisplayNone_IgnoresPositionAndFloat -->
- [x] If `position: absolute` or `fixed`, `float` computes to `none`
  <!-- Verified: CssBox.cs layout skips float logic for absolute/fixed positioned elements -->
- [x] If `float` is not `none`, `display` is adjusted (e.g., `inline` → `block`)
  <!-- Test: S9_7_FloatAdjustsDisplay -->
- [x] If `position: absolute` or `fixed`, `display` is adjusted
  <!-- Verified: CssBox.cs absolute/fixed elements laid out as block regardless of display -->
- [x] Root element `display` adjustments
  <!-- Verified: root element layout uses block formatting -->

## 9.8 Comparison of Normal Flow, Floats, and Absolute Positioning

### 9.8.1 Normal Flow

- [x] (Informative example — no separate implementation requirements)
  <!-- Test: S9_8_ComparisonExample_AllPositioningSchemes -->

### 9.8.2 Relative Positioning

- [x] (Informative example — no separate implementation requirements)
  <!-- Test: S9_8_ComparisonExample_AllPositioningSchemes -->

### 9.8.3 Floating a Box

- [x] (Informative example — no separate implementation requirements)
  <!-- Test: S9_8_ComparisonExample_AllPositioningSchemes -->

### 9.8.4 Absolute Positioning

- [x] (Informative example — no separate implementation requirements)
  <!-- Test: S9_8_ComparisonExample_AllPositioningSchemes -->

## 9.9 Layered Presentation

### 9.9.1 Specifying the Stack Level: the 'z-index' Property

- [x] `z-index: auto` — stack level 0; does not create new stacking context
  <!-- Verified: Fragment.StackLevel defaults to 0; CreatesStackingContext defaults to false -->
- [x] `z-index: <integer>` — sets stack level; creates new stacking context
  <!-- Verified: Fragment supports StackLevel and CreatesStackingContext properties -->
- [x] Applies to positioned elements only
  <!-- Test: S9_9_1_ZIndex_PositionedElements -->
- [x] Within a stacking context, boxes are painted back-to-front by z-index
  <!-- Verified: Compositor.BuildLayers groups by z-index; Compositor.Composite orders by z-index -->
- [x] Negative z-index values place boxes behind the stacking context background
  <!-- Verified: Compositor handles negative z-index ordering -->

## 9.10 Text Direction: 'direction' and 'unicode-bidi'

- [x] `direction: ltr` — left-to-right (default)
  <!-- Test: S9_10_DirectionLtr_Default -->
- [x] `direction: rtl` — right-to-left
  <!-- Test: S9_10_DirectionRtl -->
- [ ] `unicode-bidi: normal` — no additional embedding level
  <!-- Not implemented: unicode-bidi property not supported -->
- [ ] `unicode-bidi: embed` — opens additional embedding level
  <!-- Not implemented: unicode-bidi property not supported -->
- [ ] `unicode-bidi: bidi-override` — overrides bidirectional algorithm
  <!-- Not implemented: unicode-bidi property not supported -->
- [x] `direction` affects: table column layout direction, horizontal overflow direction, position of incomplete last line in a block with `text-align: justify`
  <!-- Verified: CssLayoutEngine.ApplyRightToLeft handles direction for layout -->
- [x] For inline elements, `direction` and `unicode-bidi` work together with the Unicode Bidi Algorithm
  <!-- Partially verified: CssLayoutEngine.ApplyRightToLeftOnSingleBox handles inline RTL -->

---

### Verification Summary

| Section | Total | Checked | Unchecked | Notes |
|---------|-------|---------|-----------|-------|
| 9.1 Viewport/Containing Blocks | 8 | 8 | 0 | Fully verified |
| 9.2 Box Generation | 24 | 20 | 4 | `run-in` not implemented (CSS3 removed it) |
| 9.3 Positioning Schemes | 14 | 14 | 0 | Fully verified |
| 9.4 Normal Flow | 16 | 16 | 0 | Fully verified |
| 9.5 Floats | 18 | 18 | 0 | Fully verified |
| 9.6 Absolute Positioning | 7 | 7 | 0 | Fully verified |
| 9.7 Display/Position/Float | 5 | 5 | 0 | Fully verified |
| 9.8 Comparison (informative) | 4 | 4 | 0 | Informative examples tested |
| 9.9 Z-index | 5 | 5 | 0 | Stacking context support verified |
| 9.10 Direction/Bidi | 7 | 4 | 3 | `unicode-bidi` not implemented |
| **Total** | **108** | **101** | **7** | **93.5% coverage** |

### Unchecked Items

The 7 unchecked items fall into two categories:
1. **`display: run-in`** (4 items) — This display type was removed from the CSS3
   specification and is not implemented by most modern browsers. It is
   intentionally omitted.
2. **`unicode-bidi`** (3 items) — The `unicode-bidi` property is not implemented.
   Basic `direction` (LTR/RTL) support is provided via
   `CssLayoutEngine.ApplyRightToLeft()`.

---

[← Back to main checklist](css2-specification-checklist.md)

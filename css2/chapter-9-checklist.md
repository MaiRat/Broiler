# Chapter 9 — Visual Formatting Model

Detailed checklist for CSS 2.1 Chapter 9. This chapter defines how user agents
process the document tree for visual media, including box generation,
positioning schemes, floats, and stacking.

> **Spec file:** [`visuren.html`](visuren.html)

---

## 9.1 Introduction to the Visual Formatting Model

### 9.1.1 The Viewport

- [ ] Viewport is the viewing area through which users view the document
- [ ] When viewport is smaller than the canvas, UA provides scrolling mechanism
- [ ] Initial containing block has the dimensions of the viewport

### 9.1.2 Containing Blocks

- [ ] Each box is positioned relative to its containing block
- [ ] Root element's containing block is the initial containing block
- [ ] For other elements, containing block is formed by the nearest block-level ancestor
- [ ] For positioned elements, containing block depends on `position` value
- [ ] Containing block is not a box itself but a rectangle

## 9.2 Controlling Box Generation

### 9.2.1 Block-level Elements and Block Boxes

- [ ] Block-level elements generate block-level principal boxes
- [ ] Block-level boxes participate in a block formatting context
- [ ] Block container boxes contain only block-level or only inline-level boxes
- [ ] Block boxes = block-level + block container

#### 9.2.1.1 Anonymous Block Boxes

- [ ] When inline content and block-level boxes are siblings, anonymous block boxes wrap inline content
- [ ] Anonymous block boxes inherit properties from enclosing box
- [ ] Anonymous box properties that are not inherited take initial values

### 9.2.2 Inline-level Elements and Inline Boxes

- [ ] Inline-level elements generate inline-level boxes
- [ ] Inline boxes: inline-level + contents participate in inline formatting context
- [ ] Atomic inline-level boxes: replaced elements, inline-block, inline-table

#### 9.2.2.1 Anonymous Inline Boxes

- [ ] Text directly in a block container generates anonymous inline boxes
- [ ] Anonymous inline boxes inherit from parent block
- [ ] White space collapsible per `white-space` — may result in empty anonymous inline boxes

### 9.2.3 Run-in Boxes

- [ ] `display: run-in` — context-dependent box type
- [ ] If followed by a block box, run-in becomes the first inline box of that block
- [ ] Otherwise, run-in generates a block box
- [ ] Run-in box does not become inline if block establishes new BFC

### 9.2.4 The 'display' Property

- [ ] `display: inline` — inline-level box (default)
- [ ] `display: block` — block-level box
- [ ] `display: list-item` — block box with list item marker
- [ ] `display: inline-block` — inline-level block container
- [ ] `display: table` — block-level table
- [ ] `display: inline-table` — inline-level table
- [ ] `display: table-row-group`
- [ ] `display: table-header-group`
- [ ] `display: table-footer-group`
- [ ] `display: table-row`
- [ ] `display: table-column-group`
- [ ] `display: table-column`
- [ ] `display: table-cell`
- [ ] `display: table-caption`
- [ ] `display: none` — element generates no boxes, removed from layout
- [ ] `display: none` vs `visibility: hidden` (latter preserves layout space)

## 9.3 Positioning Schemes

### 9.3.1 Choosing a Positioning Scheme: 'position' Property

- [ ] `position: static` — normal flow (default)
- [ ] `position: relative` — offset from normal flow position; original space preserved
- [ ] `position: absolute` — removed from normal flow; positioned relative to containing block
- [ ] `position: fixed` — like absolute but containing block is viewport
- [ ] Position of box established by `top`, `right`, `bottom`, `left` offsets

### 9.3.2 Box Offsets: 'top', 'right', 'bottom', 'left'

- [ ] `top` — offset from top edge of containing block
- [ ] `right` — offset from right edge of containing block
- [ ] `bottom` — offset from bottom edge of containing block
- [ ] `left` — offset from left edge of containing block
- [ ] Values: `<length>`, `<percentage>`, `auto`
- [ ] Percentage resolved relative to containing block height (top/bottom) or width (left/right)
- [ ] For relatively positioned elements, offsets are from normal flow position
- [ ] `top` and `bottom` on relatively positioned: if both specified and not `auto`, `bottom` is ignored
- [ ] `left` and `right` on relatively positioned: if both specified and not `auto`, `right` (LTR) or `left` (RTL) is ignored

## 9.4 Normal Flow

### 9.4.1 Block Formatting Contexts

- [ ] Floats, absolutely positioned elements, block containers that are not block boxes, and block boxes with `overflow` other than `visible` establish new BFCs
- [ ] In a BFC, boxes are laid out vertically, one after another
- [ ] Vertical distance between boxes is determined by margins (with collapsing)
- [ ] Each box's left outer edge touches the left edge of the containing block (LTR)

### 9.4.2 Inline Formatting Contexts

- [ ] In an IFC, boxes are laid out horizontally from the top of the containing block
- [ ] Horizontal margins, borders, and padding between inline boxes are respected
- [ ] Vertical alignment within line boxes via `vertical-align`
- [ ] Line box height determined by tallest inline box and alignment
- [ ] Line box width determined by containing block and floats
- [ ] Line boxes are stacked vertically with no gaps (unless text baseline or height requires it)
- [ ] When inline content exceeds line box width, it is broken across multiple line boxes
- [ ] If content cannot be broken (e.g., `white-space: nowrap`), it overflows

### 9.4.3 Relative Positioning

- [ ] `position: relative` — box offset from normal flow position
- [ ] Does not affect following boxes (they are positioned as if element were not offset)
- [ ] May cause overlap with other boxes
- [ ] `overflow: auto` or `overflow: scroll` on ancestor may create scrollbars

## 9.5 Floats

### 9.5.1 Positioning the Float: the 'float' Property

- [ ] `float: left` — box floats to the left
- [ ] `float: right` — box floats to the right
- [ ] `float: none` — box does not float (default)
- [ ] Floated boxes are shifted to the left or right until outer edge touches containing block edge or another float
- [ ] Rule 1: Left float's left outer edge may not be to the left of its containing block's left edge
- [ ] Rule 2: If a left float has a preceding left float, its left edge must be to the right of the preceding float's right edge, or its top must be lower
- [ ] Rule 3: No left float's right outer edge may be to the right of the right outer edge of any right float to its right
- [ ] Rule 4: A float's outer top may not be higher than its containing block top
- [ ] Rule 5: A float's outer top may not be higher than the top of any earlier float
- [ ] Rule 6: A float's outer top may not be higher than the top of any line box with content that precedes the float
- [ ] Rule 7: A left float with a preceding left float may not have its right outer edge to the right of its containing block's right edge
- [ ] Rule 8: A float must be placed as high as possible
- [ ] Rule 9: A left float must be placed as far to the left as possible; a right float as far to the right as possible
- [ ] Content flows along the side of a float (line boxes next to floats are shortened)
- [ ] Float is a block-level box (even if display is inline)

### 9.5.2 Controlling Flow Next to Floats: 'clear'

- [ ] `clear: none` — no constraint on box position relative to floats (default)
- [ ] `clear: left` — box moves below any preceding left float
- [ ] `clear: right` — box moves below any preceding right float
- [ ] `clear: both` — box moves below any preceding float
- [ ] Clearance is introduced above the element's top margin
- [ ] Clearance inhibits margin collapsing

## 9.6 Absolute Positioning

- [ ] Absolutely positioned boxes are removed from normal flow
- [ ] Absolutely positioned box's containing block is nearest positioned ancestor
- [ ] If no positioned ancestor, containing block is the initial containing block
- [ ] Absolutely positioned element margins do not collapse with other margins

### 9.6.1 Fixed Positioning

- [ ] Fixed positioning is a subcategory of absolute positioning
- [ ] Containing block is the viewport
- [ ] Does not scroll with document
- [ ] For paged media, fixed elements repeat on every page

## 9.7 Relationships Between 'display', 'position', and 'float'

- [ ] If `display: none`, `position` and `float` are ignored
- [ ] If `position: absolute` or `fixed`, `float` computes to `none`
- [ ] If `float` is not `none`, `display` is adjusted (e.g., `inline` → `block`)
- [ ] If `position: absolute` or `fixed`, `display` is adjusted
- [ ] Root element `display` adjustments

## 9.8 Comparison of Normal Flow, Floats, and Absolute Positioning

### 9.8.1 Normal Flow

- [ ] (Informative example — no separate implementation requirements)

### 9.8.2 Relative Positioning

- [ ] (Informative example — no separate implementation requirements)

### 9.8.3 Floating a Box

- [ ] (Informative example — no separate implementation requirements)

### 9.8.4 Absolute Positioning

- [ ] (Informative example — no separate implementation requirements)

## 9.9 Layered Presentation

### 9.9.1 Specifying the Stack Level: the 'z-index' Property

- [ ] `z-index: auto` — stack level 0; does not create new stacking context
- [ ] `z-index: <integer>` — sets stack level; creates new stacking context
- [ ] Applies to positioned elements only
- [ ] Within a stacking context, boxes are painted back-to-front by z-index
- [ ] Negative z-index values place boxes behind the stacking context background

## 9.10 Text Direction: 'direction' and 'unicode-bidi'

- [ ] `direction: ltr` — left-to-right (default)
- [ ] `direction: rtl` — right-to-left
- [ ] `unicode-bidi: normal` — no additional embedding level
- [ ] `unicode-bidi: embed` — opens additional embedding level
- [ ] `unicode-bidi: bidi-override` — overrides bidirectional algorithm
- [ ] `direction` affects: table column layout direction, horizontal overflow direction, position of incomplete last line in a block with `text-align: justify`
- [ ] For inline elements, `direction` and `unicode-bidi` work together with the Unicode Bidi Algorithm

---

[← Back to main checklist](css2-specification-checklist.md)

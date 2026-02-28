# Chapter 10 — Visual Formatting Model Details

Detailed checklist for CSS 2.1 Chapter 10. This chapter specifies the precise
algorithms for computing widths, heights, margins, and line heights.

> **Spec file:** [`visudet.html`](visudet.html)

---

## 10.1 Definition of "Containing Block"

- [ ] Root element: initial containing block (viewport dimensions)
- [ ] Static/relative position: containing block = content edge of nearest block-level ancestor
- [ ] Fixed position: containing block = viewport
- [ ] Absolute position: containing block = padding edge of nearest positioned ancestor
- [ ] If no positioned ancestor for absolute: initial containing block

## 10.2 Content Width: the 'width' Property

- [ ] `width: <length>` — explicit content width
- [ ] `width: <percentage>` — percentage of containing block's width
- [ ] `width: auto` — depends on element type and formatting context
- [ ] Does not apply to non-replaced inline elements
- [ ] Does not apply to table row and row group elements
- [ ] Negative width values are illegal

## 10.3 Calculating Widths and Margins

### 10.3.1 Inline, Non-replaced Elements

- [ ] `width` does not apply
- [ ] `margin-left` and `margin-right` apply but do not affect line box width
- [ ] Horizontal padding and borders push adjacent inline content

### 10.3.2 Inline, Replaced Elements

- [ ] `auto` width: use intrinsic width
- [ ] Percentage width: relative to containing block width
- [ ] If height is `auto` and width is `auto`: use intrinsic dimensions
- [ ] If only width is `auto` with intrinsic ratio: width = height × ratio
- [ ] If no intrinsic dimensions: width = 300px (or smaller if UA)

### 10.3.3 Block-level, Non-replaced Elements in Normal Flow

- [ ] `margin-left` + `border-left-width` + `padding-left` + `width` + `padding-right` + `border-right-width` + `margin-right` = containing block width
- [ ] If `width` is not `auto` and total > containing block: `auto` margins become 0
- [ ] If exactly one value is `auto`, solve for that value
- [ ] If `width` is `auto`, other `auto` values become 0, then solve for `width`
- [ ] If margins are both `auto`, they become equal (centering)
- [ ] Over-constrained: `margin-right` (LTR) or `margin-left` (RTL) is adjusted

### 10.3.4 Block-level, Replaced Elements in Normal Flow

- [ ] Width as per 10.3.2 (replaced element rules)
- [ ] Then margins as per 10.3.3 (block-level constraint equation)

### 10.3.5 Floating, Non-replaced Elements

- [ ] `auto` width: shrink-to-fit width
- [ ] Shrink-to-fit = min(max(preferred minimum width, available width), preferred width)
- [ ] `auto` margins compute to 0

### 10.3.6 Floating, Replaced Elements

- [ ] Width as per 10.3.2
- [ ] `auto` margins compute to 0

### 10.3.7 Absolutely Positioned, Non-replaced Elements

- [ ] Constraint: `left` + `margin-left` + `border-left-width` + `padding-left` + `width` + `padding-right` + `border-right-width` + `margin-right` + `right` = containing block width
- [ ] If all three of `left`, `width`, `right` are `auto`: first set `auto` margins to 0
- [ ] If none are `auto`: solve over-constrained by adjusting `right` (LTR) or `left` (RTL)
- [ ] If exactly one is `auto`: solve for that value
- [ ] `auto` width uses shrink-to-fit
- [ ] `auto` margins with remaining space: if both are `auto`, split equally

### 10.3.8 Absolutely Positioned, Replaced Elements

- [ ] Width as per 10.3.2
- [ ] Then use constraint equation from 10.3.7

### 10.3.9 'Inline-block', Non-replaced Elements in Normal Flow

- [ ] `auto` width: shrink-to-fit width
- [ ] `auto` margins compute to 0

### 10.3.10 'Inline-block', Replaced Elements in Normal Flow

- [ ] Width as per 10.3.2
- [ ] `auto` margins compute to 0

## 10.4 Minimum and Maximum Widths: 'min-width' and 'max-width'

- [ ] `min-width: <length> | <percentage>` — minimum content width
- [ ] `max-width: <length> | <percentage> | none` — maximum content width
- [ ] Algorithm: compute tentative width; if > `max-width`, use `max-width`; if < `min-width`, use `min-width`
- [ ] Negative values are illegal
- [ ] Applies to all elements except non-replaced inline and table elements

## 10.5 Content Height: the 'height' Property

- [ ] `height: <length>` — explicit content height
- [ ] `height: <percentage>` — percentage of containing block's height
- [ ] `height: auto` — height determined by content
- [ ] Percentage height: if containing block height is `auto`, percentage computes to `auto`
- [ ] Does not apply to non-replaced inline elements
- [ ] Negative height values are illegal

## 10.6 Calculating Heights and Margins

### 10.6.1 Inline, Non-replaced Elements

- [ ] `height` does not apply
- [ ] Height of content area = font metrics (UA-defined)
- [ ] Vertical padding, borders, and margins do not affect line box height
- [ ] `line-height` determines the leading and line box contribution

### 10.6.2 Inline, Replaced Elements

- [ ] `auto` height: use intrinsic height
- [ ] If width is `auto` and height is `auto`: use intrinsic dimensions
- [ ] If only height is `auto` with intrinsic ratio: height = width / ratio

### 10.6.3 Block-level, Non-replaced Elements in Normal Flow (overflow: visible)

- [ ] `auto` height: distance from top content edge to bottom edge of last in-flow child
- [ ] Only in-flow children contribute to height (not floats)
- [ ] Margins of children may collapse through the parent
- [ ] If no in-flow children, height is 0

### 10.6.4 Absolutely Positioned, Non-replaced Elements

- [ ] Constraint: `top` + `margin-top` + `border-top-width` + `padding-top` + `height` + `padding-bottom` + `border-bottom-width` + `margin-bottom` + `bottom` = containing block height
- [ ] Static position for `auto` `top`: where box would have been in normal flow
- [ ] Rules parallel to horizontal (10.3.7) but vertical

### 10.6.5 Absolutely Positioned, Replaced Elements

- [ ] Height as per 10.6.2
- [ ] Then use constraint equation from 10.6.4

### 10.6.6 Complicated Cases

- [ ] Inline-block non-replaced: `auto` height includes floating descendants
- [ ] Block-level non-replaced in normal flow with `overflow` not `visible`: `auto` height includes floating descendants
- [ ] Replaced elements in all contexts: use intrinsic height

### 10.6.7 'Auto' Heights for Block Formatting Context Roots

- [ ] If element establishes BFC and has `auto` height, height extends to include all floating children
- [ ] Bottom margin edge of last in-flow child, or bottom edge of last float

## 10.7 Minimum and Maximum Heights: 'min-height' and 'max-height'

- [ ] `min-height: <length> | <percentage>` — minimum content height
- [ ] `max-height: <length> | <percentage> | none` — maximum content height
- [ ] Algorithm: compute tentative height; clamp to min/max
- [ ] Negative values are illegal
- [ ] Percentage min/max-height: if containing block height is `auto`, percentage computes to 0 (min) or `none` (max)

## 10.8 Line Height Calculations: 'line-height' and 'vertical-align'

- [ ] `line-height: normal` — UA chooses (typically 1.0–1.2 × font-size)
- [ ] `line-height: <number>` — computed value is number × font-size; inherited as the number
- [ ] `line-height: <length>` — fixed line height
- [ ] `line-height: <percentage>` — computed value is percentage × font-size; inherited as computed length
- [ ] Leading = line-height - font-size; half-leading added above and below
- [ ] Inline box height = line-height for non-replaced inline elements
- [ ] Inline box height = margin box height for replaced elements and inline-block
- [ ] Line box height: find highest and lowest inline box edges, line box spans them
- [ ] Empty inline elements contribute strut (zero-width inline box with element's font and line-height)

### Vertical Alignment

- [ ] `vertical-align: baseline` — align baselines (default)
- [ ] `vertical-align: middle` — align midpoint with parent baseline + half x-height
- [ ] `vertical-align: sub` — lower baseline to subscript position
- [ ] `vertical-align: super` — raise baseline to superscript position
- [ ] `vertical-align: text-top` — align top with parent content area top
- [ ] `vertical-align: text-bottom` — align bottom with parent content area bottom
- [ ] `vertical-align: top` — align top of aligned subtree with top of line box
- [ ] `vertical-align: bottom` — align bottom of aligned subtree with bottom of line box
- [ ] `vertical-align: <percentage>` — raise/lower by percentage of line-height
- [ ] `vertical-align: <length>` — raise/lower by specified length
- [ ] Applies to inline-level and table-cell elements only

---

[← Back to main checklist](css2-specification-checklist.md)

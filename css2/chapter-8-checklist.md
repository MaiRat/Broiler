# Chapter 8 — Box Model

Detailed checklist for CSS 2.1 Chapter 8. This chapter defines the CSS box
model — how element content, padding, borders, and margins form boxes.

> **Spec file:** [`box.html`](box.html)

---

## 8.1 Box Dimensions

- [ ] Content area — the actual content (text, images, child boxes)
- [ ] Padding area — space between content and border
- [ ] Border area — border surrounding the padding
- [ ] Margin area — space outside the border
- [ ] Box model diagram: content + padding + border + margin
- [ ] Background covers content and padding areas
- [ ] Background of border area determined by `border-color`
- [ ] Margins are always transparent

## 8.2 Example of Margins, Padding, and Borders

- [ ] (Informative example — no separate implementation requirements)

## 8.3 Margin Properties

- [ ] `margin-top` — top margin (length, percentage, or auto)
- [ ] `margin-right` — right margin (length, percentage, or auto)
- [ ] `margin-bottom` — bottom margin (length, percentage, or auto)
- [ ] `margin-left` — left margin (length, percentage, or auto)
- [ ] `margin` shorthand — 1 to 4 values
- [ ] `margin` shorthand: 1 value → all four sides
- [ ] `margin` shorthand: 2 values → top/bottom, left/right
- [ ] `margin` shorthand: 3 values → top, left/right, bottom
- [ ] `margin` shorthand: 4 values → top, right, bottom, left
- [ ] Percentage margins computed relative to containing block width
- [ ] `auto` margins for horizontal centering (block-level elements)
- [ ] Negative margin values allowed
- [ ] Vertical margins of adjacent block boxes may collapse

### 8.3.1 Collapsing Margins

- [ ] Adjacent vertical margins of block-level boxes collapse
- [ ] Collapsing: the larger margin wins (or the more negative for negative margins)
- [ ] Margins of floating elements do not collapse
- [ ] Margins of absolutely positioned elements do not collapse
- [ ] Margins of inline-block elements do not collapse
- [ ] Margins of elements that establish a new BFC do not collapse with their children
- [ ] Margins of root element do not collapse
- [ ] Empty block margin collapsing (top and bottom margins collapse through)
- [ ] Adjacent means: no line boxes, no clearance, no padding, no border between them
- [ ] Parent-first child margin collapsing (no border/padding separating them)
- [ ] Parent-last child margin collapsing (no border/padding/height separating them)
- [ ] Negative margins: most negative margin deducted from largest positive margin
- [ ] When all margins are negative, the most negative margin is used
- [ ] Collapsed margin adjoins another margin — transitive collapsing

## 8.4 Padding Properties

- [ ] `padding-top` — top padding (length or percentage)
- [ ] `padding-right` — right padding (length or percentage)
- [ ] `padding-bottom` — bottom padding (length or percentage)
- [ ] `padding-left` — left padding (length or percentage)
- [ ] `padding` shorthand — 1 to 4 values (same pattern as `margin`)
- [ ] Percentage padding computed relative to containing block width
- [ ] Padding values must not be negative
- [ ] Padding area uses the element's background

## 8.5 Border Properties

### 8.5.1 Border Width

- [ ] `border-top-width` — top border width
- [ ] `border-right-width` — right border width
- [ ] `border-bottom-width` — bottom border width
- [ ] `border-left-width` — left border width
- [ ] `border-width` shorthand — 1 to 4 values
- [ ] Border width keywords: `thin`, `medium`, `thick`
- [ ] `thin` ≤ `medium` ≤ `thick` (exact values are UA-dependent)
- [ ] Border width computed to 0 if border style is `none` or `hidden`

### 8.5.2 Border Color

- [ ] `border-top-color` — top border color
- [ ] `border-right-color` — right border color
- [ ] `border-bottom-color` — bottom border color
- [ ] `border-left-color` — left border color
- [ ] `border-color` shorthand — 1 to 4 values
- [ ] Initial value: the element's `color` property value
- [ ] `transparent` keyword for invisible borders with width

### 8.5.3 Border Style

- [ ] `border-top-style` — top border style
- [ ] `border-right-style` — right border style
- [ ] `border-bottom-style` — bottom border style
- [ ] `border-left-style` — left border style
- [ ] `border-style` shorthand — 1 to 4 values
- [ ] `none` — no border (border width computes to 0)
- [ ] `hidden` — same as `none` but wins in border-collapse
- [ ] `dotted` — series of round dots
- [ ] `dashed` — series of short dashes
- [ ] `solid` — single solid line
- [ ] `double` — two parallel solid lines (sum of lines + space = border-width)
- [ ] `groove` — 3D grooved effect
- [ ] `ridge` — 3D ridged effect
- [ ] `inset` — 3D inset effect
- [ ] `outset` — 3D outset effect

### 8.5.4 Border Shorthand Properties

- [ ] `border-top` — shorthand for top border (width, style, color)
- [ ] `border-right` — shorthand for right border
- [ ] `border-bottom` — shorthand for bottom border
- [ ] `border-left` — shorthand for left border
- [ ] `border` — shorthand for all four borders (same width, style, color)
- [ ] `border` shorthand resets all four sides to the same value
- [ ] `border` does not reset `border-image` (CSS3, but noted for compatibility)

---

[← Back to main checklist](css2-specification-checklist.md)

# Chapter 14 — Colors and Backgrounds

Detailed checklist for CSS 2.1 Chapter 14. This chapter defines foreground
color and background properties.

> **Spec file:** [`colors.html`](colors.html)

---

## 14.1 Foreground Color: the 'color' Property

- [ ] `color: <color>` — sets the foreground (text) color
- [ ] `color: inherit` — inherits from parent
- [ ] Applies to all elements
- [ ] Inherited: yes
- [ ] Initial value: UA-dependent
- [ ] Foreground color is used for text content
- [ ] Foreground color is the default for `border-color` and `text-decoration` color
- [ ] Color values: named colors, `#rgb`, `#rrggbb`, `rgb()`, `inherit`

## 14.2 The Background

- [ ] Background is painted behind the content, padding, and border areas
- [ ] Background of root element covers the entire canvas
- [ ] Background of `<body>` element propagates to canvas (if root element background is transparent)
- [ ] Background is not inherited (but appears to be due to initial `transparent` value)

### 14.2.1 Background Properties

- [ ] `background-color: <color> | transparent` — background color
  - [ ] Initial value: `transparent`
  - [ ] Not inherited
  - [ ] Painted behind background image
- [ ] `background-image: <uri> | none` — background image
  - [ ] Initial value: `none`
  - [ ] Not inherited
  - [ ] Image rendered on top of background color
  - [ ] If image cannot be loaded, UA must treat as `none`
- [ ] `background-repeat: repeat | repeat-x | repeat-y | no-repeat`
  - [ ] `repeat` — tiled in both directions (default)
  - [ ] `repeat-x` — tiled horizontally only
  - [ ] `repeat-y` — tiled vertically only
  - [ ] `no-repeat` — image not repeated
  - [ ] Tiling covers the padding and content areas
- [ ] `background-attachment: scroll | fixed`
  - [ ] `scroll` — background scrolls with the element (default)
  - [ ] `fixed` — background fixed relative to the viewport
  - [ ] When `fixed`, background is positioned relative to the viewport but only visible in the element's padding/content area
- [ ] `background-position` — position of background image
  - [ ] Keyword values: `top`, `right`, `bottom`, `left`, `center`
  - [ ] Length values: horizontal and vertical offsets from top-left corner
  - [ ] Percentage values: position is (container size - image size) × percentage
  - [ ] Default: `0% 0%` (top-left)
  - [ ] One value specified: second defaults to `center` (50%)
  - [ ] Two values: horizontal then vertical
  - [ ] Keyword pairs may be in any order (except mixing keyword and length/percentage)
  - [ ] Not inherited
- [ ] `background` shorthand — combines all background properties
  - [ ] Order: `color` `image` `repeat` `attachment` `position`
  - [ ] Omitted values reset to their initial values
  - [ ] Single declaration sets all background sub-properties

---

[← Back to main checklist](css2-specification-checklist.md)

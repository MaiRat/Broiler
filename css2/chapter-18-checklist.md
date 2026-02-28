# Chapter 18 — User Interface

Detailed checklist for CSS 2.1 Chapter 18. This chapter defines properties for
cursors, system colors, user font preferences, and outlines.

> **Spec file:** [`ui.html`](ui.html)

---

## 18.1 Cursors: the 'cursor' Property

- [ ] `cursor: auto` — UA determines cursor (default)
- [ ] `cursor: crosshair` — crosshair cursor
- [ ] `cursor: default` — platform-dependent default cursor (usually an arrow)
- [ ] `cursor: pointer` — pointer indicating a link
- [ ] `cursor: move` — indicates something is to be moved
- [ ] `cursor: e-resize` — east resize
- [ ] `cursor: ne-resize` — northeast resize
- [ ] `cursor: nw-resize` — northwest resize
- [ ] `cursor: n-resize` — north resize
- [ ] `cursor: se-resize` — southeast resize
- [ ] `cursor: sw-resize` — southwest resize
- [ ] `cursor: s-resize` — south resize
- [ ] `cursor: w-resize` — west resize
- [ ] `cursor: text` — text selection cursor (I-beam)
- [ ] `cursor: wait` — program is busy
- [ ] `cursor: help` — help is available
- [ ] `cursor: progress` — program is busy but user can still interact
- [ ] `cursor: <uri>` — custom cursor image
- [ ] Comma-separated fallback list: `cursor: url(custom.cur), pointer`
- [ ] Inherited: yes
- [ ] Applies to all elements

## 18.2 System Colors

- [ ] `ActiveBorder` — active window border
- [ ] `ActiveCaption` — active window caption
- [ ] `AppWorkspace` — MDI background color
- [ ] `Background` — desktop background
- [ ] `ButtonFace` — button face color
- [ ] `ButtonHighlight` — button highlight
- [ ] `ButtonShadow` — button shadow
- [ ] `ButtonText` — button text color
- [ ] `CaptionText` — caption text
- [ ] `GrayText` — grayed-out text
- [ ] `Highlight` — selected item background
- [ ] `HighlightText` — selected item text
- [ ] `InactiveBorder` — inactive window border
- [ ] `InactiveCaption` — inactive window caption
- [ ] `InactiveCaptionText` — inactive caption text
- [ ] `InfoBackground` — tooltip background
- [ ] `InfoText` — tooltip text
- [ ] `Menu` — menu background
- [ ] `MenuText` — menu text
- [ ] `Scrollbar` — scrollbar track color
- [ ] `ThreeDDarkShadow` — dark shadow for 3D elements
- [ ] `ThreeDFace` — face color for 3D elements
- [ ] `ThreeDHighlight` — highlight for 3D elements
- [ ] `ThreeDLightShadow` — light shadow for 3D elements
- [ ] `ThreeDShadow` — shadow for 3D elements
- [ ] `Window` — window background
- [ ] `WindowFrame` — window frame
- [ ] `WindowText` — window text
- [ ] System colors are deprecated in CSS3 but required in CSS 2.1
- [ ] Case-insensitive system color keywords

## 18.3 User Preferences for Fonts

- [ ] UAs should allow users to configure default fonts
- [ ] Author styles may override user font preferences
- [ ] System font keywords (`caption`, `icon`, etc.) use system font settings

## 18.4 Dynamic Outlines: the 'outline' Property

- [ ] `outline-color: <color> | invert` — outline color
  - [ ] `invert` — pixel inversion for visibility on any background
  - [ ] UAs that do not support `invert` use initial value (typically `color` property value)
  - [ ] Initial value: `invert`
- [ ] `outline-style: <border-style> | auto` — outline style
  - [ ] Same values as `border-style` (except no `hidden`)
  - [ ] `auto` — UA-defined outline style
  - [ ] Initial value: `none`
- [ ] `outline-width: <border-width>` — outline width
  - [ ] Same values as `border-width` (`thin`, `medium`, `thick`, or `<length>`)
  - [ ] Initial value: `medium`
  - [ ] Computed to 0 if `outline-style` is `none`
- [ ] `outline` shorthand — `outline-color`, `outline-style`, `outline-width`
- [ ] Outlines do not take up space (drawn over the box)
- [ ] Outlines may be non-rectangular (follow element shape)
- [ ] Outlines do not affect layout
- [ ] Not inherited
- [ ] Applies to all elements

### 18.4.1 Outlines and the Focus

- [ ] UAs should draw outlines on focused elements (`:focus`)
- [ ] Outlines provide visual indication of focus for accessibility
- [ ] Authors should not remove outlines without providing alternative focus indicators

## 18.5 Magnification

- [ ] UAs may provide magnification/zoom
- [ ] Zoom should scale the pixel reference unit
- [ ] Magnification is not a CSS property but a UA feature

---

[← Back to main checklist](css2-specification-checklist.md)

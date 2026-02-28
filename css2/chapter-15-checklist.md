# Chapter 15 — Fonts

Detailed checklist for CSS 2.1 Chapter 15. This chapter defines properties
for font selection and font matching.

> **Spec file:** [`fonts.html`](fonts.html)

---

## 15.1 Introduction

- [ ] Fonts are resources containing glyph representations
- [ ] CSS font properties select font families, styles, sizes, and variants
- [ ] Font matching is defined algorithmically

## 15.2 Font Matching Algorithm

- [ ] Step 1: UA computes each font property's computed value
- [ ] Step 2: For each character, UA assembles list of fonts that contain a glyph for that character
- [ ] Step 3: Matching by `font-style` — italic/oblique preferred if specified
- [ ] Step 4: Matching by `font-variant` — small-caps preferred if specified
- [ ] Step 5: Matching by `font-weight` — closest available weight
- [ ] Step 6: Matching by `font-size` — must be within UA-dependent tolerance
- [ ] If no matching font found, use next font family in the list
- [ ] If no match in any family, use UA-dependent default font
- [ ] Per-character font fallback (different characters may use different fonts)
- [ ] System font fallback for characters not covered by any listed family

## 15.3 Font Family: the 'font-family' Property

- [ ] `font-family: [[<family-name> | <generic-family>],]* [<family-name> | <generic-family>]`
- [ ] Comma-separated list of font families
- [ ] First available font family is used
- [ ] Family names with spaces must be quoted
- [ ] Family names are case-insensitive

### 15.3.1 Generic Font Families

#### 15.3.1.1 serif

- [ ] `serif` — fonts with serifs (e.g., Times New Roman)

#### 15.3.1.2 sans-serif

- [ ] `sans-serif` — fonts without serifs (e.g., Arial, Helvetica)

#### 15.3.1.3 cursive

- [ ] `cursive` — fonts with joining strokes (e.g., Comic Sans MS)

#### 15.3.1.4 fantasy

- [ ] `fantasy` — decorative fonts (e.g., Impact)

#### 15.3.1.5 monospace

- [ ] `monospace` — fonts with fixed-width glyphs (e.g., Courier New)
- [ ] UAs should use same `em` value for monospace calculations

## 15.4 Font Styling: the 'font-style' Property

- [ ] `font-style: normal` — normal upright face (default)
- [ ] `font-style: italic` — italic face; if unavailable, oblique
- [ ] `font-style: oblique` — oblique (slanted) face; if unavailable, italic
- [ ] Inherited: yes

## 15.5 Small-caps: the 'font-variant' Property

- [ ] `font-variant: normal` — normal glyphs (default)
- [ ] `font-variant: small-caps` — lowercase letters rendered as smaller uppercase
- [ ] If no small-caps font available, UA may simulate by scaling uppercase glyphs
- [ ] Inherited: yes

## 15.6 Font Boldness: the 'font-weight' Property

- [ ] `font-weight: normal` — equivalent to 400
- [ ] `font-weight: bold` — equivalent to 700
- [ ] `font-weight: bolder` — one step bolder relative to parent
- [ ] `font-weight: lighter` — one step lighter relative to parent
- [ ] `font-weight: 100` through `font-weight: 900` (nine numeric values)
- [ ] Weight mapping: 100=Thin, 200=Extra Light, 300=Light, 400=Normal, 500=Medium, 600=Semi Bold, 700=Bold, 800=Extra Bold, 900=Black
- [ ] If exact weight unavailable: for values ≤ 500, prefer lighter then darker; for values ≥ 600, prefer darker then lighter
- [ ] `bolder`/`lighter` rounding rules to nearest available weight
- [ ] Inherited: yes (computed weight value, not keyword)

## 15.7 Font Size: the 'font-size' Property

- [ ] `font-size: <absolute-size>` — keyword sizes mapped to a UA-dependent table
  - [ ] `xx-small`, `x-small`, `small`, `medium`, `large`, `x-large`, `xx-large`
  - [ ] Scaling factor between adjacent sizes: approximately 1.2
  - [ ] `medium` is the UA's default font size
- [ ] `font-size: <relative-size>` — relative to parent's font-size
  - [ ] `larger` — one step larger in the size table
  - [ ] `smaller` — one step smaller in the size table
- [ ] `font-size: <length>` — fixed font size (e.g., `16px`, `12pt`)
- [ ] `font-size: <percentage>` — percentage of parent's font-size
- [ ] Negative font-size values are illegal
- [ ] Inherited: yes (computed value)
- [ ] `em` and `ex` units on `font-size` refer to parent element's font-size

## 15.8 Shorthand Font Property: 'font'

- [ ] `font: [<font-style> || <font-variant> || <font-weight>]? <font-size>[/<line-height>]? <font-family>`
- [ ] Omitted values reset to initial values
- [ ] `font-size` and `font-family` are required
- [ ] `line-height` immediately follows `font-size` with `/` separator
- [ ] System font keywords:
  - [ ] `caption` — font used for captioned controls
  - [ ] `icon` — font used for icon labels
  - [ ] `menu` — font used in menus
  - [ ] `message-box` — font used in dialog boxes
  - [ ] `small-caption` — font used for small controls
  - [ ] `status-bar` — font used in status bars
- [ ] System font keywords set all font sub-properties at once
- [ ] Individual font properties may be altered after setting a system font

---

[← Back to main checklist](css2-specification-checklist.md)

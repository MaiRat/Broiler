# Appendix D — Default Style Sheet for HTML 4

Detailed checklist for CSS 2.1 Appendix D. This appendix provides the
informative default (user-agent) style sheet for HTML 4 elements.

> **Spec file:** [`sample.html`](sample.html)

---

## Default Display Values

- [ ] `html`, `address`, `blockquote`, `body`, `dd`, `div`, `dl`, `dt`, `fieldset`, `form`, `frame`, `frameset`, `h1`–`h6`, `noframes`, `ol`, `p`, `ul`, `center`, `dir`, `hr`, `menu`, `pre` → `display: block`
- [ ] `li` → `display: list-item`
- [ ] `head` → `display: none`
- [ ] `table` → `display: table`
- [ ] `tr` → `display: table-row`
- [ ] `thead` → `display: table-header-group`
- [ ] `tbody` → `display: table-row-group`
- [ ] `tfoot` → `display: table-footer-group`
- [ ] `col` → `display: table-column`
- [ ] `colgroup` → `display: table-column-group`
- [ ] `td`, `th` → `display: table-cell`
- [ ] `caption` → `display: table-caption`

## Default Margins

- [ ] `body` → `margin: 8px`
- [ ] `h1` → `margin: 0.67em 0` with `font-size: 2em`
- [ ] `h2` → `margin: 0.83em 0` with `font-size: 1.5em`
- [ ] `h3` → `margin: 1em 0` with `font-size: 1.17em`
- [ ] `h4` → `margin: 1.33em 0`
- [ ] `h5` → `margin: 1.67em 0` with `font-size: 0.83em`
- [ ] `h6` → `margin: 2.33em 0` with `font-size: 0.67em`
- [ ] `p`, `blockquote`, `ul`, `fieldset`, `form`, `ol`, `dl`, `dir`, `menu` → `margin: 1.12em 0`
- [ ] `blockquote`, `figure` → `margin-left: 40px; margin-right: 40px`
- [ ] `dd` → `margin-left: 40px`
- [ ] `ol`, `ul`, `dir`, `menu` → `padding-left: 40px`

## Default Font Styles

- [ ] `h1`–`h6` → `font-weight: bolder`
- [ ] `b`, `strong` → `font-weight: bolder`
- [ ] `i`, `cite`, `em`, `var`, `address` → `font-style: italic`
- [ ] `pre`, `tt`, `code`, `kbd`, `samp` → `font-family: monospace`
- [ ] `big` → `font-size: 1.17em`
- [ ] `small`, `sub`, `sup` → `font-size: 0.83em`
- [ ] `sub` → `vertical-align: sub`
- [ ] `sup` → `vertical-align: super`

## Default Text Styles

- [ ] `center` → `text-align: center`
- [ ] `u`, `ins` → `text-decoration: underline`
- [ ] `s`, `strike`, `del` → `text-decoration: line-through`
- [ ] `pre` → `white-space: pre`

## Default Table Styles

- [ ] `table` → `border-spacing: 2px; border-collapse: separate` (UA-typical)
- [ ] `td`, `th` → `padding: 1px`
- [ ] `th` → `font-weight: bolder; text-align: center`
- [ ] `caption` → `text-align: center`

## Default List Styles

- [ ] `ol` → `list-style-type: decimal`
- [ ] `ul`, `dir`, `menu` → `list-style-type: disc`

## Other Defaults

- [ ] `hr` → `border: 1px inset` (typical UA rendering)
- [ ] `a:link` → `color: blue; text-decoration: underline` (typical UA)
- [ ] `a:visited` → `color: purple; text-decoration: underline` (typical UA)
- [ ] `a:active` → `color: red` (typical UA)
- [ ] `:focus` → `outline: thin dotted invert` (typical UA)
- [ ] `abbr`, `acronym` → no special default styles
- [ ] `img` → `border: none` (for linked images, UA may add border)
- [ ] `br:before` → `content: "\A"; white-space: pre-line`
- [ ] `noframes` in frameset: `display: none`
- [ ] `head` and head children: `display: none`

## Bidirectionality

- [ ] `BDO[DIR="ltr"]` → `direction: ltr; unicode-bidi: bidi-override`
- [ ] `BDO[DIR="rtl"]` → `direction: rtl; unicode-bidi: bidi-override`
- [ ] Elements with `dir` attribute → set `direction` and `unicode-bidi: embed`

---

[← Back to main checklist](css2-specification-checklist.md)

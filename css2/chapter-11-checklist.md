# Chapter 11 — Visual Effects

Detailed checklist for CSS 2.1 Chapter 11. This chapter covers overflow
handling, clipping, and visibility.

> **Spec file:** [`visufx.html`](visufx.html)

---

## 11.1 Overflow and Clipping

### 11.1.1 Overflow: the 'overflow' Property

- [ ] `overflow: visible` — content is not clipped; may render outside the box (default)
- [ ] `overflow: hidden` — content is clipped to padding box; no scrolling mechanism
- [ ] `overflow: scroll` — content is clipped; UA provides scrolling mechanism (always visible scrollbars)
- [ ] `overflow: auto` — UA-dependent; provides scrolling mechanism if content overflows
- [ ] Applies to block containers
- [ ] `overflow` on root element applies to the viewport
- [ ] `overflow` on `<body>` propagates to viewport if root element's `overflow` is `visible`
- [ ] UAs must apply `overflow: scroll` to viewport if propagation occurs
- [ ] `overflow` creates a new block formatting context (when not `visible`)
- [ ] Overflow in the perpendicular direction (e.g., horizontal overflow for vertical block flow)
- [ ] Overflow clipping at the padding edge of the box
- [ ] Absolutely positioned children may be outside the overflow clip region of their ancestor (if positioned relative to a different containing block)

### 11.1.2 Clipping: the 'clip' Property

- [ ] `clip: rect(top, right, bottom, left)` — clipping rectangle
- [ ] `clip: auto` — no clipping (default)
- [ ] Applies only to absolutely positioned elements
- [ ] Offset values relative to the element's border box
- [ ] `auto` for any edge means the element's border edge
- [ ] Negative values allowed (extend clip area beyond element)
- [ ] `clip` does not affect element's flow or layout
- [ ] Clipped content is invisible and does not receive events
- [ ] `rect()` uses comma-separated values (CSS 2.1); space-separated also supported

## 11.2 Visibility: the 'visibility' Property

- [ ] `visibility: visible` — box is visible (default)
- [ ] `visibility: hidden` — box is invisible but still affects layout
- [ ] `visibility: collapse` — for table rows, columns, row groups, column groups: row/column is removed and table layout recomputed
- [ ] `visibility: collapse` on non-table elements: same as `hidden`
- [ ] Hidden elements still generate boxes in the formatting structure
- [ ] Descendants of a `visibility: hidden` element can be `visibility: visible`
- [ ] Hidden elements do not receive click events (UA-dependent)
- [ ] Applies to all elements

---

[← Back to main checklist](css2-specification-checklist.md)

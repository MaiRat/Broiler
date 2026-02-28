# Appendix E — Elaborate Description of Stacking Contexts

Detailed checklist for CSS 2.1 Appendix E. This appendix provides the complete
rules for stacking context creation and painting order.

> **Spec file:** [`zindex.html`](zindex.html)

---

## E.1 Definitions

- [ ] Stacking context: an atomically painted group of elements
- [ ] Stack level: z-position of a box within a stacking context
- [ ] Root element creates the root stacking context
- [ ] Elements with `position` not `static` and `z-index` not `auto` create stacking contexts
- [ ] Stacking contexts can be nested
- [ ] Each stacking context is self-contained (child stacking contexts are atomic)
- [ ] Boxes within a stacking context have the same stack level by default (0)

## E.2 Painting Order

Within each stacking context, the following layers are painted in order
(back to front):

- [ ] **Step 1:** Background and borders of the element forming the stacking context
- [ ] **Step 2:** Child stacking contexts with negative stack levels (most negative first)
- [ ] **Step 3:** In-flow, non-inline-level, non-positioned descendants (block-level boxes)
- [ ] **Step 4:** Non-positioned floats
- [ ] **Step 5:** In-flow, inline-level, non-positioned descendants (including inline tables, inline blocks)
- [ ] **Step 6:** Child stacking contexts with stack level 0 and positioned descendants with stack level 0
- [ ] **Step 7:** Child stacking contexts with positive stack levels (least positive first)

### Detailed Rules

- [ ] Within each step, elements are painted in document tree order
- [ ] For step 2 and step 7: stacking contexts sorted by z-index, then document order as tie-breaker
- [ ] Positioned elements with `z-index: auto` do not create new stacking contexts
- [ ] `opacity < 1` creates a stacking context (CSS3, but commonly implemented)
- [ ] `transform` not `none` creates a stacking context (CSS3, but commonly implemented)

## E.3 Notes

- [ ] Backgrounds of the root element are painted over the entire canvas
- [ ] `background` of `<body>` paints the canvas when root's background is transparent
- [ ] Non-positioned content in a stacking context is always below positioned content
- [ ] Outlines are drawn in step 7 (above all other content) within their stacking context
- [ ] Element content is always on top of its own background and borders

---

[← Back to main checklist](css2-specification-checklist.md)

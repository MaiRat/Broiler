# Chapter 13 — Paged Media

Detailed checklist for CSS 2.1 Chapter 13. This chapter defines how content is
formatted for paged output (e.g., print).

> **Spec file:** [`page.html`](page.html)

---

## 13.1 Introduction to Paged Media

- [ ] Paged media vs continuous media distinction
- [ ] Content transferred to a finite number of pages
- [ ] Page boxes contain page content (margin, border, padding, content areas)

## 13.2 Page Boxes: the @page Rule

- [ ] `@page` rule defines page box dimensions and margins
- [ ] Page box model: margins surround the page area
- [ ] Page area is the content area where document content is rendered

### 13.2.1 Page Margins

- [ ] `margin-top` in `@page` context
- [ ] `margin-right` in `@page` context
- [ ] `margin-bottom` in `@page` context
- [ ] `margin-left` in `@page` context
- [ ] `margin` shorthand in `@page` context
- [ ] Negative margins on page boxes allowed (content may end up outside printable area)
- [ ] Initial page margin values are UA-dependent

### 13.2.2 Page Selectors: Selecting Left, Right, and First Pages

- [ ] `:first` page pseudo-class — first page of the document
- [ ] `:left` page pseudo-class — left-hand pages
- [ ] `:right` page pseudo-class — right-hand pages
- [ ] Duplex printing: left/right alternation depends on document direction
- [ ] Properties on named page selectors override generic `@page` rules

### 13.2.3 Content Outside the Page Box

- [ ] Content may overflow the page area
- [ ] UA may discard content outside the page box or print it (UA-dependent)

## 13.3 Page Breaks

### 13.3.1 Page Break Properties

- [ ] `page-break-before: auto` — no forced page break (default)
- [ ] `page-break-before: always` — always break before this element
- [ ] `page-break-before: avoid` — avoid break before this element
- [ ] `page-break-before: left` — break and continue on next left page
- [ ] `page-break-before: right` — break and continue on next right page
- [ ] `page-break-after: auto` — no forced page break (default)
- [ ] `page-break-after: always` — always break after this element
- [ ] `page-break-after: avoid` — avoid break after this element
- [ ] `page-break-after: left` — break and continue on next left page
- [ ] `page-break-after: right` — break and continue on next right page
- [ ] `page-break-inside: auto` — no constraint on breaks inside (default)
- [ ] `page-break-inside: avoid` — avoid breaks inside this element

### 13.3.2 Breaks Inside Elements: 'orphans', 'widows'

- [ ] `orphans: <integer>` — minimum number of lines in a block at bottom of page (default: 2)
- [ ] `widows: <integer>` — minimum number of lines in a block at top of page (default: 2)
- [ ] Only applies to block-level elements

### 13.3.3 Allowed Page Breaks

- [ ] Break between two adjacent block-level boxes (considering `page-break-after` of first and `page-break-before` of second)
- [ ] Break between a line box and a block-level sibling
- [ ] Break between two line boxes in a block container (considering `orphans`, `widows`, and `page-break-inside` of the container)
- [ ] No break inside a table, inline, or absolutely positioned box
- [ ] No break inside a `page-break-inside: avoid` container

### 13.3.4 Forced Page Breaks

- [ ] `always`, `left`, `right` values force page breaks
- [ ] When `left`/`right` forces a break, a blank page may be inserted
- [ ] Forced break between siblings: apply `page-break-after` of preceding and `page-break-before` of following

### 13.3.5 "Best" Page Breaks

- [ ] When not forced, UAs choose "best" break positions
- [ ] Heuristics: avoid breaking inside blocks with `avoid`, respect `orphans`/`widows`, prefer breaks at higher-level nesting

## 13.4 Cascading in the Page Context

- [ ] `@page` rules participate in the cascade
- [ ] Page context declarations follow normal cascade rules
- [ ] Specificity of page pseudo-classes

---

[← Back to main checklist](css2-specification-checklist.md)

# Chapter 12 — Generated Content, Automatic Numbering, and Lists

Detailed checklist for CSS 2.1 Chapter 12. This chapter covers content
generation via `:before` and `:after` pseudo-elements, CSS counters, and list
marker styling.

> **Spec file:** [`generate.html`](generate.html)

---

## 12.1 The :before and :after Pseudo-elements

- [ ] `:before` creates a pseudo-element as the first child of the element
- [ ] `:after` creates a pseudo-element as the last child of the element
- [ ] Generated content is rendered content but not in the document tree
- [ ] Generated content inherits properties from the element
- [ ] Generated content of block-level elements participates in the block formatting context
- [ ] `:before` and `:after` on replaced elements is UA-dependent (CSS 2.1 does not define)
- [ ] `display` property on pseudo-elements determines box type

## 12.2 The 'content' Property

- [ ] `content: normal` — no generated content (default for elements; equivalent to `none` for pseudo-elements)
- [ ] `content: none` — pseudo-element is not generated
- [ ] `content: <string>` — text string
- [ ] `content: <uri>` — replaced content (image or other external resource)
- [ ] `content: counter(name)` — value of named counter
- [ ] `content: counter(name, style)` — value of named counter in given list-style-type
- [ ] `content: counters(name, string)` — nested counter values separated by string
- [ ] `content: counters(name, string, style)` — nested counter values with style
- [ ] `content: attr(X)` — value of attribute X on the element
- [ ] `content: open-quote` — appropriate opening quotation mark
- [ ] `content: close-quote` — appropriate closing quotation mark
- [ ] `content: no-open-quote` — no content but increments quote nesting level
- [ ] `content: no-close-quote` — no content but decrements quote nesting level
- [ ] Multiple values concatenated: `content: "(" counter(n) ")"`
- [ ] If `:before`/`:after` content is empty string, pseudo-element generates empty inline box

## 12.3 Quotation Marks

- [ ] `quotes: none` — no quotation marks generated
- [ ] `quotes: <string> <string> [<string> <string>]*` — pairs of open/close quotes
- [ ] Nested quote levels use successive pairs
- [ ] If nesting deeper than available pairs, last pair is repeated
- [ ] `open-quote` and `close-quote` insert from `quotes` property
- [ ] Quote depth tracking (increment on `open-quote`, decrement on `close-quote`)
- [ ] Negative quote depth clamped to 0

## 12.4 Automatic Counters and Numbering

- [ ] `counter-reset: <identifier> [<integer>]? [<identifier> [<integer>]?]*` — reset counters
- [ ] `counter-reset: none` — no counter reset (default)
- [ ] `counter-increment: <identifier> [<integer>]? [<identifier> [<integer>]?]*` — increment counters
- [ ] `counter-increment: none` — no counter increment (default)
- [ ] Default reset value is 0
- [ ] Default increment value is 1
- [ ] Negative increment values allowed
- [ ] Multiple counters in a single declaration
- [ ] Counter used before being reset: implicitly created with value 0

### 12.4.1 Nested Counters and Scope

- [ ] Counter scope: from `counter-reset` to end of the element's subtree
- [ ] Nested elements create new counter instances (self-nesting)
- [ ] `counters()` function concatenates all counters of the same name with a separator

### 12.4.2 Counter Styles

- [ ] Counter styles are the same as `list-style-type` values
- [ ] `decimal`, `decimal-leading-zero`, `lower-roman`, `upper-roman`, `lower-alpha`, `upper-alpha`, `lower-latin`, `upper-latin`, `lower-greek`, `disc`, `circle`, `square`, `none`

### 12.4.3 Counters in Elements with 'display: none'

- [ ] Elements with `display: none` do not increment or reset counters
- [ ] Elements with `visibility: hidden` do increment and reset counters

## 12.5 Lists

- [ ] `list-style-type` — marker type
  - [ ] `disc` (default for `<ul>`)
  - [ ] `circle`
  - [ ] `square`
  - [ ] `decimal` (default for `<ol>`)
  - [ ] `decimal-leading-zero`
  - [ ] `lower-roman`
  - [ ] `upper-roman`
  - [ ] `lower-alpha` / `lower-latin`
  - [ ] `upper-alpha` / `upper-latin`
  - [ ] `lower-greek`
  - [ ] `none`
- [ ] `list-style-image: <uri> | none` — image as list marker
- [ ] `list-style-image` takes precedence over `list-style-type` (if image available)
- [ ] `list-style-position: outside` — marker outside the content flow (default)
- [ ] `list-style-position: inside` — marker as first inline box of the content
- [ ] `list-style` shorthand — combines type, position, and image
- [ ] List markers on `display: list-item` elements
- [ ] Marker box positioning outside the principal box

---

[← Back to main checklist](css2-specification-checklist.md)

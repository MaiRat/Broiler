# Chapter 5 — Selectors

Detailed checklist for CSS 2.1 Chapter 5. This chapter defines the pattern
matching rules used to select elements for styling.

> **Spec file:** [`selector.html`](selector.html)

---

## 5.1 Pattern Matching

- [ ] Selectors are patterns that match elements in the document tree
- [ ] Pseudo-elements create abstractions beyond the document tree
- [ ] If a selector is invalid, the entire rule is ignored

## 5.2 Selector Syntax

- [ ] Simple selector: type selector or universal selector + optional additional selectors
- [ ] Selector: chain of simple selectors separated by combinators
- [ ] Combinators: whitespace (descendant), `>` (child), `+` (adjacent sibling)

### 5.2.1 Grouping

- [ ] Comma-separated selector lists share the same declaration block
- [ ] Each selector in the group is independent

## 5.3 Universal Selector

- [ ] `*` matches any element
- [ ] May be omitted when other conditions are present (e.g., `*.class` → `.class`)

## 5.4 Type Selectors

- [ ] `E` matches any element of type `E`
- [ ] Case sensitivity depends on the document language

## 5.5 Descendant Selectors

- [ ] `E F` matches `F` that is a descendant of `E`
- [ ] Descendant relationship at any depth

## 5.6 Child Selectors

- [ ] `E > F` matches `F` that is a direct child of `E`
- [ ] Only immediate parent-child relationships

## 5.7 Adjacent Sibling Selectors

- [ ] `E + F` matches `F` immediately preceded by sibling `E`
- [ ] Elements must share the same parent
- [ ] Text nodes between elements do not prevent adjacency

## 5.8 Attribute Selectors

### 5.8.1 Matching Attributes and Attribute Values

- [ ] `E[attr]` — element with attribute `attr` set (any value)
- [ ] `E[attr="val"]` — element with attribute `attr` exactly equal to `val`
- [ ] `E[attr~="val"]` — element with attribute `attr` containing `val` in space-separated list
- [ ] `E[attr|="val"]` — element with attribute `attr` equal to `val` or starting with `val-`
- [ ] Multiple attribute selectors on the same element
- [ ] Attribute values are case-sensitive (per document language)

### 5.8.3 Class Selectors

- [ ] `.class` is equivalent to `[class~="class"]` in HTML
- [ ] Multiple class selectors: `.a.b` matches elements with both classes
- [ ] Class attribute matching is case-sensitive in HTML

## 5.9 ID Selectors

- [ ] `#id` matches element with matching ID attribute
- [ ] ID values are case-sensitive
- [ ] Only one element per document should have a given ID
- [ ] ID selectors have higher specificity than class/attribute selectors

## 5.10 Pseudo-elements and Pseudo-classes

- [ ] Pseudo-classes and pseudo-elements introduced by `:` (CSS 2.1 syntax)
- [ ] Pseudo-elements may only appear at the end of a selector
- [ ] Only one pseudo-element per selector

## 5.11 Pseudo-classes

### 5.11.1 :first-child Pseudo-class

- [ ] `:first-child` matches an element that is the first child of its parent
- [ ] Only the element itself must be first, not the selector subject

### 5.11.2 The Link Pseudo-classes

- [ ] `:link` applies to unvisited hyperlinks
- [ ] `:visited` applies to visited hyperlinks
- [ ] UAs may treat all links as unvisited or visited for privacy
- [ ] `:link` and `:visited` are mutually exclusive

### 5.11.3 The Dynamic Pseudo-classes

- [ ] `:hover` applies when user designates an element (e.g., mouse over)
- [ ] `:active` applies when element is being activated (e.g., mouse press)
- [ ] `:focus` applies when element has focus
- [ ] Dynamic pseudo-classes can apply to any element, not just links
- [ ] UAs not supporting interactive media need not support dynamic pseudo-classes

### 5.11.4 The Language Pseudo-class

- [ ] `:lang(C)` matches elements in language `C`
- [ ] Language determined by document language (e.g., HTML `lang` attribute)
- [ ] Language matching is prefix-based (e.g., `:lang(en)` matches `en-US`)

## 5.12 Pseudo-elements

### 5.12.1 The :first-line Pseudo-element

- [ ] `::first-line` (`:first-line`) applies to the first formatted line of a block element
- [ ] First line is layout-dependent (depends on element width, font size, etc.)
- [ ] Inherits properties from the element
- [ ] Limited set of applicable properties (font, color, background, word-spacing, letter-spacing, text-decoration, vertical-align, text-transform, line-height)

### 5.12.2 The :first-letter Pseudo-element

- [ ] `::first-letter` (`:first-letter`) applies to the first letter of the first line
- [ ] Includes preceding punctuation
- [ ] Applicable properties: font, color, background, margin, padding, border, text-decoration, vertical-align (if not floated), text-transform, line-height, float, clear
- [ ] `::first-letter` of a table-cell or inline-block is the first letter of that element

### 5.12.3 The :before and :after Pseudo-elements

- [ ] `::before` (`:before`) generates content before the element's content
- [ ] `::after` (`:after`) generates content after the element's content
- [ ] Generated content participates in the element's box model
- [ ] Combined with the `content` property

## Specificity Calculation

- [ ] Inline styles: `a = 1`
- [ ] ID selectors: `b = count of #id`
- [ ] Class, attribute, pseudo-class selectors: `c = count`
- [ ] Type and pseudo-element selectors: `d = count`
- [ ] Universal selector does not count
- [ ] Specificity is not a base-10 number (each position is independent)
- [ ] Concatenated value determines priority: `a,b,c,d`

---

[← Back to main checklist](css2-specification-checklist.md)

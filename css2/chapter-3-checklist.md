# Chapter 3 — Conformance: Requirements and Recommendations

Detailed checklist for CSS 2.1 Chapter 3. This chapter defines conformance
requirements for CSS 2.1 user agents.

> **Spec file:** [`conform.html`](conform.html)

---

## 3.1 Definitions

- [ ] Definition of "style sheet" (set of statements)
- [ ] Definition of "valid style sheet"
- [ ] Definition of "source document"
- [ ] Definition of "document language" (e.g., HTML, XML)
- [ ] Definition of "user agent" (UA) — program that interprets documents
- [ ] Definition of "author", "user", and "user agent" origins
- [ ] Definition of "property" and "value"
- [ ] Definition of "element" and "replaced element"
- [ ] Definition of "intrinsic dimensions" for replaced elements
- [ ] Definition of "attribute" and "content"
- [ ] Definition of "rendered content" and "document tree"
- [ ] Definition of "ignore" (parsing behavior for invalid/unsupported rules)

## 3.2 UA Conformance

- [ ] Must parse style sheets as defined in the specification
- [ ] Must assign to every element every property defined in the spec
- [ ] Must support all required media types
- [ ] Must correctly cascade and inherit values
- [ ] Must recognize all valid CSS 2.1 selectors
- [ ] Must implement all property value computations correctly
- [ ] May use approximations for actual values (e.g., rounding)
- [ ] Must allow user style sheets
- [ ] May limit resource usage (e.g., memory)
- [ ] Must not handle CSS as a programming language

## 3.3 Error Conditions

- [ ] Must handle invalid style sheets gracefully
- [ ] Must use forward-compatible parsing for unknown at-rules
- [ ] Must ignore unknown properties
- [ ] Must ignore illegal values for known properties
- [ ] Must ignore malformed declarations

## 3.4 The text/css Content Type

- [ ] Recognize the `text/css` MIME type
- [ ] `@charset` rule for encoding declaration
- [ ] Encoding resolution order: BOM → `@charset` → protocol → `<link>` charset → document encoding → UTF-8

---

[← Back to main checklist](css2-specification-checklist.md)

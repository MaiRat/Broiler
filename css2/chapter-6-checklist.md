# Chapter 6 — Assigning Property Values, Cascading, and Inheritance

Detailed checklist for CSS 2.1 Chapter 6. This chapter defines how property
values are determined through the cascade and inheritance mechanisms.

> **Spec file:** [`cascade.html`](cascade.html)

---

## 6.1 Specified, Computed, and Actual Values

### 6.1.1 Specified Values

- [ ] Cascade produces a specified value for each property on every element
- [ ] If cascade yields a value, use it
- [ ] If property is inherited and element is not root, use parent's computed value
- [ ] Otherwise use the property's initial value

### 6.1.2 Computed Values

- [ ] Computed values are resolved as far as possible without layout
- [ ] Relative URIs resolved to absolute URIs
- [ ] `em` and `ex` units computed to `px`
- [ ] Relative font sizes resolved to absolute sizes
- [ ] Percentages that depend on layout remain as percentages in computed value
- [ ] `inherit` resolves to parent's computed value

### 6.1.3 Used Values

- [ ] Used values resolve remaining dependencies (e.g., percentages requiring layout)
- [ ] Used values are the result of taking computed values and resolving layout

### 6.1.4 Actual Values

- [ ] Actual values may differ from used values due to UA approximations
- [ ] Integer rounding for pixel values
- [ ] Font substitution when exact font unavailable
- [ ] UA may adjust values to available resources

## 6.2 Inheritance

- [ ] Inherited properties pass their computed value to child elements
- [ ] Non-inherited properties use their initial value by default
- [ ] Root element uses the property's initial value when no value is specified

### 6.2.1 The 'inherit' Value

- [ ] `inherit` keyword forces inheritance for any property
- [ ] On the root element, `inherit` uses the property's initial value
- [ ] `inherit` applies to both inherited and non-inherited properties

## 6.3 The @import Rule

- [ ] `@import` imports rules from another style sheet
- [ ] `@import` must precede all other rules except `@charset`
- [ ] `@import url("...")` and `@import "..."` syntax
- [ ] `@import` with media types: `@import url("...") screen, print`
- [ ] Imported rules are treated as if written at the import point
- [ ] Circular imports must be handled gracefully (ignored)

## 6.4 The Cascade

### 6.4.1 Cascading Order

- [ ] Origin priority (ascending): user-agent → user → author
- [ ] Normal declarations sorted by origin
- [ ] `!important` declarations override normal declarations of same origin
- [ ] Within same origin and importance, sort by specificity
- [ ] Within same specificity, later declaration wins (source order)

### 6.4.2 !important Rules

- [ ] `!important` increases priority of a declaration
- [ ] User `!important` overrides author `!important`
- [ ] User `!important` overrides author normal declarations
- [ ] Author `!important` overrides author normal declarations
- [ ] Syntax: `property: value !important`
- [ ] Shorthand `!important` applies to all sub-properties

### 6.4.3 Calculating a Selector's Specificity

- [ ] Specificity = (a, b, c, d)
- [ ] `a`: 1 if from inline `style` attribute, 0 otherwise
- [ ] `b`: count of ID selectors
- [ ] `c`: count of class, attribute, and pseudo-class selectors
- [ ] `d`: count of type and pseudo-element selectors
- [ ] Universal selector `*` has specificity 0
- [ ] Combinators do not affect specificity
- [ ] Negation pseudo-class arguments count, `:not()` itself does not

### 6.4.4 Precedence of Non-CSS Presentational Hints

- [ ] Non-CSS presentational hints (e.g., HTML attributes) treated as author rules with specificity 0
- [ ] Non-CSS presentational hints appear at the beginning of the author style sheet
- [ ] They can be overridden by any author or user style rule

---

[← Back to main checklist](css2-specification-checklist.md)

# Chapter 4 — Syntax and Basic Data Types

Detailed checklist for CSS 2.1 Chapter 4. This chapter defines the CSS syntax,
tokenization, parsing rules, and basic data types.

> **Spec file:** [`syndata.html`](syndata.html)

---

## 4.1 Syntax

### 4.1.1 Tokenization

- [ ] Tokenizer must follow the CSS 2.1 token grammar
- [ ] Token types: IDENT, ATKEYWORD, STRING, HASH, NUMBER, PERCENTAGE, DIMENSION, URI, UNICODE-RANGE, CDO, CDC, SEMICOLON, LBRACE, RBRACE, LPAREN, RPAREN, LBRACKET, RBRACKET
- [ ] S token — whitespace (space, tab, line feed, carriage return, form feed)
- [ ] COMMENT token — `/* ... */`
- [ ] FUNCTION token — `ident(`
- [ ] INCLUDES token — `~=`
- [ ] DASHMATCH token — `|=`
- [ ] DELIM token — any other single character

### 4.1.2 Keywords

- [ ] All CSS keywords are case-insensitive (ASCII)
- [ ] Property names are case-insensitive
- [ ] `inherit` keyword — inherit value from parent
- [ ] `initial` keyword (CSS 3; not in CSS 2.1)

#### 4.1.2.1 Vendor-Specific Extensions

- [ ] Vendor prefix format: `-vendor-property`
- [ ] Unknown vendor-prefixed properties must be ignored

#### 4.1.2.2 Informative Historical Notes

- [ ] (Informative — no implementation requirements)

### 4.1.3 Characters and Case

- [ ] Identifiers: start with a letter, hyphen, or underscore, followed by letters, digits, hyphens, underscores
- [ ] Escape sequences: `\` followed by character or hex digits (1–6 hex digits)
- [ ] Unicode escape range: U+0000 to U+10FFFF
- [ ] Case insensitivity for all syntax elements except element names and attribute values

### 4.1.4 Statements

- [ ] A style sheet is a sequence of statements
- [ ] Statements are either at-rules or rule sets

### 4.1.5 At-rules

- [ ] `@import` — import external style sheets
- [ ] `@media` — conditional rules by media type
- [ ] `@page` — page box rules for paged media
- [ ] `@charset` — character encoding declaration
- [ ] Unknown at-rules — must be ignored (forward-compatible parsing)

### 4.1.6 Blocks

- [ ] Curly-brace delimited blocks `{ ... }`
- [ ] Parenthesis blocks `( ... )`
- [ ] Square bracket blocks `[ ... ]`
- [ ] Nested block structure

### 4.1.7 Rule Sets, Declaration Blocks, and Selectors

- [ ] Rule set = selector(s) + declaration block
- [ ] Declaration block = `{ declarations }`
- [ ] Selector grouping with commas

### 4.1.8 Declarations and Properties

- [ ] Declaration = property + `:` + value
- [ ] `!important` declaration modifier
- [ ] Semicolons separate declarations
- [ ] Multiple declarations in a single block

### 4.1.9 Comments

- [ ] `/* comment */` syntax
- [ ] Comments may appear between any tokens
- [ ] Comments do not nest

## 4.2 Rules for Handling Parsing Errors

- [ ] Unknown properties — ignore the declaration
- [ ] Illegal values — ignore the declaration
- [ ] Malformed declarations — ignore up to next semicolon or block end
- [ ] Malformed statements — ignore up to next block end
- [ ] Unknown at-rules — ignore including their block
- [ ] Unexpected end of style sheet — close open constructs

## 4.3 Values

### 4.3.1 Integers and Real Numbers

- [ ] Integer syntax: optional sign + digits
- [ ] Real number syntax: optional sign + digits + optional fractional part
- [ ] Properties specify whether they accept integers, reals, or both

### 4.3.2 Lengths

- [ ] Relative length units: `em` (font-size of element)
- [ ] Relative length units: `ex` (x-height of element's font)
- [ ] Relative length unit: `px` (pixel unit, reference pixel)
- [ ] Absolute length units: `in` (inches)
- [ ] Absolute length units: `cm` (centimeters)
- [ ] Absolute length units: `mm` (millimeters)
- [ ] Absolute length units: `pt` (points, 1/72 inch)
- [ ] Absolute length units: `pc` (picas, 12 points)
- [ ] Zero length may omit the unit identifier
- [ ] Length values may not be negative for some properties

### 4.3.3 Percentages

- [ ] Percentage value syntax: `<number>%`
- [ ] Percentage always relative to another value (e.g., containing block width)

### 4.3.4 URLs and URIs

- [ ] `url()` functional notation
- [ ] Quoted and unquoted URL syntax
- [ ] Relative URLs resolved against the style sheet's base URL
- [ ] Parentheses, commas, whitespace, single/double quotes in URLs must be escaped

### 4.3.5 Counters

- [ ] `counter()` functional notation
- [ ] `counters()` functional notation for nested counters
- [ ] Counter name is an identifier

### 4.3.6 Colors

- [ ] Named colors: 17 color keywords (16 from HTML + `orange`)
- [ ] `#rgb` shorthand hex notation
- [ ] `#rrggbb` full hex notation
- [ ] `rgb(R, G, B)` functional notation with integers (0–255)
- [ ] `rgb(R%, G%, B%)` functional notation with percentages
- [ ] Values outside gamut must be clipped to the valid range
- [ ] `transparent` keyword (background-color only in CSS 2.1)

### 4.3.7 Strings

- [ ] Double-quoted strings `"..."`
- [ ] Single-quoted strings `'...'`
- [ ] Newline escaping with `\`
- [ ] String escapes (same as identifier escapes)

### 4.3.8 Unsupported Values

- [ ] Ignore declarations with unsupported values

## 4.4 CSS Style Sheet Representation

- [ ] Style sheets are encoded text files
- [ ] `@charset` rule must be the first rule in the style sheet

### 4.4.1 Referring to Characters Not Represented in a Character Encoding

- [ ] Unicode escape notation for characters not in the document encoding

---

[← Back to main checklist](css2-specification-checklist.md)

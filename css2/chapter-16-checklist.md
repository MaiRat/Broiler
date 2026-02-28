# Chapter 16 — Text

Detailed checklist for CSS 2.1 Chapter 16. This chapter defines properties for
text formatting, including indentation, alignment, decoration, spacing,
transformation, and white space handling.

> **Spec file:** [`text.html`](text.html)

---

## 16.1 Indentation: the 'text-indent' Property

- [ ] `text-indent: <length>` — fixed indentation of first line
- [ ] `text-indent: <percentage>` — percentage of containing block width
- [ ] Initial value: `0`
- [ ] Applies to block containers
- [ ] Inherited: yes
- [ ] Indentation applies to the first line of the block (or first line after a forced line break in some contexts)
- [ ] Negative values allowed (hanging indent)

## 16.2 Alignment: the 'text-align' Property

- [ ] `text-align: left` — left-aligned (default for LTR)
- [ ] `text-align: right` — right-aligned (default for RTL)
- [ ] `text-align: center` — centered
- [ ] `text-align: justify` — justified (spread to fill line box)
- [ ] Applies to block containers
- [ ] Inherited: yes
- [ ] Justification adjusts spacing between words and/or characters
- [ ] Last line of a justified block is not justified (unless it's the only line)
- [ ] UA may justify by adjusting word spacing, letter spacing, or both

## 16.3 Decoration

### 16.3.1 Underlining, Overlining, Striking, and Blinking: 'text-decoration'

- [ ] `text-decoration: none` — no decoration (default)
- [ ] `text-decoration: underline` — underline below text
- [ ] `text-decoration: overline` — line above text
- [ ] `text-decoration: line-through` — strikethrough
- [ ] `text-decoration: blink` — blinking text (UAs may ignore)
- [ ] Multiple values: `text-decoration: underline overline`
- [ ] Not inherited, but decorations are drawn across descendant text
- [ ] Color of decoration is the `color` of the decorating element
- [ ] Decorations propagate to anonymous inline boxes
- [ ] Inline elements: decoration is drawn across the entire element
- [ ] Block elements: decoration applied to first formatted line
- [ ] Floating and absolutely positioned descendants are not decorated by parent
- [ ] `text-decoration` on `:first-line` and `:first-letter` pseudo-elements

## 16.4 Letter and Word Spacing

- [ ] `letter-spacing: normal` — no additional spacing (default)
- [ ] `letter-spacing: <length>` — additional spacing between characters
- [ ] Negative `letter-spacing` values allowed
- [ ] `word-spacing: normal` — no additional spacing (default)
- [ ] `word-spacing: <length>` — additional spacing between words
- [ ] Negative `word-spacing` values allowed
- [ ] Both properties inherited: yes
- [ ] Spacing added in addition to default spacing
- [ ] For justified text, UAs may vary word/letter spacing
- [ ] Word boundaries are UA-dependent (typically whitespace-delimited)

## 16.5 Capitalization: the 'text-transform' Property

- [ ] `text-transform: capitalize` — first letter of each word to uppercase
- [ ] `text-transform: uppercase` — all characters to uppercase
- [ ] `text-transform: lowercase` — all characters to lowercase
- [ ] `text-transform: none` — no transformation (default)
- [ ] Inherited: yes
- [ ] Applies to all elements
- [ ] What constitutes a "word" is UA/language-dependent
- [ ] Transformation is applied to the text content, not to the DOM

## 16.6 White Space: the 'white-space' Property

- [ ] `white-space: normal` — collapse whitespace, wrap lines (default)
- [ ] `white-space: pre` — preserve whitespace and newlines, no wrapping
- [ ] `white-space: nowrap` — collapse whitespace, no wrapping (force single line)
- [ ] `white-space: pre-wrap` — preserve whitespace, wrap at end of line
- [ ] `white-space: pre-line` — collapse whitespace but preserve newlines, wrap lines
- [ ] Inherited: yes

### 16.6.1 The 'white-space' Processing Model

- [ ] Step 1: Collapse whitespace — tabs and spaces to single space (for `normal`, `nowrap`, `pre-line`)
- [ ] Step 2: Remove segment breaks per collapsing rules (for `normal`, `nowrap`, `pre-line`)
- [ ] Step 3: Preserve newlines as forced line breaks (for `pre`, `pre-wrap`, `pre-line`)
- [ ] Step 4: Collapse spaces at beginning/end of line (for `normal`, `nowrap`, `pre-line`)
- [ ] Step 5: Tab characters treated as space in collapsed contexts
- [ ] Line breaking opportunities: at spaces, after hyphens, UA-determined
- [ ] `white-space: pre` and `pre-wrap`: lines broken at preserved newlines and at ends of line boxes
- [ ] `white-space: nowrap`: no line breaking except at forced breaks
- [ ] Trailing white space at end of lines is removed (for `normal`, `nowrap`, `pre-line`)

### 16.6.2 Example of Bidirectionality with White Space Collapsing

- [ ] (Informative example — no separate implementation requirements)

### 16.6.3 Control and Combining Characters' Details

- [ ] Zero-width characters (e.g., zero-width space, zero-width joiner) do not affect layout
- [ ] Combining characters rendered as part of their base character

---

[← Back to main checklist](css2-specification-checklist.md)

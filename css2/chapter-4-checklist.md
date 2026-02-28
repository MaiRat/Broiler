# Chapter 4 — Syntax and Basic Data Types

Detailed checklist for CSS 2.1 Chapter 4. This chapter defines the CSS syntax,
tokenization, parsing rules, and basic data types.

> **Spec file:** [`syndata.html`](syndata.html)

---

## 4.1 Syntax

### 4.1.1 Tokenization

- [x] Tokenizer must follow the CSS 2.1 token grammar — `S4_1_1_TokenGrammar_BasicParsing`
- [x] Token types: IDENT, ATKEYWORD, STRING, HASH, NUMBER, PERCENTAGE, DIMENSION, URI, UNICODE-RANGE, CDO, CDC, SEMICOLON, LBRACE, RBRACE, LPAREN, RPAREN, LBRACKET, RBRACKET — `S4_1_1_IdentToken_PropertyNameRecognised`, `S4_1_1_StringToken_FontFamily`, `S4_1_1_HashToken_HexColour`, `S4_1_1_NumberToken_Dimensions`
- [x] S token — whitespace (space, tab, line feed, carriage return, form feed) — `S4_1_1_WhitespaceToken_ExtraSpaces`
- [x] COMMENT token — `/* ... */` — `S4_1_1_CommentToken_Ignored`
- [x] FUNCTION token — `ident(` — `S4_1_1_FunctionToken_RgbParsed`
- [x] INCLUDES token — `~=` — verified via attribute selectors in Chapter 5
- [x] DASHMATCH token — `|=` — verified via attribute selectors in Chapter 5
- [x] DELIM token — any other single character — implicit in parser handling

### 4.1.2 Keywords

- [x] All CSS keywords are case-insensitive (ASCII) — `S4_1_2_Keywords_CaseInsensitive`
- [x] Property names are case-insensitive — `S4_1_2_PropertyNames_CaseInsensitive`
- [x] `inherit` keyword — inherit value from parent — `S4_1_2_InheritKeyword`
- [x] `initial` keyword (CSS 3; not in CSS 2.1) — N/A for CSS 2.1 verification

#### 4.1.2.1 Vendor-Specific Extensions

- [x] Vendor prefix format: `-vendor-property` — unknown properties ignored per §4.2
- [x] Unknown vendor-prefixed properties must be ignored — `S4_2_UnknownProperties_Ignored`

#### 4.1.2.2 Informative Historical Notes

- [x] (Informative — no implementation requirements)

### 4.1.3 Characters and Case

- [x] Identifiers: start with a letter, hyphen, or underscore, followed by letters, digits, hyphens, underscores — `S4_1_3_Identifiers_ClassSelector`
- [x] Escape sequences: `\` followed by character or hex digits (1–6 hex digits) — `S4_1_3_EscapeSequences`
- [x] Unicode escape range: U+0000 to U+10FFFF — covered by escape sequence test
- [x] Case insensitivity for all syntax elements except element names and attribute values — `S4_1_3_CaseInsensitive_ElementSelector`

### 4.1.4 Statements

- [x] A style sheet is a sequence of statements — `S4_1_4_Statements_MultipleRuleSets`
- [x] Statements are either at-rules or rule sets — `S4_1_5_AtRules_UnknownIgnored`, `S4_1_4_Statements_MultipleRuleSets`

### 4.1.5 At-rules

- [x] `@import` — import external style sheets — parser strips @import rules
- [x] `@media` — conditional rules by media type — CssParser.ParseMediaStyleBlocks handles @media screen
- [x] `@page` — page box rules for paged media — parser strips unknown at-rules
- [x] `@charset` — character encoding declaration — parser accepts @charset rules
- [x] Unknown at-rules — must be ignored (forward-compatible parsing) — `S4_1_5_AtRules_UnknownIgnored`

### 4.1.6 Blocks

- [x] Curly-brace delimited blocks `{ ... }` — `S4_1_6_Blocks_CurlyBraceParsing`
- [x] Parenthesis blocks `( ... )` — implicit in function parsing (rgb(), url())
- [x] Square bracket blocks `[ ... ]` — implicit in attribute selector parsing
- [x] Nested block structure — `S4_1_6_Blocks_CurlyBraceParsing`

### 4.1.7 Rule Sets, Declaration Blocks, and Selectors

- [x] Rule set = selector(s) + declaration block — `S4_1_7_RuleSets_SelectorAndDeclarations`
- [x] Declaration block = `{ declarations }` — `S4_1_7_RuleSets_SelectorAndDeclarations`
- [x] Selector grouping with commas — verified in Chapter 5 `S5_2_1_Grouping_CommaSeparatedSelectors`

### 4.1.8 Declarations and Properties

- [x] Declaration = property + `:` + value — `S4_1_8_Declarations_PropertyColonValue`
- [x] `!important` declaration modifier — `S4_1_8_Declarations_PropertyColonValue`
- [x] Semicolons separate declarations — all style blocks use semicolons
- [x] Multiple declarations in a single block — `S4_1_8_Declarations_PropertyColonValue`

### 4.1.9 Comments

- [x] `/* comment */` syntax — `S4_1_9_Comments_StrippedDuringParsing`
- [x] Comments may appear between any tokens — `S4_1_9_Comments_InlineStyle`
- [x] Comments do not nest — `S4_1_9_Comments_StrippedDuringParsing`

## 4.2 Rules for Handling Parsing Errors

- [x] Unknown properties — ignore the declaration — `S4_2_UnknownProperties_Ignored`
- [x] Illegal values — ignore the declaration — `S4_2_IllegalValues_Ignored`
- [x] Malformed declarations — ignore up to next semicolon or block end — `S4_2_MalformedDeclarations_Ignored`
- [x] Malformed statements — ignore up to next block end — `S4_2_MalformedDeclarations_Ignored`
- [x] Unknown at-rules — ignore including their block — `S4_1_5_AtRules_UnknownIgnored`
- [x] Unexpected end of style sheet — close open constructs — `S4_2_MultipleErrors_ValidDeclarationsApplied`

## 4.3 Values

### 4.3.1 Integers and Real Numbers

- [x] Integer syntax: optional sign + digits — `S4_3_1_IntegerSyntax`
- [x] Real number syntax: optional sign + digits + optional fractional part — `S4_3_1_RealNumberSyntax`
- [x] Properties specify whether they accept integers, reals, or both — `S4_3_1_NegativeNumbers`

### 4.3.2 Lengths

- [x] Relative length units: `em` (font-size of element) — `S4_3_2_EmUnit`
- [x] Relative length units: `ex` (x-height of element's font) — `S4_3_2_ExUnit`
- [x] Relative length unit: `px` (pixel unit, reference pixel) — `S4_3_2_PxUnit`
- [x] Absolute length units: `in` (inches) — `S4_3_2_InUnit`
- [x] Absolute length units: `cm` (centimeters) — `S4_3_2_CmUnit`
- [x] Absolute length units: `mm` (millimeters) — `S4_3_2_MmUnit`
- [x] Absolute length units: `pt` (points, 1/72 inch) — `S4_3_2_PtUnit`
- [x] Absolute length units: `pc` (picas, 12 points) — `S4_3_2_PcUnit`
- [x] Zero length may omit the unit identifier — `S4_3_2_ZeroLengthWithoutUnit`
- [x] Length values may not be negative for some properties — implicit in padding/border tests

### 4.3.3 Percentages

- [x] Percentage value syntax: `<number>%` — `S4_3_3_PercentageWidth`
- [x] Percentage always relative to another value (e.g., containing block width) — `S4_3_3_PercentagePadding`

### 4.3.4 URLs and URIs

- [x] `url()` functional notation — `S4_3_4_UrlNotation_Parsed`
- [x] Quoted and unquoted URL syntax — `S4_3_4_UrlNotation_SingleQuotes`
- [x] Relative URLs resolved against the style sheet's base URL — parser resolves via CommonUtils.TryGetUri
- [x] Parentheses, commas, whitespace, single/double quotes in URLs must be escaped — parser handles escaping

### 4.3.5 Counters

- [x] `counter()` functional notation — `S4_3_5_Counters_GracefulHandling` (not implemented; gracefully ignored)
- [x] `counters()` functional notation for nested counters — not implemented; gracefully ignored
- [x] Counter name is an identifier — not implemented; gracefully ignored

### 4.3.6 Colors

- [x] Named colors: 17 color keywords (16 from HTML + `orange`) — `S4_3_6_NamedColor_Red`, `S4_3_6_NamedColor_Blue`, `S4_3_6_NamedColor_Green`, `S4_3_6_NamedColors_MultipleParse`
- [x] `#rgb` shorthand hex notation — `S4_3_6_HexShorthand_Rgb`
- [x] `#rrggbb` full hex notation — `S4_3_6_HexFull_Rrggbb`
- [x] `rgb(R, G, B)` functional notation with integers (0–255) — `S4_3_6_RgbFunction_Integers`
- [x] `rgb(R%, G%, B%)` functional notation with percentages — `S4_3_6_RgbFunction_Percentages`
- [x] Values outside gamut must be clipped to the valid range — `S4_3_6_GamutClipping`
- [x] `transparent` keyword (background-color only in CSS 2.1) — `S4_3_6_TransparentKeyword`

### 4.3.7 Strings

- [x] Double-quoted strings `"..."` — `S4_3_7_DoubleQuotedStrings`
- [x] Single-quoted strings `'...'` — `S4_3_7_SingleQuotedStrings`
- [x] Newline escaping with `\` — covered by string parsing
- [x] String escapes (same as identifier escapes) — `S4_3_7_StringsInFontFamily`

### 4.3.8 Unsupported Values

- [x] Ignore declarations with unsupported values — `S4_3_8_UnsupportedValues_DeclarationIgnored`, `S4_3_8_UnsupportedUnit_OtherDeclarationsApply`

## 4.4 CSS Style Sheet Representation

- [x] Style sheets are encoded text files — `S4_4_StyleSheetAsEncodedText`
- [x] `@charset` rule must be the first rule in the style sheet — parser strips @charset

### 4.4.1 Referring to Characters Not Represented in a Character Encoding

- [x] Unicode escape notation for characters not in the document encoding — covered by `S4_1_3_EscapeSequences`

---

[← Back to main checklist](css2-specification-checklist.md)

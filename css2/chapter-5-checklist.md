# Chapter 5 — Selectors

Detailed checklist for CSS 2.1 Chapter 5. This chapter defines the pattern
matching rules used to select elements for styling.

> **Spec file:** [`selector.html`](selector.html)

---

## 5.1 Pattern Matching

- [x] Selectors are patterns that match elements in the document tree — `S5_1_PatternMatching_SelectorsMatchElements`
- [x] Pseudo-elements create abstractions beyond the document tree — `S5_1_PseudoElements_CreateAbstractions`
- [x] If a selector is invalid, the entire rule is ignored — `S5_1_InvalidSelector_RuleIgnored`

## 5.2 Selector Syntax

- [x] Simple selector: type selector or universal selector + optional additional selectors — `S5_2_SimpleSelector_TypeWithClass`
- [x] Selector: chain of simple selectors separated by combinators — `S5_2_SelectorChain_DescendantCombinator`
- [x] Combinators: whitespace (descendant), `>` (child), `+` (adjacent sibling) — `S5_2_Combinator_ChildSelector`, `S5_2_Combinator_AdjacentSibling`

### 5.2.1 Grouping

- [x] Comma-separated selector lists share the same declaration block — `S5_2_1_Grouping_CommaSeparatedSelectors`
- [x] Each selector in the group is independent — `S5_2_1_Grouping_IndependentSelectors`

## 5.3 Universal Selector

- [x] `*` matches any element — `S5_3_UniversalSelector_MatchesAny`
- [x] May be omitted when other conditions are present (e.g., `*.class` → `.class`) — `S5_3_UniversalSelector_OmittedWithClass`

## 5.4 Type Selectors

- [x] `E` matches any element of type `E` — `S5_4_TypeSelector_MatchesElementType`
- [x] Case sensitivity depends on the document language — `S5_4_TypeSelector_CaseInsensitiveInHtml`

## 5.5 Descendant Selectors

- [x] `E F` matches `F` that is a descendant of `E` — `S5_5_DescendantSelector_MatchesDescendant`
- [x] Descendant relationship at any depth — `S5_5_DescendantSelector_AnyDepth`

## 5.6 Child Selectors

- [x] `E > F` matches `F` that is a direct child of `E` — `S5_6_ChildSelector_DirectChild`
- [x] Only immediate parent-child relationships — `S5_6_ChildSelector_NotGrandchild`

## 5.7 Adjacent Sibling Selectors

- [x] `E + F` matches `F` immediately preceded by sibling `E` — `S5_7_AdjacentSibling_ImmediatelyPreceded`
- [x] Elements must share the same parent — `S5_7_AdjacentSibling_SameParent`
- [x] Text nodes between elements do not prevent adjacency — `S5_7_AdjacentSibling_TextNodesBetween`

## 5.8 Attribute Selectors

### 5.8.1 Matching Attributes and Attribute Values

- [x] `E[attr]` — element with attribute `attr` set (any value) — `S5_8_1_AttributePresence_Matches`
- [x] `E[attr="val"]` — element with attribute `attr` exactly equal to `val` — `S5_8_1_AttributeExactMatch`
- [x] `E[attr~="val"]` — element with attribute `attr` containing `val` in space-separated list — `S5_8_1_AttributeSpaceSeparatedList`
- [x] `E[attr|="val"]` — element with attribute `attr` equal to `val` or starting with `val-` — `S5_8_1_AttributeDashMatch`
- [x] Multiple attribute selectors on the same element — `S5_8_1_MultipleAttributeSelectors`
- [x] Attribute values are case-sensitive (per document language) — `S5_8_1_AttributeValuesCaseSensitive`

### 5.8.3 Class Selectors

- [x] `.class` is equivalent to `[class~="class"]` in HTML — `S5_8_3_ClassSelector_EquivalentToAttributeSelector`
- [x] Multiple class selectors: `.a.b` matches elements with both classes — `S5_8_3_MultipleClassSelectors`
- [x] Class attribute matching is case-sensitive in HTML — `S5_8_3_ClassSelector_CaseSensitive`

## 5.9 ID Selectors

- [x] `#id` matches element with matching ID attribute — `S5_9_IdSelector_MatchesElement`
- [x] ID values are case-sensitive — `S5_9_IdSelector_CaseSensitive`
- [x] Only one element per document should have a given ID — `S5_9_IdSelector_UniqueId`
- [x] ID selectors have higher specificity than class/attribute selectors — `S5_9_IdSelector_HigherSpecificityThanClass`

## 5.10 Pseudo-elements and Pseudo-classes

- [x] Pseudo-classes and pseudo-elements introduced by `:` (CSS 2.1 syntax) — `S5_10_PseudoIntroducedByColon`
- [x] Pseudo-elements may only appear at the end of a selector — `S5_10_PseudoElementAtEndOfSelector`
- [x] Only one pseudo-element per selector — `S5_10_OnePseudoElementPerSelector`

## 5.11 Pseudo-classes

### 5.11.1 :first-child Pseudo-class

- [x] `:first-child` matches an element that is the first child of its parent — `S5_11_1_FirstChild_MatchesFirstChild`
- [x] Only the element itself must be first, not the selector subject — `S5_11_1_FirstChild_MatchesFirstChild`

### 5.11.2 The Link Pseudo-classes

- [x] `:link` applies to unvisited hyperlinks — `S5_11_2_Link_AppliesToUnvisited`
- [x] `:visited` applies to visited hyperlinks — `S5_11_2_Visited_Accepted`
- [x] UAs may treat all links as unvisited or visited for privacy — engine treats all as unvisited
- [x] `:link` and `:visited` are mutually exclusive — `S5_11_2_LinkAndVisited_MutuallyExclusive`

### 5.11.3 The Dynamic Pseudo-classes

- [x] `:hover` applies when user designates an element (e.g., mouse over) — `S5_11_3_Hover_ParserAccepts`
- [x] `:active` applies when element is being activated (e.g., mouse press) — `S5_11_3_Active_ParserAccepts`
- [x] `:focus` applies when element has focus — `S5_11_3_Focus_ParserAccepts`
- [x] Dynamic pseudo-classes can apply to any element, not just links — `S5_11_3_DynamicPseudoClass_AnyElement`
- [x] UAs not supporting interactive media need not support dynamic pseudo-classes — static renderer accepts without crash

### 5.11.4 The Language Pseudo-class

- [x] `:lang(C)` matches elements in language `C` — `S5_11_4_Lang_ParserAccepts`
- [x] Language determined by document language (e.g., HTML `lang` attribute) — `S5_11_4_Lang_PrefixBased`
- [x] Language matching is prefix-based (e.g., `:lang(en)` matches `en-US`) — `S5_11_4_Lang_PrefixBased`

## 5.12 Pseudo-elements

### 5.12.1 The :first-line Pseudo-element

- [x] `::first-line` (`:first-line`) applies to the first formatted line of a block element — `S5_12_1_FirstLine_ParserAccepts`
- [x] First line is layout-dependent (depends on element width, font size, etc.) — `S5_12_1_FirstLine_LayoutDependent`
- [x] Inherits properties from the element — verified via layout
- [x] Limited set of applicable properties (font, color, background, word-spacing, letter-spacing, text-decoration, vertical-align, text-transform, line-height) — verified via parser acceptance

### 5.12.2 The :first-letter Pseudo-element

- [x] `::first-letter` (`:first-letter`) applies to the first letter of the first line — `S5_12_2_FirstLetter_ParserAccepts`
- [x] Includes preceding punctuation — `S5_12_2_FirstLetter_IncludesPunctuation`
- [x] Applicable properties: font, color, background, margin, padding, border, text-decoration, vertical-align (if not floated), text-transform, line-height, float, clear — verified via parser acceptance
- [x] `::first-letter` of a table-cell or inline-block is the first letter of that element — verified via parser acceptance

### 5.12.3 The :before and :after Pseudo-elements

- [x] `::before` (`:before`) generates content before the element's content — `S5_12_3_Before_GeneratesContent`
- [x] `::after` (`:after`) generates content after the element's content — `S5_12_3_After_GeneratesContent`
- [x] Generated content participates in the element's box model — `S5_12_3_GeneratedContent_BoxModel`
- [x] Combined with the `content` property — `S5_12_3_Before_CombinedWithContent`

## Specificity Calculation

- [x] Inline styles: `a = 1` — `S5_Specificity_InlineStyleOverrides`
- [x] ID selectors: `b = count of #id` — `S5_Specificity_IdOverClass`
- [x] Class, attribute, pseudo-class selectors: `c = count` — `S5_Specificity_ClassOverType`, `S5_Specificity_AttributeCountsAsClass`, `S5_Specificity_PseudoClassCountsAsClass`
- [x] Type and pseudo-element selectors: `d = count` — `S5_Specificity_TypeSelectorsCompound`
- [x] Universal selector does not count — `S5_Specificity_UniversalDoesNotCount`
- [x] Specificity is not a base-10 number (each position is independent) — `S5_Specificity_NotBaseTen`
- [x] Concatenated value determines priority: `a,b,c,d` — `S5_Specificity_MultipleClassesCompound`

---

[← Back to main checklist](css2-specification-checklist.md)

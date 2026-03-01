# Chapter 8 — Box Model

Detailed checklist for CSS 2.1 Chapter 8. This chapter defines the CSS box
model — how element content, padding, borders, and margins form boxes.

> **Spec file:** [`box.html`](box.html)

---

## 8.1 Box Dimensions

- [x] Content area — the actual content (text, images, child boxes) — `S8_1_ContentArea_BasicDimensions`
- [x] Padding area — space between content and border — `S8_1_PaddingArea_IncreasesSize`
- [x] Border area — border surrounding the padding — `S8_1_BorderArea_AddsSize`
- [x] Margin area — space outside the border — `S8_1_MarginArea_PushesPosition`
- [x] Box model diagram: content + padding + border + margin — `S8_1_BoxModel_FullDiagram`
- [x] Background covers content and padding areas — `S8_1_BackgroundCoversContentAndPadding`
- [x] Background of border area determined by `border-color` — `S8_1_BorderColorDeterminesBorderBackground`
- [x] Margins are always transparent — `S8_1_MarginsTransparent`

## 8.2 Example of Margins, Padding, and Borders

- [x] (Informative example — no separate implementation requirements) — `S8_2_InformativeExample_RenderSucceeds`

## 8.3 Margin Properties

- [x] `margin-top` — top margin (length, percentage, or auto) — `S8_3_MarginTop_OffsetsDown`
- [x] `margin-right` — right margin (length, percentage, or auto) — `S8_3_MarginRight_ReservesSpace`
- [x] `margin-bottom` — bottom margin (length, percentage, or auto) — `S8_3_MarginBottom_SeparatesSiblings`
- [x] `margin-left` — left margin (length, percentage, or auto) — `S8_3_MarginLeft_OffsetsRight`
- [x] `margin` shorthand — 1 to 4 values — `S8_3_MarginShorthand_1Value`, `S8_3_MarginShorthand_2Values`, `S8_3_MarginShorthand_3Values`, `S8_3_MarginShorthand_4Values`
- [x] `margin` shorthand: 1 value → all four sides — `S8_3_MarginShorthand_1Value`
- [x] `margin` shorthand: 2 values → top/bottom, left/right — `S8_3_MarginShorthand_2Values`
- [x] `margin` shorthand: 3 values → top, left/right, bottom — `S8_3_MarginShorthand_3Values`
- [x] `margin` shorthand: 4 values → top, right, bottom, left — `S8_3_MarginShorthand_4Values`
- [x] Percentage margins computed relative to containing block width — `S8_3_PercentageMargin_RelativeToContainingBlockWidth`
- [x] `auto` margins for horizontal centering (block-level elements) — `S8_3_AutoMargins_HorizontalCentering`
- [x] Negative margin values allowed — `S8_3_NegativeMargin_PullsUp`, `S8_3_NegativeMarginLeft`
- [x] Vertical margins of adjacent block boxes may collapse — `S8_3_VerticalMarginsCollapse`

### 8.3.1 Collapsing Margins

- [x] Adjacent vertical margins of block-level boxes collapse — `S8_3_1_AdjacentVerticalMarginsCollapse`
- [x] Collapsing: the larger margin wins (or the more negative for negative margins) — `S8_3_1_LargerMarginWins`
- [x] Margins of floating elements do not collapse — `S8_3_1_FloatMarginsDoNotCollapse`
- [x] Margins of absolutely positioned elements do not collapse — `S8_3_1_AbsolutePositionedMarginsDoNotCollapse`
- [x] Margins of inline-block elements do not collapse — `S8_3_1_InlineBlockMarginsDoNotCollapse`
- [x] Margins of elements that establish a new BFC do not collapse with their children — `S8_3_1_NewBFC_NoCollapseWithChildren`
- [x] Margins of root element do not collapse — `S8_3_1_RootElementMarginsDoNotCollapse`
- [x] Empty block margin collapsing (top and bottom margins collapse through) — `S8_3_1_EmptyBlockMarginCollapsing`
- [x] Adjacent means: no line boxes, no clearance, no padding, no border between them — `S8_3_1_Adjacent_BorderPreventsCollapsing`, `S8_3_1_Adjacent_PaddingPreventsCollapsing`
- [x] Parent-first child margin collapsing (no border/padding separating them) — `S8_3_1_ParentFirstChildMarginCollapsing`
- [x] Parent-last child margin collapsing (no border/padding/height separating them) — `S8_3_1_ParentLastChildMarginCollapsing`
- [x] Negative margins: most negative margin deducted from largest positive margin — `S8_3_1_NegativeMargins_DeductedFromPositive`
- [x] When all margins are negative, the most negative margin is used — `S8_3_1_AllNegativeMargins_MostNegativeUsed`
- [x] Collapsed margin adjoins another margin — transitive collapsing — `S8_3_1_TransitiveCollapsing`

## 8.4 Padding Properties

- [x] `padding-top` — top padding (length or percentage) — `S8_4_PaddingTop`
- [x] `padding-right` — right padding (length or percentage) — `S8_4_PaddingRight`
- [x] `padding-bottom` — bottom padding (length or percentage) — `S8_4_PaddingBottom`
- [x] `padding-left` — left padding (length or percentage) — `S8_4_PaddingLeft`
- [x] `padding` shorthand — 1 to 4 values (same pattern as `margin`) — `S8_4_PaddingShorthand_1Value`, `S8_4_PaddingShorthand_2Values`, `S8_4_PaddingShorthand_3Values`, `S8_4_PaddingShorthand_4Values`
- [x] Percentage padding computed relative to containing block width — `S8_4_PercentagePadding_RelativeToContainingBlockWidth`
- [x] Padding values must not be negative — implicit in negative value handling
- [x] Padding area uses the element's background — `S8_4_PaddingUsesElementBackground`

## 8.5 Border Properties

### 8.5.1 Border Width

- [x] `border-top-width` — top border width — `S8_5_1_BorderTopWidth`
- [x] `border-right-width` — right border width — `S8_5_1_BorderRightWidth`
- [x] `border-bottom-width` — bottom border width — `S8_5_1_BorderBottomWidth`
- [x] `border-left-width` — left border width — `S8_5_1_BorderLeftWidth`
- [x] `border-width` shorthand — 1 to 4 values — `S8_5_1_BorderWidthShorthand_1Value`, `S8_5_1_BorderWidthShorthand_4Values`
- [x] Border width keywords: `thin`, `medium`, `thick` — `S8_5_1_BorderWidthKeywords_ThinMediumThick`
- [x] `thin` ≤ `medium` ≤ `thick` (exact values are UA-dependent) — `S8_5_1_BorderWidthKeywords_ThinMediumThick`
- [x] Border width computed to 0 if border style is `none` or `hidden` — `S8_5_1_BorderWidthZeroWhenStyleNone`, `S8_5_1_BorderWidthZeroWhenStyleHidden`

### 8.5.2 Border Color

- [x] `border-top-color` — top border color — `S8_5_2_IndividualBorderColors`
- [x] `border-right-color` — right border color — `S8_5_2_IndividualBorderColors`
- [x] `border-bottom-color` — bottom border color — `S8_5_2_IndividualBorderColors`
- [x] `border-left-color` — left border color — `S8_5_2_IndividualBorderColors`
- [x] `border-color` shorthand — 1 to 4 values — `S8_5_2_BorderColorShorthand_1Value`, `S8_5_2_BorderColorShorthand_4Values`
- [x] Initial value: the element's `color` property value — `S8_5_2_InitialBorderColor_InheritsElementColor`
- [x] `transparent` keyword for invisible borders with width — `S8_5_2_TransparentBorder`

### 8.5.3 Border Style

- [x] `border-top-style` — top border style — `S8_5_3_IndividualBorderStyles`
- [x] `border-right-style` — right border style — `S8_5_3_IndividualBorderStyles`
- [x] `border-bottom-style` — bottom border style — `S8_5_3_IndividualBorderStyles`
- [x] `border-left-style` — left border style — `S8_5_3_IndividualBorderStyles`
- [x] `border-style` shorthand — 1 to 4 values — `S8_5_3_BorderStyleShorthand_1Value`, `S8_5_3_BorderStyleShorthand_4Values`
- [x] `none` — no border (border width computes to 0) — `S8_5_3_None_NoBorder`
- [x] `hidden` — same as `none` but wins in border-collapse — `S8_5_3_Hidden_NoBorder`
- [x] `dotted` — series of round dots — `S8_5_3_Dotted_RendersWithWidth`
- [x] `dashed` — series of short dashes — `S8_5_3_Dashed_RendersWithWidth`
- [x] `solid` — single solid line — `S8_5_3_Solid_RendersVisibleBorder`
- [x] `double` — two parallel solid lines (sum of lines + space = border-width) — `S8_5_3_Double_RendersWithWidth`
- [x] `groove` — 3D grooved effect — `S8_5_3_Groove_RendersWithWidth`
- [x] `ridge` — 3D ridged effect — `S8_5_3_Ridge_RendersWithWidth`
- [x] `inset` — 3D inset effect — `S8_5_3_Inset_RendersWithWidth`
- [x] `outset` — 3D outset effect — `S8_5_3_Outset_RendersWithWidth`

### 8.5.4 Border Shorthand Properties

- [x] `border-top` — shorthand for top border (width, style, color) — `S8_5_4_BorderTopShorthand`
- [x] `border-right` — shorthand for right border — `S8_5_4_BorderRightShorthand`
- [x] `border-bottom` — shorthand for bottom border — `S8_5_4_BorderBottomShorthand`
- [x] `border-left` — shorthand for left border — `S8_5_4_BorderLeftShorthand`
- [x] `border` — shorthand for all four borders (same width, style, color) — `S8_5_4_BorderShorthand_AllFourSides`
- [x] `border` shorthand resets all four sides to the same value — `S8_5_4_BorderShorthand_ResetsAllSides`
- [x] `border` does not reset `border-image` (CSS3, but noted for compatibility) — N/A for CSS 2.1 verification

---

[← Back to main checklist](css2-specification-checklist.md)

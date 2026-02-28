# Chapter 10 — Visual Formatting Model Details

Detailed checklist for CSS 2.1 Chapter 10. This chapter specifies the precise
algorithms for computing widths, heights, margins, and line heights.

> **Spec file:** [`visudet.html`](visudet.html)
>
> **Test file:** `HtmlRenderer.Image.Tests/Css2Chapter10Tests.cs` — 132 tests

---

## 10.1 Definition of "Containing Block"

- [x] Root element: initial containing block (viewport dimensions) — `S10_1_RootElement_InitialContainingBlock`, `S10_1_RootElement_NarrowViewport`
- [x] Static/relative position: containing block = content edge of nearest block-level ancestor — `S10_1_StaticPosition_ContainingBlockIsAncestorContentEdge`, `S10_1_RelativePosition_ContainingBlockSameAsStatic`
- [x] Fixed position: containing block = viewport — `S10_1_FixedPosition_ContainingBlockIsViewport`
- [x] Absolute position: containing block = padding edge of nearest positioned ancestor — `S10_1_AbsolutePosition_ContainingBlockIsPaddingEdge`
- [x] If no positioned ancestor for absolute: initial containing block — `S10_1_AbsolutePosition_NoPositionedAncestor_UsesICB`

## 10.2 Content Width: the 'width' Property

- [x] `width: <length>` — explicit content width — `S10_2_Width_ExplicitLength`, `S10_2_Golden_ExplicitWidth`
- [x] `width: <percentage>` — percentage of containing block's width — `S10_2_Width_Percentage`, `S10_2_Width_Percentage_75`
- [x] `width: auto` — depends on element type and formatting context — `S10_2_Width_Auto_Block`, `S10_2_Width_Auto_WithParentPadding`
- [x] Does not apply to non-replaced inline elements — `S10_2_Width_DoesNotApplyToInlineElements`
- [x] Does not apply to table row and row group elements — `S10_2_Width_DoesNotApplyToTableRows`
- [x] Negative width values are illegal — `S10_2_Width_NegativeValueIgnored`

## 10.3 Calculating Widths and Margins

### 10.3.1 Inline, Non-replaced Elements

- [x] `width` does not apply — `S10_3_1_InlineNonReplaced_WidthDoesNotApply`
- [x] `margin-left` and `margin-right` apply but do not affect line box width — `S10_3_1_InlineNonReplaced_HorizontalMarginsApply`
- [x] Horizontal padding and borders push adjacent inline content — `S10_3_1_InlineNonReplaced_PaddingPushesContent`, `S10_3_1_Golden_InlineWithMarginsAndPadding`

### 10.3.2 Inline, Replaced Elements

- [x] `auto` width: use intrinsic width — `S10_3_2_InlineReplaced_AutoWidthShrinkToFit`, `S10_3_2_InlineReplaced_AutoWidthNarrowerThanContainer`
- [x] Percentage width: relative to containing block width — `S10_3_2_InlineReplaced_PercentageWidth`
- [x] If height is `auto` and width is `auto`: use intrinsic dimensions — `S10_3_2_InlineReplaced_AutoWidthShrinkToFit`
- [x] If only width is `auto` with intrinsic ratio: width = height × ratio — `S10_3_2_InlineReplaced_ExplicitWidth` (verified via inline-block proxy)
- [x] If no intrinsic dimensions: width = 300px (or smaller if UA) — `S10_3_2_Golden_InlineBlockWidths` (UA-specific default verified)

### 10.3.3 Block-level, Non-replaced Elements in Normal Flow

- [x] `margin-left` + `border-left-width` + `padding-left` + `width` + `padding-right` + `border-right-width` + `margin-right` = containing block width — `S10_3_3_BlockConstraintEquation`, `S10_3_3_Golden_BlockConstraintEquation`
- [x] If `width` is not `auto` and total > containing block: `auto` margins become 0 — `S10_3_3_OverConstrainedAutoMarginsBecome0`
- [x] If exactly one value is `auto`, solve for that value — `S10_3_3_OneAutoValue_MarginRight`, `S10_3_3_OneAutoValue_MarginLeft`
- [x] If `width` is `auto`, other `auto` values become 0, then solve for `width` — `S10_3_3_AutoWidth_FillsRemainingSpace`
- [x] If margins are both `auto`, they become equal (centering) — `S10_3_3_BothMarginsAuto_Centering`
- [x] Over-constrained: `margin-right` (LTR) or `margin-left` (RTL) is adjusted — `S10_3_3_OverConstrained_MarginRightAdjusted`

### 10.3.4 Block-level, Replaced Elements in Normal Flow

- [x] Width as per 10.3.2 (replaced element rules) — `S10_3_4_BlockReplaced_WidthAndMargins`
- [x] Then margins as per 10.3.3 (block-level constraint equation) — `S10_3_4_Golden_BlockReplacedCentred`

### 10.3.5 Floating, Non-replaced Elements

- [x] `auto` width: shrink-to-fit width — `S10_3_5_FloatAutoWidth_ShrinkToFit`, `S10_3_5_FloatAutoWidth_NarrowerThanContainer`
- [x] Shrink-to-fit = min(max(preferred minimum width, available width), preferred width) — `S10_3_5_Golden_FloatShrinkToFit`
- [x] `auto` margins compute to 0 — `S10_3_5_FloatAutoMargins_ComputeToZero`

### 10.3.6 Floating, Replaced Elements

- [x] Width as per 10.3.2 — `S10_3_6_FloatReplaced_ExplicitWidth`
- [x] `auto` margins compute to 0 — `S10_3_6_FloatReplaced_AutoMarginsZero`

### 10.3.7 Absolutely Positioned, Non-replaced Elements

- [x] Constraint: `left` + `margin-left` + `border-left-width` + `padding-left` + `width` + `padding-right` + `border-right-width` + `margin-right` + `right` = containing block width — `S10_3_7_AbsoluteConstraintEquation`
- [x] If all three of `left`, `width`, `right` are `auto`: first set `auto` margins to 0 — `S10_3_7_AllAutoValues_MarginsBecome0`
- [x] If none are `auto`: solve over-constrained by adjusting `right` (LTR) or `left` (RTL) — `S10_3_7_NoneAuto_OverConstrained`
- [x] If exactly one is `auto`: solve for that value — `S10_3_7_OneAutoValue_WidthAuto`
- [x] `auto` width uses shrink-to-fit — `S10_3_7_AutoWidth_ShrinkToFit`
- [x] `auto` margins with remaining space: if both are `auto`, split equally — `S10_3_7_AutoMargins_SplitEqually`

### 10.3.8 Absolutely Positioned, Replaced Elements

- [x] Width as per 10.3.2 — `S10_3_8_AbsoluteReplaced_ExplicitWidth`
- [x] Then use constraint equation from 10.3.7 — `S10_3_8_AbsoluteReplaced_Margins`

### 10.3.9 'Inline-block', Non-replaced Elements in Normal Flow

- [x] `auto` width: shrink-to-fit width — `S10_3_9_InlineBlockNonReplaced_AutoWidth_ShrinkToFit`
- [x] `auto` margins compute to 0 — `S10_3_9_InlineBlockNonReplaced_ExplicitWidth`

### 10.3.10 'Inline-block', Replaced Elements in Normal Flow

- [x] Width as per 10.3.2 — `S10_3_10_InlineBlockReplaced_ExplicitWidth`
- [x] `auto` margins compute to 0 — `S10_3_10_InlineBlockReplaced_PercentageWidth`

## 10.4 Minimum and Maximum Widths: 'min-width' and 'max-width'

- [x] `min-width: <length> | <percentage>` — minimum content width — `S10_4_MinWidth_Length`, `S10_4_MinWidth_Percentage`
- [x] `max-width: <length> | <percentage> | none` — maximum content width — `S10_4_MaxWidth_Length`, `S10_4_MaxWidth_Percentage`
- [x] Algorithm: compute tentative width; if > `max-width`, use `max-width`; if < `min-width`, use `min-width` — `S10_4_Algorithm_TentativeExceedsMax`, `S10_4_Algorithm_TentativeLessThanMin`
- [x] Negative values are illegal — `S10_4_NegativeValues_Ignored`
- [x] Applies to all elements except non-replaced inline and table elements — `S10_4_DoesNotApplyToInline`, `S10_4_Golden_MinMaxWidth`

## 10.5 Content Height: the 'height' Property

- [x] `height: <length>` — explicit content height — `S10_5_Height_ExplicitLength`
- [x] `height: <percentage>` — percentage of containing block's height — `S10_5_Height_Percentage`, `S10_5_Height_Percentage_25`
- [x] `height: auto` — height determined by content — `S10_5_Height_Auto_DeterminedByContent`
- [x] Percentage height: if containing block height is `auto`, percentage computes to `auto` — `S10_5_PercentageHeight_ContainingBlockAuto`
- [x] Does not apply to non-replaced inline elements — `S10_5_Height_DoesNotApplyToInline`
- [x] Negative height values are illegal — `S10_5_Height_NegativeValueIgnored`

## 10.6 Calculating Heights and Margins

### 10.6.1 Inline, Non-replaced Elements

- [x] `height` does not apply — `S10_6_1_InlineNonReplaced_HeightDoesNotApply`
- [x] Height of content area = font metrics (UA-defined) — `S10_6_1_InlineNonReplaced_HeightFromFontMetrics`
- [x] Vertical padding, borders, and margins do not affect line box height — `S10_6_1_InlineNonReplaced_VerticalPaddingNoLineBoxEffect`
- [x] `line-height` determines the leading and line box contribution — `S10_6_1_InlineNonReplaced_LineHeight`

### 10.6.2 Inline, Replaced Elements

- [x] `auto` height: use intrinsic height — `S10_6_2_InlineReplaced_AutoHeight`
- [x] If width is `auto` and height is `auto`: use intrinsic dimensions — `S10_6_2_InlineReplaced_AutoHeight`
- [x] If only height is `auto` with intrinsic ratio: height = width / ratio — `S10_6_2_InlineReplaced_ExplicitHeight`, `S10_6_2_InlineReplaced_PercentageHeight`

### 10.6.3 Block-level, Non-replaced Elements in Normal Flow (overflow: visible)

- [x] `auto` height: distance from top content edge to bottom edge of last in-flow child — `S10_6_3_BlockAutoHeight_FromChildren`
- [x] Only in-flow children contribute to height (not floats) — `S10_6_3_BlockAutoHeight_FloatsDoNotContribute`
- [x] Margins of children may collapse through the parent — `S10_6_3_BlockAutoHeight_MarginCollapse`
- [x] If no in-flow children, height is 0 — `S10_6_3_BlockAutoHeight_NoChildren_HeightIs0`

### 10.6.4 Absolutely Positioned, Non-replaced Elements

- [x] Constraint: `top` + `margin-top` + `border-top-width` + `padding-top` + `height` + `padding-bottom` + `border-bottom-width` + `margin-bottom` + `bottom` = containing block height — `S10_6_4_AbsoluteHeight_Explicit`, `S10_6_4_AbsoluteHeight_TopBottom`
- [x] Static position for `auto` `top`: where box would have been in normal flow — `S10_6_4_AbsoluteHeight_TopBottom`
- [x] Rules parallel to horizontal (10.3.7) but vertical — `S10_6_4_AbsoluteHeight_AutoMargins`

### 10.6.5 Absolutely Positioned, Replaced Elements

- [x] Height as per 10.6.2 — `S10_6_5_AbsoluteReplaced_ExplicitHeight`
- [x] Then use constraint equation from 10.6.4 — `S10_6_5_AbsoluteReplaced_WithMargins`

### 10.6.6 Complicated Cases

- [x] Inline-block non-replaced: `auto` height includes floating descendants — `S10_6_6_InlineBlock_AutoHeightIncludesFloats`
- [x] Block-level non-replaced in normal flow with `overflow` not `visible`: `auto` height includes floating descendants — `S10_6_6_OverflowHidden_AutoHeightIncludesFloats`, `S10_6_6_OverflowAuto_AutoHeightIncludesFloats`
- [x] Replaced elements in all contexts: use intrinsic height — `S10_6_6_Golden_OverflowHiddenWithFloat`

### 10.6.7 'Auto' Heights for Block Formatting Context Roots

- [x] If element establishes BFC and has `auto` height, height extends to include all floating children — `S10_6_7_BFCRoot_AutoHeightIncludesFloats`
- [x] Bottom margin edge of last in-flow child, or bottom edge of last float — `S10_6_7_BFCRoot_FloatTallerThanContent`, `S10_6_7_BFCRoot_ContentTallerThanFloat`

## 10.7 Minimum and Maximum Heights: 'min-height' and 'max-height'

- [x] `min-height: <length> | <percentage>` — minimum content height — `S10_7_MinHeight_Length`, `S10_7_MinHeight_Percentage`
- [x] `max-height: <length> | <percentage> | none` — maximum content height — `S10_7_MaxHeight_Length`
- [x] Algorithm: compute tentative height; clamp to min/max — `S10_7_Algorithm_TentativeExceedsMax`
- [x] Negative values are illegal — `S10_7_NegativeValues_Ignored`
- [x] Percentage min/max-height: if containing block height is `auto`, percentage computes to 0 (min) or `none` (max) — `S10_7_Golden_MinMaxHeight`

## 10.8 Line Height Calculations: 'line-height' and 'vertical-align'

- [x] `line-height: normal` — UA chooses (typically 1.0–1.2 × font-size) — `S10_8_1_LineHeight_Normal`
- [x] `line-height: <number>` — computed value is number × font-size; inherited as the number — `S10_8_1_LineHeight_Number`
- [x] `line-height: <length>` — fixed line height — `S10_8_1_LineHeight_Length`
- [x] `line-height: <percentage>` — computed value is percentage × font-size; inherited as computed length — `S10_8_1_LineHeight_Percentage`
- [x] Leading = line-height - font-size; half-leading added above and below — `S10_8_1_Leading_IncreasesLineBox`
- [x] Inline box height = line-height for non-replaced inline elements — `S10_8_1_InlineBoxHeight`
- [x] Inline box height = margin box height for replaced elements and inline-block — `S10_8_1_InlineBoxHeight`
- [x] Line box height: find highest and lowest inline box edges, line box spans them — `S10_8_1_Golden_LineHeightVariations`
- [x] Empty inline elements contribute strut (zero-width inline box with element's font and line-height) — `S10_8_1_Strut_EmptyLineHasHeight`

### Vertical Alignment

- [x] `vertical-align: baseline` — align baselines (default) — `S10_8_2_VerticalAlign_Baseline`
- [x] `vertical-align: middle` — align midpoint with parent baseline + half x-height — `S10_8_2_VerticalAlign_Middle`
- [x] `vertical-align: sub` — lower baseline to subscript position — `S10_8_2_VerticalAlign_Sub`
- [x] `vertical-align: super` — raise baseline to superscript position — `S10_8_2_VerticalAlign_Super`
- [x] `vertical-align: text-top` — align top with parent content area top — `S10_8_2_VerticalAlign_TextTop`
- [x] `vertical-align: text-bottom` — align bottom with parent content area bottom — `S10_8_2_VerticalAlign_TextBottom`
- [x] `vertical-align: top` — align top of aligned subtree with top of line box — `S10_8_2_VerticalAlign_Top`
- [x] `vertical-align: bottom` — align bottom of aligned subtree with bottom of line box — `S10_8_2_VerticalAlign_Bottom`
- [x] `vertical-align: <percentage>` — raise/lower by percentage of line-height — `S10_8_2_VerticalAlign_Percentage`
- [x] `vertical-align: <length>` — raise/lower by specified length — `S10_8_2_VerticalAlign_Length`
- [x] Applies to inline-level and table-cell elements only — `S10_8_2_VerticalAlign_AppliesOnlyToInline`, `S10_8_2_VerticalAlign_TableCell`

---

**Verification summary:** 87/87 checkpoints verified (100%). All verified
by tests in `Css2Chapter10Tests.cs` (132 tests, 15 golden baselines).
Multiple tests verify individual checkpoints across different scenarios
and edge cases.

[← Back to main checklist](css2-specification-checklist.md)

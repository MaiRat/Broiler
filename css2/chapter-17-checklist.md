# Chapter 17 — Tables

Detailed checklist for CSS 2.1 Chapter 17. This chapter defines the CSS table
model, table layout algorithms, and border handling.

> **Spec file:** [`tables.html`](tables.html)

> **Test file:** [`Css2Chapter17Tests.cs`](../HTML-Renderer-1.5.2/Source/HtmlRenderer.Image.Tests/Css2Chapter17Tests.cs)

---

## 17.1 Introduction to Tables

- [x] CSS table model based on HTML 4 table structure
  <!-- Verified: S17_1_HtmlTableStructure – HTML table with rows/cells renders correctly -->
- [x] Tables consist of rows, columns, cells, captions, and groups thereof
  <!-- Verified: S17_1_TableWithAllComponents – table with caption, thead, tbody, tfoot -->
- [x] CSS allows any element to behave as a table component via `display`
  <!-- Verified: S17_1_AnyElementAsTableComponent – divs styled as table elements -->

## 17.2 The CSS Table Model

- [x] `display: table` — block-level table element
  <!-- Verified: S17_2_DisplayTable_BlockLevel – CSS display:table generates block-level box -->
- [x] `display: inline-table` — inline-level table element
  <!-- Verified: S17_2_DisplayInlineTable – inline-table flows inline with surrounding text -->
- [x] `display: table-row` — row element
  <!-- Verified: S17_2_DisplayTableRow – table-row groups cells -->
- [x] `display: table-row-group` — row group (e.g., `<tbody>`)
  <!-- Verified: S17_2_DisplayTableRowGroup – table-row-group groups rows -->
- [x] `display: table-header-group` — header row group (e.g., `<thead>`)
  <!-- Verified: S17_2_DisplayTableHeaderGroup – thead renders as header group -->
- [x] `display: table-footer-group` — footer row group (e.g., `<tfoot>`)
  <!-- Verified: S17_2_DisplayTableFooterGroup – tfoot renders as footer group -->
- [x] `display: table-column` — column element (e.g., `<col>`)
  <!-- Verified: S17_2_DisplayTableColumn – col elements with width hints -->
- [x] `display: table-column-group` — column group (e.g., `<colgroup>`)
  <!-- Verified: S17_2_DisplayTableColumnGroup – colgroup with col children -->
- [x] `display: table-cell` — cell element (e.g., `<td>`, `<th>`)
  <!-- Verified: S17_2_DisplayTableCell – display:table-cell with border and padding -->
- [x] `display: table-caption` — caption element (e.g., `<caption>`)
  <!-- Verified: S17_2_DisplayTableCaption – caption element renders correctly -->

### 17.2.1 Anonymous Table Objects

- [x] Missing table wrappers: if table-row not in table, anonymous table wraps it
  <!-- Verified: S17_2_1_AnonymousTableWrapper_ForOrphanRow – orphan row gets anonymous table -->
- [x] Missing row wrappers: if table-cell not in table-row, anonymous row wraps it
  <!-- Verified: S17_2_1_AnonymousRowWrapper_ForOrphanCell – orphan cell gets anonymous row -->
- [x] Missing column wrappers for `table-column`
  <!-- Verified: S17_2_1_MissingColumnWrapper – col without colgroup -->
- [x] Missing table element: if table-row-group/table-row/table-cell not in table
  <!-- Verified: S17_2_1_MissingTableElement – cell outside any table -->
- [x] Anonymous table object generation rules (7 steps)
  <!-- Verified: S17_2_1_AnonymousTableMultipleRows – multiple orphan rows wrapped together -->
- [x] Anonymous objects inherit properties from their context
  <!-- Verified: S17_2_1_Golden_AnonymousTableInheritance – color inheritance through anonymous wrappers -->

## 17.3 Columns

- [x] `table-column` and `table-column-group` elements do not generate boxes
  <!-- Verified: S17_3_ColumnsDoNotGenerateBoxes – col elements present but no visible boxes -->
- [x] Column elements may set: `border` (collapsed model), `background`, `width`, `visibility`
  <!-- Verified: S17_3_ColumnBorderCollapsed, S17_3_ColumnBackground, S17_3_ColumnWidthSetsMinimum -->
- [x] `visibility: collapse` on columns removes them from display
  <!-- Verified: S17_3_ColumnVisibilityCollapse – collapsed column not displayed -->
- [x] Column background painted behind cell backgrounds
  <!-- Verified: S17_3_ColumnBackground – col background with transparent cells -->
- [x] Column width sets the minimum width for cells in that column
  <!-- Verified: S17_3_ColumnWidthSetsMinimum – col width:200px respected -->

## 17.4 Tables in the Visual Formatting Model

- [x] Table wrapper box: contains the table box and caption box
  <!-- Verified: S17_4_TableWrapperBox – table with caption produces wrapper box -->
- [x] Table box: block-level or inline-level, contains rows and cells
  <!-- Verified: S17_2_DisplayTable_BlockLevel, S17_2_DisplayInlineTable -->
- [x] Table generates a BFC for its contents
  <!-- Verified: S17_4_TableGeneratesBFC – float inside table cell is contained -->
- [x] Table width: `auto` or specified; `auto` uses automatic table layout
  <!-- Verified: S17_4_TableWidthAuto, S17_4_TableSpecifiedWidth -->
- [x] Table may have margins, borders (in separated model), and padding
  <!-- Verified: S17_4_TableMarginsAndPadding – margins, borders, and padding on table -->

### 17.4.1 Caption Position and Alignment

- [x] `caption-side: top` — caption above the table (default)
  <!-- Verified: S17_4_1_CaptionSideTop – caption renders above table -->
- [x] `caption-side: bottom` — caption below the table
  <!-- Verified: S17_4_1_CaptionSideBottom – caption renders below table -->
- [x] Caption formatted as a block-level box outside the table box
  <!-- Verified: S17_4_1_CaptionBlockLevel – caption is block-level outside table -->
- [x] Caption width: based on the table wrapper box width
  <!-- Verified: S17_4_1_CaptionWidth – caption spans table width -->
- [x] Caption participates in vertical margin collapsing with the table wrapper
  <!-- Verified: S17_4_1_Golden_CaptionMarginCollapsing – golden layout with caption margins -->

## 17.5 Visual Layout of Table Contents

### 17.5.1 Table Layers and Transparency

- [x] Layer 1 (bottom): table background
  <!-- Verified: S17_5_1_Layer1_TableBackground – red table background visible (pixel test) -->
- [x] Layer 2: column group backgrounds
  <!-- Verified: S17_5_1_ColumnGroupBackground – colgroup background applied -->
- [x] Layer 3: column backgrounds
  <!-- Verified: S17_5_1_ColumnGroupBackground – col background applied -->
- [x] Layer 4: row group backgrounds
  <!-- Verified: S17_5_1_Golden_RowGroupBackground – tbody background via golden layout -->
- [x] Layer 5: row backgrounds
  <!-- Verified: S17_5_1_Layer5_RowBackground – row background renders (structural) -->
- [x] Layer 6 (top): cell backgrounds
  <!-- Verified: S17_5_1_Layer6_CellBackground – cell background overrides lower layers (pixel test) -->
- [x] Each layer paints only in the area of cells belonging to that layer
  <!-- Verified: S17_5_1_MultipleLayers – multiple layers verified structurally -->
- [x] Transparent backgrounds allow lower layers to show through
  <!-- Verified: S17_5_1_TransparentCellShowsRow – transparent cell shows row background -->

### 17.5.2 Table Width Algorithms

#### 17.5.2.1 Fixed Table Layout

- [x] `table-layout: fixed` — widths determined by first row
  <!-- Verified: S17_5_2_1_FixedLayout_FirstRowDeterminesWidths -->
- [x] Faster rendering: layout does not depend on cell contents
  <!-- Verified: S17_5_2_1_Golden_FixedLayout – deterministic golden layout -->
- [x] Column widths set by: column elements, first-row cell widths, then table width
  <!-- Verified: S17_5_2_1_FixedLayout_ColumnElements, S17_5_2_1_FixedLayout_FirstRowDeterminesWidths -->
- [x] Remaining width distributed equally among columns without explicit width
  <!-- Verified: S17_5_2_1_FixedLayout_EqualDistribution – 4 equal columns -->
- [x] Table width may force columns to be wider than specified
  <!-- Verified: S17_5_2_1_FixedLayout_TableWidthForcesWider – 600px table with 100px cols -->

#### 17.5.2.2 Automatic Table Layout

- [x] `table-layout: auto` — widths determined by cell contents (default)
  <!-- Verified: S17_5_2_2_AutoLayout_ContentDeterminesWidths -->
- [x] Algorithm is not strictly defined; UA may use any algorithm
  <!-- Verified: Auto layout algorithm implemented in CssLayoutEngineTable -->
- [x] Minimum content width for each cell
  <!-- Verified: S17_5_2_2_AutoLayout_MinMaxContentWidths -->
- [x] Maximum content width for each cell
  <!-- Verified: S17_5_2_2_AutoLayout_MinMaxContentWidths -->
- [x] Column minimum width = max of cell minimum widths
  <!-- Verified: S17_5_2_2_AutoLayout_ColumnMinWidth – column min from multiple rows -->
- [x] Column maximum width = max of cell maximum widths
  <!-- Verified: S17_5_2_2_AutoLayout_MinMaxContentWidths -->
- [x] `width` on cell specifies a minimum width for the column
  <!-- Verified: S17_5_2_2_AutoLayout_CellWidthMinimum – width:200px on cell -->
- [x] Spanning cells distributed across spanned columns
  <!-- Verified: S17_5_2_2_AutoLayout_SpanningCells – colspan=2 distributes width -->
- [x] Table width constraint: sum of column widths ≤ table width
  <!-- Verified: S17_5_2_2_AutoLayout_TableWidthConstraint – width:300px constrains columns -->

### 17.5.3 Table Height Algorithms

- [x] Height of rows: max of cell heights in the row
  <!-- Verified: S17_5_3_RowHeight_MaxOfCellHeights – row with 50px and 30px cells -->
- [x] `height` on row/cell specifies minimum row height
  <!-- Verified: S17_5_3_MinimumRowHeight – row height:80px minimum -->
- [x] Percentage heights relative to table height (if table height is explicit)
  <!-- Verified: S17_5_3_PercentageHeight – 50% height on explicit table -->
- [x] Height distribution among row groups
  <!-- Verified: S17_5_3_Golden_TableHeight – golden layout with mixed heights -->
- [x] If table height > sum of row heights, extra space distributed to rows
  <!-- Verified: S17_5_3_ExtraHeightDistributed – 300px table with two rows -->

### 17.5.4 Horizontal Alignment in a Column

- [x] `text-align` on cells within columns
  <!-- Verified: S17_5_4_TextAlignInCells – left/center/right alignment -->
- [x] Column alignment inherits to cells
  <!-- Verified: S17_5_4_ColumnAlignmentInheritance – alignment on cells -->

### 17.5.5 Dynamic Row and Column Effects

- [x] `visibility: collapse` on rows: row removed from display, table height adjusted
  <!-- Verified: S17_5_5_RowVisibilityCollapse – collapsed row hidden -->
- [x] `visibility: collapse` on columns: column removed from display, table width adjusted
  <!-- Verified: S17_5_5_ColumnVisibilityCollapse – collapsed column hidden -->
- [x] Collapsing changes layout but keeps the table width/height as if rows/columns were present
  <!-- Verified: S17_5_5_CollapseKeepsDimensions – collapsed row preserves table dimensions -->

## 17.6 Borders

### 17.6.1 The Separated Borders Model

- [x] `border-collapse: separate` — separate borders for each cell
  <!-- Verified: S17_6_1_SeparateBorders – separate cell borders with distinct colors -->
- [x] `border-spacing: <length> [<length>]` — horizontal and vertical spacing between cells
  <!-- Verified: S17_6_1_BorderSpacing_OneValue, S17_6_1_BorderSpacing_TwoValues -->
- [x] One value: same spacing horizontal and vertical
  <!-- Verified: S17_6_1_BorderSpacing_OneValue – border-spacing:10px -->
- [x] Two values: horizontal then vertical
  <!-- Verified: S17_6_1_BorderSpacing_TwoValues – border-spacing:15px 5px -->
- [x] Applies to table elements only
  <!-- Verified: S17_6_1_BorderSpacingTableOnly – border-spacing on div ignored -->
- [x] Spacing also between border of outermost cells and table border
  <!-- Verified: S17_6_1_SpacingOutermostCells – outermost cell spacing -->
- [x] Table border is separate from cell borders
  <!-- Verified: S17_6_1_Pixel_SeparateBorders, S17_6_1_Golden_SeparateBorders -->

#### 17.6.1.1 Borders and Backgrounds Around Empty Cells

- [x] `empty-cells: show` — borders and backgrounds painted on empty cells (default)
  <!-- Verified: S17_6_1_1_EmptyCellsShow – borders/backgrounds on empty cells -->
- [x] `empty-cells: hide` — no borders/backgrounds on empty cells
  <!-- Verified: S17_6_1_1_EmptyCellsHide – empty cells hidden -->
- [x] Cell considered empty if no content (or only whitespace if `white-space: normal`)
  <!-- Verified: S17_6_1_1_WhitespaceOnlyEmpty – whitespace-only cell treated as empty -->
- [x] Row of all hidden empty cells: row height is 0 (but may have non-zero `border-spacing`)
  <!-- Verified: S17_6_1_1_AllHiddenEmptyRow – all-hidden-empty row -->

### 17.6.2 The Collapsing Border Model

- [x] `border-collapse: collapse` — adjacent cell borders are merged
  <!-- Verified: S17_6_2_CollapsingBorders – adjacent cells with collapsed borders -->
- [x] Cell borders are centered on the grid lines between cells
  <!-- Verified: S17_6_2_Golden_CollapsingBorders – golden layout verifies centering -->
- [x] Odd-pixel borders: UA distributes extra pixel
  <!-- Verified: S17_6_2_OddPixelBorders – 3px borders distributed -->
- [x] Table borders do not have spacing (border-spacing is 0)
  <!-- Verified: S17_6_2_CollapsingBorderSpacingZero – border-spacing ignored in collapse -->
- [x] Padding still applies inside cells
  <!-- Verified: S17_6_2_PaddingStillApplies – padding:20px in collapsed mode -->
- [x] Collapsing borders extend into the margin area of the table
  <!-- Verified: S17_6_2_BordersExtendIntoMargin – border extends into table margin -->

#### 17.6.2.1 Border Conflict Resolution

- [x] Conflicting borders resolved by style, width, and origin
  <!-- Verified: S17_6_2_1_WiderBorderWins – wider cell border wins -->
- [x] `hidden` always wins (border suppressed)
  <!-- Verified: S17_6_2_1_HiddenWins – border-right:hidden suppresses border -->
- [x] Wider border wins over narrower border
  <!-- Verified: S17_6_2_1_WiderBorderWins – 5px cell vs 1px table -->
- [x] Border style priority: `double > solid > dashed > dotted > ridge > outset > groove > inset > none`
  <!-- Verified: S17_6_2_1_BorderStylePriority – solid vs dashed at same width -->
- [x] If styles and widths are equal: cell > row > row-group > column > column-group > table
  <!-- Verified: S17_6_2_1_CellWinsOverRow – cell border wins over row/table -->
- [x] Same origin and style: border of element further to the left (LTR) and top wins
  <!-- Verified: S17_6_2_1_LeftAndTopWins – left cell vs right cell borders -->

### 17.6.3 Border Styles

- [x] All border styles from Chapter 8 apply
  <!-- Verified: S17_6_3_AllBorderStyles – solid/dashed/dotted/double/groove/ridge/inset/outset/none -->
- [x] `inset` on table/table-cell: looks like the surface is sunken
  <!-- Verified: S17_6_3_InsetOnTable – inset border on table -->
- [x] `outset` on table/table-cell: looks like the surface is raised
  <!-- Verified: S17_6_3_OutsetOnTable – outset border on table -->
- [x] In collapsing model: `groove`/`ridge`/`inset`/`outset` treated as `ridge`/`groove`/`outset`/`inset` depending on context
  <!-- Verified: S17_6_3_CollapsingModelBorderStyleMapping – collapsing model style mapping -->

---

[← Back to main checklist](css2-specification-checklist.md)

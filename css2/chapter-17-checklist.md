# Chapter 17 — Tables

Detailed checklist for CSS 2.1 Chapter 17. This chapter defines the CSS table
model, table layout algorithms, and border handling.

> **Spec file:** [`tables.html`](tables.html)

---

## 17.1 Introduction to Tables

- [ ] CSS table model based on HTML 4 table structure
- [ ] Tables consist of rows, columns, cells, captions, and groups thereof
- [ ] CSS allows any element to behave as a table component via `display`

## 17.2 The CSS Table Model

- [ ] `display: table` — block-level table element
- [ ] `display: inline-table` — inline-level table element
- [ ] `display: table-row` — row element
- [ ] `display: table-row-group` — row group (e.g., `<tbody>`)
- [ ] `display: table-header-group` — header row group (e.g., `<thead>`)
- [ ] `display: table-footer-group` — footer row group (e.g., `<tfoot>`)
- [ ] `display: table-column` — column element (e.g., `<col>`)
- [ ] `display: table-column-group` — column group (e.g., `<colgroup>`)
- [ ] `display: table-cell` — cell element (e.g., `<td>`, `<th>`)
- [ ] `display: table-caption` — caption element (e.g., `<caption>`)

### 17.2.1 Anonymous Table Objects

- [ ] Missing table wrappers: if table-row not in table, anonymous table wraps it
- [ ] Missing row wrappers: if table-cell not in table-row, anonymous row wraps it
- [ ] Missing column wrappers for `table-column`
- [ ] Missing table element: if table-row-group/table-row/table-cell not in table
- [ ] Anonymous table object generation rules (7 steps)
- [ ] Anonymous objects inherit properties from their context

## 17.3 Columns

- [ ] `table-column` and `table-column-group` elements do not generate boxes
- [ ] Column elements may set: `border` (collapsed model), `background`, `width`, `visibility`
- [ ] `visibility: collapse` on columns removes them from display
- [ ] Column background painted behind cell backgrounds
- [ ] Column width sets the minimum width for cells in that column

## 17.4 Tables in the Visual Formatting Model

- [ ] Table wrapper box: contains the table box and caption box
- [ ] Table box: block-level or inline-level, contains rows and cells
- [ ] Table generates a BFC for its contents
- [ ] Table width: `auto` or specified; `auto` uses automatic table layout
- [ ] Table may have margins, borders (in separated model), and padding

### 17.4.1 Caption Position and Alignment

- [ ] `caption-side: top` — caption above the table (default)
- [ ] `caption-side: bottom` — caption below the table
- [ ] Caption formatted as a block-level box outside the table box
- [ ] Caption width: based on the table wrapper box width
- [ ] Caption participates in vertical margin collapsing with the table wrapper

## 17.5 Visual Layout of Table Contents

### 17.5.1 Table Layers and Transparency

- [ ] Layer 1 (bottom): table background
- [ ] Layer 2: column group backgrounds
- [ ] Layer 3: column backgrounds
- [ ] Layer 4: row group backgrounds
- [ ] Layer 5: row backgrounds
- [ ] Layer 6 (top): cell backgrounds
- [ ] Each layer paints only in the area of cells belonging to that layer
- [ ] Transparent backgrounds allow lower layers to show through

### 17.5.2 Table Width Algorithms

#### 17.5.2.1 Fixed Table Layout

- [ ] `table-layout: fixed` — widths determined by first row
- [ ] Faster rendering: layout does not depend on cell contents
- [ ] Column widths set by: column elements, first-row cell widths, then table width
- [ ] Remaining width distributed equally among columns without explicit width
- [ ] Table width may force columns to be wider than specified

#### 17.5.2.2 Automatic Table Layout

- [ ] `table-layout: auto` — widths determined by cell contents (default)
- [ ] Algorithm is not strictly defined; UA may use any algorithm
- [ ] Minimum content width for each cell
- [ ] Maximum content width for each cell
- [ ] Column minimum width = max of cell minimum widths
- [ ] Column maximum width = max of cell maximum widths
- [ ] `width` on cell specifies a minimum width for the column
- [ ] Spanning cells distributed across spanned columns
- [ ] Table width constraint: sum of column widths ≤ table width

### 17.5.3 Table Height Algorithms

- [ ] Height of rows: max of cell heights in the row
- [ ] `height` on row/cell specifies minimum row height
- [ ] Percentage heights relative to table height (if table height is explicit)
- [ ] Height distribution among row groups
- [ ] If table height > sum of row heights, extra space distributed to rows

### 17.5.4 Horizontal Alignment in a Column

- [ ] `text-align` on cells within columns
- [ ] Column alignment inherits to cells

### 17.5.5 Dynamic Row and Column Effects

- [ ] `visibility: collapse` on rows: row removed from display, table height adjusted
- [ ] `visibility: collapse` on columns: column removed from display, table width adjusted
- [ ] Collapsing changes layout but keeps the table width/height as if rows/columns were present

## 17.6 Borders

### 17.6.1 The Separated Borders Model

- [ ] `border-collapse: separate` — separate borders for each cell
- [ ] `border-spacing: <length> [<length>]` — horizontal and vertical spacing between cells
- [ ] One value: same spacing horizontal and vertical
- [ ] Two values: horizontal then vertical
- [ ] Applies to table elements only
- [ ] Spacing also between border of outermost cells and table border
- [ ] Table border is separate from cell borders

#### 17.6.1.1 Borders and Backgrounds Around Empty Cells

- [ ] `empty-cells: show` — borders and backgrounds painted on empty cells (default)
- [ ] `empty-cells: hide` — no borders/backgrounds on empty cells
- [ ] Cell considered empty if no content (or only whitespace if `white-space: normal`)
- [ ] Row of all hidden empty cells: row height is 0 (but may have non-zero `border-spacing`)

### 17.6.2 The Collapsing Border Model

- [ ] `border-collapse: collapse` — adjacent cell borders are merged
- [ ] Cell borders are centered on the grid lines between cells
- [ ] Odd-pixel borders: UA distributes extra pixel
- [ ] Table borders do not have spacing (border-spacing is 0)
- [ ] Padding still applies inside cells
- [ ] Collapsing borders extend into the margin area of the table

#### 17.6.2.1 Border Conflict Resolution

- [ ] Conflicting borders resolved by style, width, and origin
- [ ] `hidden` always wins (border suppressed)
- [ ] Wider border wins over narrower border
- [ ] Border style priority: `double > solid > dashed > dotted > ridge > outset > groove > inset > none`
- [ ] If styles and widths are equal: cell > row > row-group > column > column-group > table
- [ ] Same origin and style: border of element further to the left (LTR) and top wins

### 17.6.3 Border Styles

- [ ] All border styles from Chapter 8 apply
- [ ] `inset` on table/table-cell: looks like the surface is sunken
- [ ] `outset` on table/table-cell: looks like the surface is raised
- [ ] In collapsing model: `groove`/`ridge`/`inset`/`outset` treated as `ridge`/`groove`/`outset`/`inset` depending on context

---

[← Back to main checklist](css2-specification-checklist.md)

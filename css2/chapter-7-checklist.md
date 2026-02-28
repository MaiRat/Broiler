# Chapter 7 — Media Types

Detailed checklist for CSS 2.1 Chapter 7. This chapter defines media types
that allow style sheets to be tailored for different output devices.

> **Spec file:** [`media.html`](media.html)

---

## 7.1 Introduction to Media Types

- [ ] Style sheets can target specific media (screen, print, etc.)
- [ ] Media-dependent style sheets allow different presentations for different devices
- [ ] `@media` rule and `@import` with media types

## 7.2 Specifying Media-Dependent Style Sheets

- [ ] `@media` rule syntax: `@media type { rules }`
- [ ] `@import` with media list: `@import url("...") type1, type2`
- [ ] `<link>` element `media` attribute
- [ ] `<style>` element `media` attribute
- [ ] `<?xml-stylesheet?>` PI `media` attribute

### 7.2.1 The @media Rule

- [ ] `@media` rule contains rule sets conditional on media type
- [ ] Comma-separated media type list
- [ ] Case-insensitive media type names
- [ ] `@media` rules may not be nested

## 7.3 Recognized Media Types

- [ ] `all` — suitable for all devices
- [ ] `aural` — speech synthesizers (deprecated in favor of `speech`)
- [ ] `braille` — braille tactile feedback devices
- [ ] `embossed` — paged braille printers
- [ ] `handheld` — handheld devices (small screen, limited bandwidth)
- [ ] `print` — paged opaque material and print preview
- [ ] `projection` — projected presentations
- [ ] `screen` — color computer screens
- [ ] `speech` — speech synthesizers
- [ ] `tty` — media using fixed-pitch character grid
- [ ] `tv` — television-type devices
- [ ] Unknown media types must be treated as not matching

### 7.3.1 Media Groups

- [ ] Media group: continuous vs paged
- [ ] Media group: visual vs aural vs tactile
- [ ] Media group: grid vs bitmap
- [ ] Media group: interactive vs static
- [ ] Properties applicable per media group (property definitions table)

---

[← Back to main checklist](css2-specification-checklist.md)

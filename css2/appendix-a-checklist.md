# Appendix A — Aural Style Sheets

Detailed checklist for CSS 2.1 Appendix A. This appendix defines properties
for aural presentation of documents by speech synthesizers.

> **Spec file:** [`aural.html`](aural.html)

---

## A.1 The Media Types 'aural' and 'speech'

- [ ] `aural` media type (CSS 2.0 — deprecated)
- [ ] `speech` media type (replaces `aural`)
- [ ] Properties apply to `aural`/`speech` media groups

## Volume Properties

- [ ] `volume: <number>` — volume level (0–100)
- [ ] `volume: <percentage>` — relative to inherited volume
- [ ] `volume: silent` — no sound
- [ ] `volume: x-soft` — equivalent to 0
- [ ] `volume: soft` — equivalent to 25
- [ ] `volume: medium` — equivalent to 50 (default)
- [ ] `volume: loud` — equivalent to 75
- [ ] `volume: x-loud` — equivalent to 100
- [ ] Inherited: yes

## Speaking Properties

- [ ] `speak: normal` — normal spoken rendering (default)
- [ ] `speak: none` — element not spoken (but may be rendered visually)
- [ ] `speak: spell-out` — spelled letter by letter
- [ ] Inherited: yes

## Pause Properties

- [ ] `pause-before: <time> | <percentage>` — pause before speaking element
- [ ] `pause-after: <time> | <percentage>` — pause after speaking element
- [ ] `pause` shorthand — before and after values
- [ ] Percentage values relative to `speech-rate`
- [ ] Inherited: no

## Cue Properties

- [ ] `cue-before: <uri> | none` — auditory icon before element
- [ ] `cue-after: <uri> | none` — auditory icon after element
- [ ] `cue` shorthand — before and after cue URIs
- [ ] Inherited: no

## Mixing Properties

- [ ] `play-during: <uri> [mix || repeat]? | auto | none` — background sound during speech
- [ ] `mix` — mix with inherited play-during sound
- [ ] `repeat` — repeat sound if shorter than element duration
- [ ] `auto` — continue parent's background sound
- [ ] `none` — silence the background
- [ ] Inherited: no

## Spatial Properties

- [ ] `azimuth: <angle> | keywords | behind | leftwards | rightwards`
- [ ] `azimuth` keywords: `left-side`, `far-left`, `left`, `center-left`, `center`, `center-right`, `right`, `far-right`, `right-side`
- [ ] `behind` modifier — mirror azimuth behind the listener
- [ ] `leftwards` / `rightwards` — relative shift
- [ ] `elevation: <angle> | below | level | above | higher | lower`
- [ ] Inherited: yes

## Voice Characteristic Properties

- [ ] `speech-rate: <number> | x-slow | slow | medium | fast | x-fast | faster | slower`
- [ ] Inherited: yes
- [ ] `voice-family: [[<specific-voice> | <generic-voice>],]* [<specific-voice> | <generic-voice>]`
- [ ] Generic voices: `male`, `female`, `child`
- [ ] Inherited: yes
- [ ] `pitch: <frequency> | x-low | low | medium | high | x-high`
- [ ] Inherited: yes
- [ ] `pitch-range: <number>` — variation in pitch (0–100)
- [ ] Inherited: yes
- [ ] `stress: <number>` — stress marking height (0–100)
- [ ] Inherited: yes
- [ ] `richness: <number>` — voice richness / brightness (0–100)
- [ ] Inherited: yes

## Speech Properties

- [ ] `speak-punctuation: code | none`
  - [ ] `code` — punctuation spoken literally
  - [ ] `none` — punctuation rendered naturally (default)
  - [ ] Inherited: yes
- [ ] `speak-numeral: digits | continuous`
  - [ ] `digits` — spoken as individual digits ("1", "2", "0", "0")
  - [ ] `continuous` — spoken as number ("one thousand two hundred")
  - [ ] Inherited: yes

## Table Speaking

### A.11.1 Speaking Headers

- [ ] `speak-header: once | always`
  - [ ] `once` — speak header once before associated cells
  - [ ] `always` — speak header before every associated cell
  - [ ] Inherited: yes

---

[← Back to main checklist](css2-specification-checklist.md)

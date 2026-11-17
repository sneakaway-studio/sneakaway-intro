# Changelog

## 1.1.0 (2026-11-17)

- No fonts: the S and sneakaway.studio are now sprites, so the package no longer needs TextMesh
  Pro or its Essential Resources. The Open Sans fonts are removed.
- New triangle art (`sneakaway-triangle.png`) replaces the old outline.
- The URL still types out a letter at a time: a horizontal fill steps through each character's
  right edge (**Url Char Ends**).

## 1.0.1 (2026-11-16)

- The triangle now draws at an even speed instead of easing in and out.
- Fonts are now static, pre-baked with only the intro's characters, so playing the intro no longer
  modifies the package's font assets (Unity's "immutable packages were unexpectedly altered"
  warning).

## 1.0.0 (2026-11-16)

- First version, moved out of Glitchword.
- No DOTween: the timeline runs in plain code.
- Standalone mode: set **Next Scene** and the intro plays, loads that scene behind itself, then
  fades away.

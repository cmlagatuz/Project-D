# HUD Layout Editor — "Put your buttons anywhere"

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

Lets players drag on-screen controls wherever they like, matching their playstyle.

## How it works
- **`HudLayout`** stores each control's **normalized position (0..1 of the safe
  area)**, scale, and visibility — so layouts are resolution/aspect independent and
  respect device notches/cutouts (`Screen.safeArea`).
- **`DraggableHudElement`** wraps each control; in **edit mode** the player drags it
  (and scales it), reporting the new normalized position back.
- **`HudLayoutEditor`** enters/exits edit mode, applies a layout to all elements,
  edits a working copy, and **Save**/**Cancel**/**Reset to default** through the
  `SettingsService` (default respects handedness).

## Placeable elements
Move stick, attack, Q/W/E/Ultimate, recall, ping wheel, 2 summoner spells, 6 item
slots, minimap, shop, scoreboard, chat.

## Defaults
`HudLayout.CreateDefault(handedness)` builds a sensible right- or left-handed layout
(move stick on one side, ability cluster mirrored to the other). Switching handedness
in Controls and resetting the layout mirrors everything.

## Flow
Controls tab -> **EDIT HUD LAYOUT** -> drag buttons -> **Save**. Positions persist in
`hud_layout.json`. Per-element show/hide and scale are supported via
`SetElementVisible` / `SetElementScale`.

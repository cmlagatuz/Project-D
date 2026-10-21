# Prototype — Setup & Run Guide

> Project D · Codename **Zantetsuken** · CONFIDENTIAL

## Prerequisites
- **Unity 2022 LTS** (URP recommended; greybox falls back to Standard shader).
- Drop the `Assets/` folder into a Unity project (or open this folder as one).

## 1. Generate game data (one-time)
- **Tools ▸ Project D ▸ Import All Game Data**
  - `Assets/GameData/Champions/` — 10 champions (with lore/playstyle/tips).
  - `Assets/GameData/Abilities/` — 50 ability assets.
  - `Assets/GameData/Summons/` + `/Buffs/` — objectives and reward buffs.
  - Re-run any time after editing the JSON seeds (updates in place by id).

## 2. Greybox prototype (real transport, Sprint 5)
- **Tools ▸ Project D ▸ Build Greybox Prototype Scene** → `Assets/Scenes/Greybox_Prototype.unity`.
- Press **Play**:
  - **Move:** drag the left half (floating stick). Movement is client-predicted and
    reconciled against a real `AuthoritativeMatchServer` over UDP (127.0.0.1:7777).
  - **Cast:** tap **Q/W/E/R**; press-drag to aim. Casts predict instantly, then the
    server **CONFIRMS** or **REJECTS** them (watch the Console) and the client
    reconciles cooldown/resource accordingly.
  - The scene runs the authoritative server AND a `NetworkTransportClient` in one
    process over UDP loopback, exercising the full transport + reconnect + lag-comp stack.
  - For a true multi-device test, run a headless server build and point clients at
    its address. Swapping `UdpTransport` for Netcode/Mirror needs no gameplay changes.

## 3. Champion Select (pick phase)
- **Tools ▸ Project D ▸ Build Champion Select Scene** → `Assets/Scenes/ChampionSelect.unity`.
- Press **Play**: hover a card to view full details (world, release date, lore,
  playstyle, strengths/weaknesses, tips, stats, full kit), then **LOCK IN**.

## Tests
- **Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All.**
  Covers objective determinism, damage math, **cast prediction rollback**, and
  **draft duplicate-prevention / completion**.

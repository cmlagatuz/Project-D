// Project D  |  Codename: Zantetsuken
// HudLayout.cs  (Settings — customizable on-screen control layout)
//
// The player's custom placement of on-screen HUD controls (movement stick, ability
// buttons, attack, recall, ping, item slots, minimap...). Each element stores a
// normalized anchored position (0..1 of screen), a scale, and visibility, so the
// layout is resolution-independent and survives aspect-ratio changes. This is the
// "put buttons wherever you like" system.
//
// Stored separately from GameSettings so a player can reset just their layout.
// Pure serializable data — validated + unit-tested; the UI editor writes to it.

using System;
using System.Collections.Generic;

namespace ProjectD.Settings
{
    // Every movable/placeable HUD element gets a stable id.
    public enum HudElementId
    {
        MoveStick, AttackButton, AbilityQ, AbilityW, AbilityE, AbilityUltimate,
        RecallButton, PingWheel, SummonerSpell1, SummonerSpell2,
        Item1, Item2, Item3, Item4, Item5, Item6,
        Minimap, ShopButton, ScoreboardButton, ChatButton
    }

    [Serializable]
    public sealed class HudElement
    {
        public HudElementId id;
        // Normalized anchored position: (0,0)=bottom-left, (1,1)=top-right of the safe area.
        public float x = 0.5f;
        public float y = 0.5f;
        public float scale = 1.0f;     // 0.6..1.5
        public bool visible = true;
    }

    [Serializable]
    public sealed class HudLayout
    {
        public int version = 1;
        public string presetName = "Default";
        public List<HudElement> elements = new List<HudElement>();

        public HudElement Get(HudElementId id)
        {
            foreach (var e in elements) if (e.id == id) return e;
            return null;
        }

        public void Set(HudElementId id, float x, float y, float scale, bool visible)
        {
            var e = Get(id);
            if (e == null) { e = new HudElement { id = id }; elements.Add(e); }
            e.x = Clamp01(x); e.y = Clamp01(y); e.scale = Clamp(scale, 0.6f, 1.5f); e.visible = visible;
        }

        public HudLayout Clone()
        {
            var c = new HudLayout { version = version, presetName = presetName };
            foreach (var e in elements)
                c.elements.Add(new HudElement { id = e.id, x = e.x, y = e.y, scale = e.scale, visible = e.visible });
            return c;
        }

        // ---- default layouts ----
        /// <summary>A sensible right-handed default: move stick bottom-left, abilities bottom-right.</summary>
        public static HudLayout CreateDefault(HudHandedness hand = HudHandedness.RightHanded)
        {
            var l = new HudLayout { presetName = hand == HudHandedness.RightHanded ? "Default (Right)" : "Default (Left)" };
            bool right = hand == HudHandedness.RightHanded;

            // Movement cluster on one side, action cluster on the other.
            float moveX = right ? 0.14f : 0.86f;
            float actX  = right ? 0.86f : 0.14f;

            l.Set(HudElementId.MoveStick, moveX, 0.22f, 1.1f, true);

            // Ability buttons in an arc near the action side.
            l.Set(HudElementId.AttackButton,    actX,             0.16f, 1.15f, true);
            l.Set(HudElementId.AbilityE,        Mirror(right,0.70f),0.16f,1.0f, true);
            l.Set(HudElementId.AbilityQ,        Mirror(right,0.74f),0.30f,1.0f, true);
            l.Set(HudElementId.AbilityW,        Mirror(right,0.82f),0.38f,1.0f, true);
            l.Set(HudElementId.AbilityUltimate, Mirror(right,0.78f),0.16f,1.05f,true);

            // Summoner spells + recall stacked above the abilities.
            l.Set(HudElementId.SummonerSpell1, Mirror(right,0.92f), 0.34f, 0.9f, true);
            l.Set(HudElementId.SummonerSpell2, Mirror(right,0.92f), 0.46f, 0.9f, true);
            l.Set(HudElementId.RecallButton,   Mirror(right,0.62f), 0.10f, 0.85f, true);
            l.Set(HudElementId.PingWheel,      moveX,               0.42f, 0.85f, true);

            // Item slots across the top of the action cluster.
            for (int i = 0; i < 6; i++)
                l.Set((HudElementId)((int)HudElementId.Item1 + i), Mirror(right, 0.60f + i * 0.06f), 0.52f, 0.8f, true);

            // Top HUD.
            l.Set(HudElementId.Minimap, right ? 0.92f : 0.08f, 0.85f, 1.0f, true);
            l.Set(HudElementId.ShopButton,       moveX,        0.60f, 0.8f, true);
            l.Set(HudElementId.ScoreboardButton, 0.5f,         0.96f, 0.8f, true);
            l.Set(HudElementId.ChatButton,       right ? 0.08f : 0.92f, 0.60f, 0.8f, true);
            return l;
        }

        private static float Mirror(bool right, float x) => right ? x : 1f - x;
        private static float Clamp01(float v) => v < 0 ? 0 : (v > 1 ? 1 : v);
        private static float Clamp(float v, float lo, float hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}

// Project D | Codename: Zantetsuken — DuelItems.cs
// The duel's shop: an ORIGINAL item catalog (names, numbers and recipes are Project D's own), buying with component
// discounts, selling, gold, plus the practice-tool helpers on the world (invincible, no cooldown, dummies).
// Engine-agnostic and deterministic. Items are cosmetic-free, soft-currency only: gold is earned in the match, never bought.
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectD.Duel
{
    public enum ItemCategory { Attack, Magic, Defense, Move, Utility }

    public enum ShopOutcome { Ok, NoUnit, Dead, UnknownItem, NotEnoughGold, InventoryFull, EmptySlot }

    /// <summary>Flat stat bonuses an item (or a whole inventory) gives.</summary>
    public struct DuelItemStats
    {
        public float Ad, Ap, Hp, Armor, Mr, Mana;
        public float AttackSpeedPct;   // 0.2 = +20% attack speed
        public float MoveSpeedPct;     // 0.1 = +10% move speed
        public float AbilityHaste;     // 10 haste = abilities recharge 9% faster (100 / (100 + haste))

        public static DuelItemStats operator +(DuelItemStats a, DuelItemStats b)
        {
            return new DuelItemStats
            {
                Ad = a.Ad + b.Ad, Ap = a.Ap + b.Ap, Hp = a.Hp + b.Hp, Armor = a.Armor + b.Armor, Mr = a.Mr + b.Mr, Mana = a.Mana + b.Mana,
                AttackSpeedPct = a.AttackSpeedPct + b.AttackSpeedPct, MoveSpeedPct = a.MoveSpeedPct + b.MoveSpeedPct,
                AbilityHaste = a.AbilityHaste + b.AbilityHaste
            };
        }
    }

    public sealed class DuelItem
    {
        public string Id;
        public string Name;
        public ItemCategory Category;
        public int Cost;
        public DuelItemStats Stats;
        /// <summary>Component item ids. Owning a component takes its cost off the price and the component is used up.</summary>
        public string[] BuildsFrom;
        public int Tier => BuildsFrom != null && BuildsFrom.Length > 0 ? 2 : 1;

        /// <summary>"+35 AD, +20% AS" style one-liner. Every stat is spelled out, so it never relies on colour.</summary>
        public string StatLine()
        {
            var sb = new StringBuilder();
            Add(sb, Stats.Ad, " Attack");
            Add(sb, Stats.Ap, " Ability Power");
            Add(sb, Stats.Hp, " Health");
            Add(sb, Stats.Armor, " Armor");
            Add(sb, Stats.Mr, " Magic Resist");
            Add(sb, Stats.Mana, " Mana");
            Add(sb, Stats.AttackSpeedPct * 100f, "% Attack Speed");
            Add(sb, Stats.MoveSpeedPct * 100f, "% Move Speed");
            Add(sb, Stats.AbilityHaste, " Ability Haste");
            return sb.ToString();
        }

        private static void Add(StringBuilder sb, float value, string label)
        {
            if (value <= 0.001f) return;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append('+').Append((int)Math.Round(value)).Append(label);
        }

        public static string CategoryName(ItemCategory c)
        {
            switch (c)
            {
                case ItemCategory.Attack: return "ATTACK";
                case ItemCategory.Magic: return "MAGIC";
                case ItemCategory.Defense: return "DEFENSE";
                case ItemCategory.Move: return "MOVE";
                default: return "UTILITY";
            }
        }
    }

    public static class DuelItemCatalog
    {
        public const int MaxSlots = 6;
        public const float SellRatio = 0.7f;

        private static readonly List<DuelItem> _all = Build();
        private static readonly Dictionary<string, DuelItem> _byId = Index(_all);

        public static IReadOnlyList<DuelItem> All => _all;

        public static DuelItem Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _byId.TryGetValue(id, out var it) ? it : null;
        }

        public static DuelItemStats StatsOf(List<string> ids)
        {
            var total = new DuelItemStats();
            if (ids == null) return total;
            for (int i = 0; i < ids.Count; i++)
            {
                var it = Get(ids[i]);
                if (it != null) total = total + it.Stats;
            }
            return total;
        }

        private static Dictionary<string, DuelItem> Index(List<DuelItem> list)
        {
            var d = new Dictionary<string, DuelItem>();
            for (int i = 0; i < list.Count; i++) d[list[i].Id] = list[i];
            return d;
        }

        private static DuelItem Make(string id, string name, ItemCategory cat, int cost, DuelItemStats stats, params string[] from)
        {
            return new DuelItem { Id = id, Name = name, Category = cat, Cost = cost, Stats = stats, BuildsFrom = from.Length > 0 ? from : null };
        }

        private static List<DuelItem> Build()
        {
            return new List<DuelItem>
            {
                // ---- tier 1: basic pieces
                Make("iron_edge",    "Iron Edge",     ItemCategory.Attack,  350, new DuelItemStats { Ad = 12f }),
                Make("quill_charm",  "Quill Charm",   ItemCategory.Attack,  300, new DuelItemStats { AttackSpeedPct = 0.15f }),
                Make("ember_page",   "Ember Page",    ItemCategory.Magic,   350, new DuelItemStats { Ap = 18f }),
                Make("leather_vest", "Leather Vest",  ItemCategory.Defense, 300, new DuelItemStats { Armor = 15f }),
                Make("linen_cloak",  "Linen Cloak",   ItemCategory.Defense, 300, new DuelItemStats { Mr = 15f }),
                Make("vital_charm",  "Vital Charm",   ItemCategory.Defense, 300, new DuelItemStats { Hp = 120f }),
                Make("swift_sandals","Swift Sandals", ItemCategory.Move,    300, new DuelItemStats { MoveSpeedPct = 0.06f }),
                Make("focus_band",   "Focus Band",    ItemCategory.Utility, 350, new DuelItemStats { AbilityHaste = 10f }),
                Make("mana_phial",   "Mana Phial",    ItemCategory.Utility, 300, new DuelItemStats { Mana = 200f }),

                // ---- tier 2: built from the pieces above (cost is the full price; owned pieces are credited)
                Make("duelist_saber","Duelist's Saber",ItemCategory.Attack, 1000, new DuelItemStats { Ad = 40f }, "iron_edge", "iron_edge"),
                Make("windcutter",   "Windcutter",    ItemCategory.Attack,  1050, new DuelItemStats { Ad = 25f, AttackSpeedPct = 0.25f }, "iron_edge", "quill_charm"),
                Make("arcane_codex", "Arcane Codex",  ItemCategory.Magic,   1100, new DuelItemStats { Ap = 45f, AbilityHaste = 10f }, "ember_page", "focus_band"),
                Make("sages_staff",  "Sage's Staff",  ItemCategory.Magic,   1150, new DuelItemStats { Ap = 70f }, "ember_page", "ember_page"),
                Make("bastion_plate","Bastion Plate", ItemCategory.Defense, 1000, new DuelItemStats { Armor = 45f, Hp = 150f }, "leather_vest", "vital_charm"),
                Make("mystic_mantle","Mystic Mantle", ItemCategory.Defense, 1000, new DuelItemStats { Mr = 45f, Hp = 150f }, "linen_cloak", "vital_charm"),
                Make("heart_of_oak", "Heart of Oak",  ItemCategory.Defense,  900, new DuelItemStats { Hp = 450f }, "vital_charm", "vital_charm"),
                Make("windrunner",   "Windrunner Boots",ItemCategory.Move,   750, new DuelItemStats { MoveSpeedPct = 0.12f, AbilityHaste = 10f }, "swift_sandals", "focus_band"),
                Make("aether_locket","Aether Locket", ItemCategory.Utility, 1000, new DuelItemStats { Mana = 450f, AbilityHaste = 15f }, "mana_phial", "focus_band"),
            };
        }
    }

    public sealed partial class DuelWorld
    {
        /// <summary>Gold every champion earns per second (0 by default so tests stay exact; the demo turns it on).</summary>
        public float PassiveGoldPerSecond;
        /// <summary>Gold for taking down an enemy champion (0 by default so tests stay exact; the demo sets 300).</summary>
        public int ChampionKillGold;

        // ------------------------------------------------------------------ shop

        private static List<int> ComponentSlots(DuelUnit u, DuelItem item)
        {
            var slots = new List<int>();
            if (item.BuildsFrom == null) return slots;
            for (int c = 0; c < item.BuildsFrom.Length; c++)
            {
                for (int i = 0; i < u.Items.Count; i++)
                {
                    if (slots.Contains(i) || u.Items[i] != item.BuildsFrom[c]) continue;
                    slots.Add(i);
                    break;
                }
            }
            return slots;
        }

        /// <summary>What the unit would pay for the item right now (owned components are credited).</summary>
        public int PriceFor(DuelUnit u, DuelItem item)
        {
            int price = item.Cost;
            var used = ComponentSlots(u, item);
            for (int i = 0; i < used.Count; i++)
            {
                var comp = DuelItemCatalog.Get(u.Items[used[i]]);
                if (comp != null) price -= comp.Cost;
            }
            return Math.Max(0, price);
        }

        public ShopOutcome BuyItem(int unitId, string itemId)
        {
            var u = Get(unitId);
            if (u == null || u.Kind != UnitKind.Champion) return ShopOutcome.NoUnit;
            if (!u.Alive) return ShopOutcome.Dead;
            var item = DuelItemCatalog.Get(itemId);
            if (item == null) return ShopOutcome.UnknownItem;

            var used = ComponentSlots(u, item);
            int price = PriceFor(u, item);
            if (u.Items.Count - used.Count >= DuelItemCatalog.MaxSlots) return ShopOutcome.InventoryFull;
            if (u.Gold < price) return ShopOutcome.NotEnoughGold;

            used.Sort();
            for (int i = used.Count - 1; i >= 0; i--) u.Items.RemoveAt(used[i]);
            u.Items.Add(item.Id);
            u.Gold -= price;
            RecalcFromItems(u);
            Emit(DuelEventKind.Gold, u, null, -price, "Bought " + item.Name + " (-" + price + ")");
            return ShopOutcome.Ok;
        }

        public ShopOutcome SellItem(int unitId, int slot)
        {
            var u = Get(unitId);
            if (u == null || u.Kind != UnitKind.Champion) return ShopOutcome.NoUnit;
            if (!u.Alive) return ShopOutcome.Dead;
            if (slot < 0 || slot >= u.Items.Count) return ShopOutcome.EmptySlot;

            var item = DuelItemCatalog.Get(u.Items[slot]);
            u.Items.RemoveAt(slot);
            int refund = item != null ? (int)(item.Cost * DuelItemCatalog.SellRatio) : 0;
            u.Gold += refund;
            RecalcFromItems(u);
            Emit(DuelEventKind.Gold, u, null, refund, "Sold " + (item != null ? item.Name : "item") + " (+" + refund + ")");
            return ShopOutcome.Ok;
        }

        public void ClearItems(int unitId)
        {
            var u = Get(unitId);
            if (u == null) return;
            u.Items.Clear();
            RecalcFromItems(u);
        }

        /// <summary>Re-reads the stats after the inventory changed. Current health and mana rise with the new maximum.</summary>
        private void RecalcFromItems(DuelUnit u)
        {
            float oldMaxHp = u.MaxHp, oldHp = u.Hp, oldMaxMana = u.MaxMana, oldMana = u.Mana;
            ApplyLevelStats(u, false);
            u.Hp = Math.Max(1f, Math.Min(u.MaxHp, oldHp + Math.Max(0f, u.MaxHp - oldMaxHp)));
            u.Mana = Math.Max(0f, Math.Min(u.MaxMana, oldMana + Math.Max(0f, u.MaxMana - oldMaxMana)));
        }

        public void GrantGold(int unitId, int amount)
        {
            var u = Get(unitId);
            if (u == null || amount == 0) return;
            u.Gold = Math.Max(0, u.Gold + amount);
            if (amount > 0) Emit(DuelEventKind.Gold, u, null, amount, "+" + amount + " gold");
        }

        // ------------------------------------------------------------------ practice helpers

        public void SetInvincible(int unitId, bool on)
        {
            var u = Get(unitId);
            if (u != null) u.Invincible = on;
        }

        /// <summary>While on: abilities and summoner spells recharge instantly and cost no mana.</summary>
        public void SetNoCooldown(int unitId, bool on)
        {
            var u = Get(unitId);
            if (u == null) return;
            u.NoCooldown = on;
            if (on) ResetCooldowns(unitId);
        }

        /// <summary>Refresh: full health and mana, all cooldowns cleared, statuses removed. Alive units only keep their position.</summary>
        public void Refresh(int unitId)
        {
            var u = Get(unitId);
            if (u == null) return;
            if (!u.Alive) { Respawn(u); }
            u.Hp = u.MaxHp;
            u.Mana = u.MaxMana;
            u.Statuses.Clear();
            ResetCooldowns(unitId);
        }

        /// <summary>Defeats a unit regardless of invincibility (the practice "kill me" button). The killer gets the usual credit.</summary>
        public void Slay(int unitId, int killerId)
        {
            var u = Get(unitId);
            if (u == null || !u.Alive || u.Kind == UnitKind.Ward) return;
            Kill(Get(killerId), u);
        }

        /// <summary>A training dummy: it can be hit (and slowed, poisoned) but never fights back, and heals back up on its own.</summary>
        public DuelUnit AddDummy(int team, DVec2 pos, float hp = 4000f)
        {
            var u = new DuelUnit
            {
                Id = _nextId++, Name = team == 1 ? "Enemy Dummy" : "Ally Dummy", Kind = UnitKind.Dummy, Team = team,
                Pos = pos, SpawnPos = pos, MaxHp = hp, Hp = hp, Armor = 20f, Mr = 20f
            };
            Units.Add(u);
            return u;
        }

        /// <summary>Removes every dummy and returns their ids (the view uses them to clean up).</summary>
        public List<int> RemoveDummies()
        {
            var ids = new List<int>();
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                if (Units[i].Kind != UnitKind.Dummy) continue;
                ids.Add(Units[i].Id);
                Units.RemoveAt(i);
            }
            return ids;
        }

        public int CountDummies()
        {
            int n = 0;
            for (int i = 0; i < Units.Count; i++) if (Units[i].Kind == UnitKind.Dummy) n++;
            return n;
        }

        /// <summary>Total champion kills for a team (the scoreboard header).</summary>
        public int TeamKills(int team)
        {
            int n = 0;
            for (int i = 0; i < Units.Count; i++) if (Units[i].Kind == UnitKind.Champion && Units[i].Team == team) n += Units[i].Kills;
            return n;
        }

        public int TeamGold(int team)
        {
            int n = 0;
            for (int i = 0; i < Units.Count; i++) if (Units[i].Kind == UnitKind.Champion && Units[i].Team == team) n += Units[i].Gold;
            return n;
        }
    }
}

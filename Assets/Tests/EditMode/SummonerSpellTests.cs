// Project D | Codename: Zantetsuken — SummonerSpellTests.cs (catalog, mode rules, loadout, cooldown tracker, icons)
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ProjectD.Modes;
using ProjectD.SummonerSpells;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class SummonerSpellTests
    {
        private static readonly GameMode[] NormalAndRanked =
            { GameMode.RankedSolo, GameMode.RankedFlex, GameMode.NormalDraft, GameMode.NormalBlind };

        private static readonly GameMode[] OtherModes =
            { GameMode.CoOpVsAI, GameMode.Practice, GameMode.WorldRift };

        // ---- catalog: the cooldown table from the design brief ---- //

        [TestCase(SummonerSpellId.Haste, 90f)]
        [TestCase(SummonerSpellId.Cure, 100f)]
        [TestCase(SummonerSpellId.Protect, 100f)]
        [TestCase(SummonerSpellId.Slow, 100f)]
        [TestCase(SummonerSpellId.Esuna, 110f)]
        [TestCase(SummonerSpellId.Warp, 150f)]
        [TestCase(SummonerSpellId.Bio, 100f)]
        [TestCase(SummonerSpellId.Ultima, 10f)]
        [TestCase(SummonerSpellId.Teleport, 150f)]
        [TestCase(SummonerSpellId.Pray, 110f)]
        [TestCase(SummonerSpellId.Warpstrike, 10f)]
        public void Cooldowns_MatchTheDesignTable(SummonerSpellId id, float seconds)
        {
            Assert.AreEqual(seconds, SummonerSpellCatalog.Get(id).CooldownSeconds, 0.0001f);
        }

        [Test]
        public void Catalog_HasNineStandardAndTwoAdditionalSpells()
        {
            var all = SummonerSpellCatalog.All.ToList();

            Assert.AreEqual(11, all.Count);
            Assert.AreEqual(9, all.Count(s => s.IsStandard));
            CollectionAssert.AreEquivalent(
                new[] { SummonerSpellId.Pray, SummonerSpellId.Warpstrike },
                all.Where(s => !s.IsStandard).Select(s => s.Id).ToArray());
        }

        [Test]
        public void EverySpell_HasAUniqueNameDescriptionAndAtLeastOneEffect()
        {
            var all = SummonerSpellCatalog.All.ToList();

            Assert.AreEqual(all.Count, all.Select(s => s.DisplayName).Distinct().Count());
            foreach (var s in all)
            {
                Assert.IsNotEmpty(s.DisplayName, s.Id.ToString());
                Assert.IsNotEmpty(s.Description, s.Id.ToString());
                Assert.Greater(s.Effects.Count, 0, s.Id.ToString());
                Assert.Greater(s.CooldownSeconds, 0f, s.Id.ToString());
            }
        }

        [Test]
        public void SpecialShapes_TeleportChannels_UltimaHitsNeutrals_WarpstrikeRecasts()
        {
            Assert.AreEqual(3.5f, SummonerSpellCatalog.Get(SummonerSpellId.Teleport).CastTimeSeconds, 0.0001f);
            Assert.AreEqual(SpellTargeting.NeutralMonster, SummonerSpellCatalog.Get(SummonerSpellId.Ultima).Targeting);
            Assert.Greater(SummonerSpellCatalog.Get(SummonerSpellId.Warpstrike).RecastWindowSeconds, 0f);
            Assert.AreEqual(0f, SummonerSpellCatalog.Get(SummonerSpellId.Haste).CastTimeSeconds, 0.0001f);
        }

        // ---- effects scale with level ---- //

        [Test]
        public void EffectValue_ScalesWithLevel_AndClampsToTheValidRange()
        {
            var heal = SummonerSpellCatalog.Get(SummonerSpellId.Cure).Effects
                .First(e => e.Kind == SpellEffectKind.Heal);

            Assert.AreEqual(90f, heal.Value(1), 0.0001f);
            Assert.AreEqual(90f + 15f * 14f, heal.Value(SummonerSpellCatalog.MaxLevel), 0.0001f);
            Assert.AreEqual(heal.Value(1), heal.Value(0), 0.0001f);
            Assert.AreEqual(heal.Value(SummonerSpellCatalog.MaxLevel), heal.Value(99), 0.0001f);
        }

        // ---- availability by mode ---- //

        [Test]
        public void NormalAndRanked_OfferExactlyTheNineStandardSpells()
        {
            foreach (var mode in NormalAndRanked)
            {
                var offered = SummonerSpellCatalog.AvailableFor(mode);

                Assert.AreEqual(9, offered.Count, mode.ToString());
                Assert.IsTrue(offered.All(s => s.IsStandard), mode.ToString());
                Assert.IsFalse(SummonerSpellCatalog.IsAvailable(SummonerSpellId.Pray, mode), mode.ToString());
                Assert.IsFalse(SummonerSpellCatalog.IsAvailable(SummonerSpellId.Warpstrike, mode), mode.ToString());
            }
        }

        [Test]
        public void OtherModes_OfferAllElevenSpells()
        {
            foreach (var mode in OtherModes)
            {
                Assert.AreEqual(11, SummonerSpellCatalog.AvailableFor(mode).Count, mode.ToString());
                Assert.IsTrue(SummonerSpellCatalog.IsAvailable(SummonerSpellId.Pray, mode), mode.ToString());
                Assert.IsTrue(SummonerSpellCatalog.IsAvailable(SummonerSpellId.Warpstrike, mode), mode.ToString());
            }
        }

        // ---- loadout ---- //

        [Test]
        public void DefaultLoadout_IsCureAndWarp_AndLegalInEveryMode()
        {
            var l = SummonerLoadout.Default();

            Assert.AreEqual(SummonerSpellId.Cure, l.Get(0));
            Assert.AreEqual(SummonerSpellId.Warp, l.Get(1));
            foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
                Assert.IsTrue(l.IsValidFor(mode), mode.ToString());
        }

        [Test]
        public void Loadout_RejectsTheSameSpellInBothSlots()
        {
            var l = new SummonerLoadout();
            Assert.AreEqual(LoadoutResult.Ok, l.TrySet(0, SummonerSpellId.Haste, GameMode.RankedSolo));

            Assert.AreEqual(LoadoutResult.DuplicateSpell, l.TrySet(1, SummonerSpellId.Haste, GameMode.RankedSolo));
            Assert.IsNull(l.Get(1));
        }

        [Test]
        public void Loadout_ReplacingTheSameSlotWithTheSameSpell_IsFine()
        {
            var l = new SummonerLoadout();
            l.TrySet(0, SummonerSpellId.Haste, GameMode.NormalDraft);

            Assert.AreEqual(LoadoutResult.Ok, l.TrySet(0, SummonerSpellId.Haste, GameMode.NormalDraft));
        }

        [Test]
        public void Loadout_RejectsAdditionalSpellsInRankedAndNormal_ButAcceptsThemElsewhere()
        {
            foreach (var mode in NormalAndRanked)
            {
                var l = new SummonerLoadout();
                Assert.AreEqual(LoadoutResult.NotAvailableInMode, l.TrySet(0, SummonerSpellId.Pray, mode), mode.ToString());
                Assert.AreEqual(LoadoutResult.NotAvailableInMode, l.TrySet(1, SummonerSpellId.Warpstrike, mode), mode.ToString());
            }

            foreach (var mode in OtherModes)
            {
                var l = new SummonerLoadout();
                Assert.AreEqual(LoadoutResult.Ok, l.TrySet(0, SummonerSpellId.Pray, mode), mode.ToString());
                Assert.AreEqual(LoadoutResult.Ok, l.TrySet(1, SummonerSpellId.Warpstrike, mode), mode.ToString());
                Assert.IsTrue(l.IsValidFor(mode), mode.ToString());
            }
        }

        [Test]
        public void Loadout_RejectsBadSlotIndexes()
        {
            var l = new SummonerLoadout();

            Assert.AreEqual(LoadoutResult.BadSlot, l.TrySet(-1, SummonerSpellId.Haste, GameMode.Practice));
            Assert.AreEqual(LoadoutResult.BadSlot, l.TrySet(2, SummonerSpellId.Haste, GameMode.Practice));
            Assert.IsNull(l.Get(5));
        }

        [Test]
        public void Loadout_ASetCarriedIntoRanked_IsInvalidIfItHoldsAnAdditionalSpell()
        {
            var l = new SummonerLoadout();
            l.TrySet(0, SummonerSpellId.Pray, GameMode.Practice);
            l.TrySet(1, SummonerSpellId.Warp, GameMode.Practice);

            Assert.IsTrue(l.IsValidFor(GameMode.Practice));
            Assert.IsFalse(l.IsValidFor(GameMode.RankedSolo));
        }

        [Test]
        public void Loadout_IncompleteIsNeverValid()
        {
            var l = new SummonerLoadout();
            Assert.IsFalse(l.IsValidFor(GameMode.Practice));

            l.TrySet(0, SummonerSpellId.Haste, GameMode.Practice);
            Assert.IsFalse(l.IsComplete);
            Assert.IsFalse(l.IsValidFor(GameMode.Practice));
        }

        [Test]
        public void Loadout_SwapAndClone()
        {
            var l = SummonerLoadout.Default();
            var copy = l.Clone();

            l.Swap();

            Assert.AreEqual(SummonerSpellId.Warp, l.Get(0));
            Assert.AreEqual(SummonerSpellId.Cure, l.Get(1));
            Assert.AreEqual(SummonerSpellId.Cure, copy.Get(0));   // the clone is independent

            l.Clear(0);
            Assert.IsNull(l.Get(0));
            Assert.IsFalse(l.IsComplete);
        }

        // ---- tracker ---- //

        private static SummonerSpellTracker NewTracker(GameMode mode = GameMode.RankedSolo)
        {
            var l = new SummonerLoadout();
            l.TrySet(0, SummonerSpellId.Haste, mode);      // 90 s
            l.TrySet(1, SummonerSpellId.Ultima, mode);     // 10 s
            return new SummonerSpellTracker(l, mode);
        }

        [Test]
        public void Cast_StartsTheCooldown_AndBlocksUntilItEnds()
        {
            var t = NewTracker();

            Assert.AreEqual(SummonerCastResult.Success, t.TryCast(0, 100f, out var def));
            Assert.AreEqual(SummonerSpellId.Haste, def.Id);
            Assert.IsFalse(t.IsReady(0, 100f));
            Assert.AreEqual(90f, t.Remaining(0, 100f), 0.0001f);
            Assert.AreEqual(30f, t.Remaining(0, 160f), 0.0001f);

            Assert.AreEqual(SummonerCastResult.OnCooldown, t.TryCast(0, 189.9f, out var blocked));
            Assert.IsNull(blocked);
            Assert.AreEqual(SummonerCastResult.Success, t.TryCast(0, 190f, out _));
        }

        [Test]
        public void TheTwoSlots_CoolDownIndependently()
        {
            var t = NewTracker();
            t.TryCast(0, 0f, out _);

            Assert.IsTrue(t.IsReady(1, 0f));
            Assert.AreEqual(SummonerCastResult.Success, t.TryCast(1, 0f, out var def));
            Assert.AreEqual(SummonerSpellId.Ultima, def.Id);
            Assert.AreEqual(10f, t.Remaining(1, 0f), 0.0001f);
            Assert.AreEqual(90f, t.Remaining(0, 0f), 0.0001f);
        }

        [Test]
        public void Remaining01_GoesFromOneToZero()
        {
            var t = NewTracker();
            Assert.AreEqual(0f, t.Remaining01(0, 0f), 0.0001f);

            t.TryCast(0, 0f, out _);

            Assert.AreEqual(1f, t.Remaining01(0, 0f), 0.0001f);
            Assert.AreEqual(0.5f, t.Remaining01(0, 45f), 0.0001f);
            Assert.AreEqual(0f, t.Remaining01(0, 90f), 0.0001f);
            Assert.AreEqual(0f, t.Remaining01(0, 500f), 0.0001f);
        }

        [Test]
        public void Haste_ShortensCooldownsWithTheAbilityHasteFormula()
        {
            var t = NewTracker();
            t.SetHaste(100f);   // 100 * 100 / (100 + 100) = half

            t.TryCast(0, 0f, out _);

            Assert.AreEqual(45f, t.Remaining(0, 0f), 0.0001f);
            Assert.AreEqual(1f, t.Remaining01(0, 0f), 0.0001f);   // the fill still starts full
        }

        [Test]
        public void ResetCooldowns_MakesBothSlotsReadyAgain()
        {
            var t = NewTracker(GameMode.Practice);
            t.TryCast(0, 0f, out _);
            t.TryCast(1, 0f, out _);

            t.ResetCooldowns();

            Assert.IsTrue(t.IsReady(0, 0f));
            Assert.IsTrue(t.IsReady(1, 0f));
        }

        [Test]
        public void EmptyAndInvalidSlots_ReportEmptySlot()
        {
            var t = new SummonerSpellTracker(new SummonerLoadout(), GameMode.RankedSolo);

            Assert.AreEqual(SummonerCastResult.EmptySlot, t.TryCast(0, 0f, out var def));
            Assert.IsNull(def);
            Assert.AreEqual(SummonerCastResult.EmptySlot, t.TryCast(7, 0f, out _));
            Assert.IsFalse(t.IsReady(0, 0f));
            Assert.AreEqual(0f, t.Remaining(7, 0f), 0.0001f);
        }

        [Test]
        public void Tracker_RefusesAnAdditionalSpellInARankedMatch()
        {
            var l = new SummonerLoadout();
            l.TrySet(0, SummonerSpellId.Pray, GameMode.Practice);   // legal there, not in Ranked

            var t = new SummonerSpellTracker(l, GameMode.RankedSolo);

            Assert.AreEqual(SummonerCastResult.ModeLocked, t.TryCast(0, 0f, out _));
        }

        [Test]
        public void Tracker_SnapshotsTheLoadout_SoLaterEditsDoNotChangeARunningMatch()
        {
            var l = SummonerLoadout.Default();
            var t = new SummonerSpellTracker(l, GameMode.NormalDraft);

            l.Swap();

            Assert.AreEqual(SummonerSpellId.Cure, t.SpellIn(0));
            Assert.AreEqual(SummonerSpellId.Warp, t.SpellIn(1));
        }

        // ---- icons ---- //

        [Test]
        public void EverySpell_HasAUniqueIconPath_AndThePngShipsInTheProject()
        {
            var all = SummonerSpellCatalog.All.ToList();
            Assert.AreEqual(all.Count, all.Select(s => s.IconResourcePath).Distinct().Count());

            foreach (var s in all)
            {
                StringAssert.StartsWith("UI/SummonerSpells/summoner_", s.IconResourcePath);
                string file = Path.Combine(Application.dataPath, "Resources", s.IconResourcePath + ".png");
                Assert.IsTrue(File.Exists(file), "missing icon: " + file);
            }
        }
    }
}

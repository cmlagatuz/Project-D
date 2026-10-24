// Project D | Codename: Zantetsuken — SummonerMechanicsTests.cs (per-spell behaviour: scaling, decay, charges, Smite tiers)
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectD.Modes;
using ProjectD.SummonerSpells;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class SummonerMechanicsTests
    {
        private static SpellEffect Fx(SummonerSpellId id, SpellEffectKind kind)
        {
            Assert.IsTrue(SummonerSpellCatalog.Get(id).TryGetEffect(kind, out var e), id + " lacks " + kind);
            return e;
        }

        // ---- Haste ---- //

        [Test]
        public void Haste_BurstDecaysToTwentyFivePercent_ForEightSeconds()
        {
            var speed = Fx(SummonerSpellId.Haste, SpellEffectKind.MoveSpeedBuff);

            Assert.AreEqual(8f, speed.DurationSeconds, 0.0001f);
            Assert.Greater(speed.ValueAt(1, 0f), 0.25f);
            Assert.AreEqual(0.25f, speed.ValueAt(1, 2f), 0.0001f);
            Assert.AreEqual(0.25f, speed.ValueAt(1, 6f), 0.0001f);
            Assert.Greater(speed.ValueAt(1, 0.5f), speed.ValueAt(1, 1.5f));   // monotonic decay
        }

        [Test]
        public void HasteBuff_RunsItsDuration_ThenEnds()
        {
            var buff = new HasteBuff();
            buff.Start(100f);

            Assert.IsTrue(buff.IsActive(100f));
            Assert.AreEqual(8f, buff.Remaining(100f), 0.0001f);
            Assert.AreEqual(0.25f, buff.SpeedBonus(105f), 0.0001f);
            Assert.IsFalse(buff.IsActive(108f));
            Assert.AreEqual(0f, buff.SpeedBonus(108f), 0.0001f);
        }

        [Test]
        public void HasteBuff_ATakedownExtendsBySix_CappedAtTheOriginalLength_AndRefreshesTheBurst()
        {
            var buff = new HasteBuff();
            buff.Start(0f);

            buff.OnTakedown(3f);   // 5 s left + 6 = 11, capped at the original 8

            Assert.AreEqual(8f, buff.Remaining(3f), 0.0001f);
            Assert.Greater(buff.SpeedBonus(3f), 0.25f);               // the burst restarted
            Assert.AreEqual(0.25f, buff.SpeedBonus(6f), 0.0001f);     // and has settled again
        }

        [Test]
        public void HasteBuff_ATakedownLateInTheBuff_AddsTheFullSixSeconds()
        {
            var buff = new HasteBuff();
            buff.Start(0f);

            buff.OnTakedown(7.5f);   // 0.5 s left + 6 = 6.5, below the cap

            Assert.AreEqual(6.5f, buff.Remaining(7.5f), 0.0001f);
        }

        [Test]
        public void HasteBuff_ATakedownAfterItEnded_DoesNothing()
        {
            var buff = new HasteBuff();
            buff.Start(0f);

            buff.OnTakedown(20f);

            Assert.IsFalse(buff.IsActive(20f));
        }

        // ---- Cure ---- //

        [Test]
        public void Cure_Heals110To400ByLevel_AndGivesThirtyPercentSpeedForTwoSeconds()
        {
            var heal = Fx(SummonerSpellId.Cure, SpellEffectKind.Heal);
            var speed = Fx(SummonerSpellId.Cure, SpellEffectKind.MoveSpeedBuff);

            Assert.AreEqual(110f, heal.Value(1), 0.001f);
            Assert.AreEqual(400f, heal.Value(15), 0.001f);
            Assert.Greater(heal.Value(8), heal.Value(7));
            Assert.AreEqual(0.30f, speed.Value(1), 0.0001f);
            Assert.AreEqual(2f, speed.DurationSeconds, 0.0001f);
        }

        [Test]
        public void CureHealTracker_HalvesARepeatHeal_UntilTheWindowPasses()
        {
            var t = new CureHealTracker();

            Assert.AreEqual(110f, t.Heal(targetId: 7, level: 1, now: 0f), 0.001f);
            Assert.AreEqual(55f, t.Heal(7, 1, 10f), 0.001f);                 // recently healed
            Assert.AreEqual(110f, t.Heal(8, 1, 10f), 0.001f);                // someone else is unaffected
            Assert.AreEqual(110f, t.Heal(7, 1, 200f), 0.001f);               // long enough ago
        }

        [Test]
        public void PickMostWounded_ChoosesTheLowestHealthFraction_TiesToTheLowestId()
        {
            var c = new List<KeyValuePair<int, float>>
            {
                new KeyValuePair<int, float>(5, 0.80f),
                new KeyValuePair<int, float>(9, 0.30f),
                new KeyValuePair<int, float>(3, 0.30f),
            };

            Assert.AreEqual(3, SummonerSpellRules.PickMostWounded(c));
            Assert.AreEqual(-1, SummonerSpellRules.PickMostWounded(new List<KeyValuePair<int, float>>()));
            Assert.AreEqual(-1, SummonerSpellRules.PickMostWounded(null));
        }

        // ---- Protect ---- //

        [Test]
        public void Protect_Shields120To560ByLevel_ForTwoAndAHalfSeconds()
        {
            var shield = Fx(SummonerSpellId.Protect, SpellEffectKind.Shield);

            Assert.AreEqual(120f, shield.Value(1), 0.001f);
            Assert.AreEqual(560f, shield.Value(15), 0.001f);
            Assert.AreEqual(2.5f, shield.DurationSeconds, 0.0001f);
        }

        // ---- Slow ---- //

        [Test]
        public void Slow_StartsAtThirtyFivePercentAndFadesToNothing_AndCutsDamageByForty()
        {
            var slow = Fx(SummonerSpellId.Slow, SpellEffectKind.MoveSpeedSlow);
            var dmg = Fx(SummonerSpellId.Slow, SpellEffectKind.DamageDealtReduction);

            Assert.AreEqual(0.35f, slow.ValueAt(1, 0f), 0.0001f);
            Assert.AreEqual(0.175f, slow.ValueAt(1, 1.25f), 0.0001f);
            Assert.AreEqual(0f, slow.ValueAt(1, 2.5f), 0.0001f);
            Assert.AreEqual(2.5f, slow.DurationSeconds, 0.0001f);
            Assert.AreEqual(0.40f, dmg.Value(1), 0.0001f);
            Assert.AreEqual(2.5f, dmg.DurationSeconds, 0.0001f);
        }

        // ---- Esuna ---- //

        [Test]
        public void Esuna_CleansesAndGrantsAQuarterSecondOfDisableImmunity()
        {
            Assert.AreEqual(SpellTargeting.Self, SummonerSpellCatalog.Get(SummonerSpellId.Esuna).Targeting);
            Fx(SummonerSpellId.Esuna, SpellEffectKind.Cleanse);
            Assert.AreEqual(0.25f, Fx(SummonerSpellId.Esuna, SpellEffectKind.DisableImmunity).DurationSeconds, 0.0001f);
        }

        // ---- Warp ---- //

        [Test]
        public void Warp_IsAnAimedBlink()
        {
            var warp = SummonerSpellCatalog.Get(SummonerSpellId.Warp);

            Assert.AreEqual(SpellTargeting.Direction, warp.Targeting);
            Assert.Greater(Fx(SummonerSpellId.Warp, SpellEffectKind.Blink).Magnitude, 0f);
        }

        // ---- Bio ---- //

        [Test]
        public void Bio_Deals72To380TrueDamageOverFiveSeconds_CutsHealingBySixty_AndGrantsVision()
        {
            var dot = Fx(SummonerSpellId.Bio, SpellEffectKind.DamageOverTime);

            Assert.AreEqual(72f, dot.Value(1), 0.001f);
            Assert.AreEqual(380f, dot.Value(15), 0.001f);
            Assert.AreEqual(5f, dot.DurationSeconds, 0.0001f);
            Assert.AreEqual(0.60f, Fx(SummonerSpellId.Bio, SpellEffectKind.HealingReduction).Value(1), 0.0001f);
            Assert.AreEqual(5f, Fx(SummonerSpellId.Bio, SpellEffectKind.GrantVision).DurationSeconds, 0.0001f);
        }

        // ---- Ultima charges ---- //

        private static SummonerSpellTracker UltimaTracker(GameMode mode = GameMode.RankedSolo)
        {
            var l = new SummonerLoadout();
            l.TrySet(0, SummonerSpellId.Ultima, mode);
            l.TrySet(1, SummonerSpellId.Haste, mode);
            return new SummonerSpellTracker(l, mode);
        }

        [Test]
        public void Ultima_StartsWithTwoCharges_AndTenSecondsBetweenCasts()
        {
            var t = UltimaTracker();

            Assert.AreEqual(2, t.Charges(0, 0f));
            Assert.AreEqual(SummonerCastResult.Success, t.TryCast(0, 0f, out _));
            Assert.AreEqual(1, t.Charges(0, 0f));
            Assert.AreEqual(10f, t.Remaining(0, 0f), 0.0001f);

            Assert.AreEqual(SummonerCastResult.OnCooldown, t.TryCast(0, 9f, out _));
            Assert.AreEqual(SummonerCastResult.Success, t.TryCast(0, 10f, out _));
            Assert.AreEqual(0, t.Charges(0, 10f));
        }

        [Test]
        public void Ultima_WithNoCharges_WaitsForTheFortyFiveSecondRefill()
        {
            var t = UltimaTracker();
            t.TryCast(0, 0f, out _);
            t.TryCast(0, 10f, out _);

            Assert.AreEqual(SummonerCastResult.OnCooldown, t.TryCast(0, 20f, out _));
            Assert.AreEqual(25f, t.Remaining(0, 20f), 0.0001f);

            Assert.AreEqual(1, t.Charges(0, 45f));
            Assert.AreEqual(SummonerCastResult.Success, t.TryCast(0, 45f, out _));
        }

        [Test]
        public void Ultima_ChargesRefillOneAtATime_UpToTwo()
        {
            var t = UltimaTracker();
            t.TryCast(0, 0f, out _);
            t.TryCast(0, 10f, out _);

            Assert.AreEqual(0, t.Charges(0, 44f));
            Assert.AreEqual(1, t.Charges(0, 45f));
            Assert.AreEqual(1, t.Charges(0, 89f));
            Assert.AreEqual(2, t.Charges(0, 90f));
            Assert.AreEqual(2, t.Charges(0, 1000f));
        }

        [Test]
        public void Ultima_ResetCooldowns_RefillsEveryCharge()
        {
            var t = UltimaTracker(GameMode.Practice);
            t.TryCast(0, 0f, out _);
            t.TryCast(0, 10f, out _);

            t.ResetCooldowns();

            Assert.AreEqual(2, t.Charges(0, 10f));
            Assert.IsTrue(t.IsReady(0, 10f));
        }

        [Test]
        public void Ultima_Haste_ShortensBothTheGapAndTheRefill()
        {
            var t = UltimaTracker();
            t.SetHaste(100f);

            t.TryCast(0, 0f, out _);

            Assert.AreEqual(5f, t.Remaining(0, 0f), 0.0001f);
            t.TryCast(0, 5f, out _);

            Assert.AreEqual(0, t.Charges(0, 22.4f));   // refill is 45 s * 0.5 = 22.5 s after the first cast
            Assert.AreEqual(1, t.Charges(0, 22.5f));
        }

        [Test]
        public void PlainSpells_ReportOneOrZeroCharges()
        {
            var t = UltimaTracker();

            Assert.AreEqual(1, t.Charges(1, 0f));
            t.TryCast(1, 0f, out _);
            Assert.AreEqual(0, t.Charges(1, 1f));
            Assert.AreEqual(1, t.Charges(1, 90f));
        }

        // ---- map rule ---- //

        [Test]
        public void UltimaAndTeleport_AreMainMapOnly_SoWorldRiftRefusesThem()
        {
            foreach (var id in new[] { SummonerSpellId.Ultima, SummonerSpellId.Teleport })
            {
                Assert.IsTrue(SummonerSpellCatalog.IsAvailable(id, GameMode.RankedSolo), id.ToString());
                Assert.IsTrue(SummonerSpellCatalog.IsAvailable(id, GameMode.Practice), id.ToString());
                Assert.IsFalse(SummonerSpellCatalog.IsAvailable(id, GameMode.WorldRift), id.ToString());

                var l = new SummonerLoadout();
                Assert.AreEqual(LoadoutResult.NotAvailableInMode, l.TrySet(0, id, GameMode.WorldRift), id.ToString());
            }
        }

        [Test]
        public void Tracker_RefusesUltimaInWorldRift()
        {
            var l = new SummonerLoadout();
            l.TrySet(0, SummonerSpellId.Ultima, GameMode.Practice);

            var t = new SummonerSpellTracker(l, GameMode.WorldRift);

            Assert.AreEqual(SummonerCastResult.ModeLocked, t.TryCast(0, 0f, out _));
        }

        // ---- Teleport ---- //

        [Test]
        public void Teleport_Channels3Point5Seconds_ToStructuresOnlyInTheFirstSixMinutes()
        {
            Assert.AreEqual(3.5f, SummonerSpellCatalog.Get(SummonerSpellId.Teleport).CastTimeSeconds, 0.0001f);

            Assert.IsTrue(SummonerSpellRules.CanTeleportTo(TeleportTargets.AlliedStructure, 0f));
            Assert.IsTrue(SummonerSpellRules.CanTeleportTo(TeleportTargets.AlliedStructure, 359.9f));
            Assert.IsFalse(SummonerSpellRules.CanTeleportTo(TeleportTargets.AlliedStructure, 360f));

            Assert.IsTrue(SummonerSpellRules.CanTeleportTo(TeleportTargets.AlliedChampion, 2000f));
            Assert.IsTrue(SummonerSpellRules.CanTeleportTo(TeleportTargets.Ward, 2000f));
            Assert.IsFalse(SummonerSpellRules.CanTeleportTo(TeleportTargets.None, 0f));
        }

        // ---- Ultima / Smite damage and economy ---- //

        [Test]
        public void SmiteMath_DamageTiers_Are600_1000_1400()
        {
            Assert.AreEqual(600f, SmiteMath.BaseDamage(0), 0.0001f);
            Assert.AreEqual(1000f, SmiteMath.BaseDamage(1), 0.0001f);
            Assert.AreEqual(1400f, SmiteMath.BaseDamage(2), 0.0001f);
            Assert.AreEqual(1400f, SmiteMath.BaseDamage(9), 0.0001f);
            Assert.AreEqual(600f, SmiteMath.BaseDamage(-3), 0.0001f);
        }

        [Test]
        public void SmiteMath_MonstersTakeTenPercentMore_ChampionsOnlyWhenUpgraded()
        {
            Assert.AreEqual(600f, SmiteMath.DamageVs(SmiteTarget.Minion, 0), 0.001f);
            Assert.AreEqual(660f, SmiteMath.DamageVs(SmiteTarget.Monster, 0), 0.001f);
            Assert.AreEqual(1540f, SmiteMath.DamageVs(SmiteTarget.Monster, 2), 0.001f);

            Assert.AreEqual(0f, SmiteMath.DamageVs(SmiteTarget.Champion, 0), 0.0001f);
            Assert.AreEqual(40f, SmiteMath.DamageVs(SmiteTarget.Champion, 1), 0.0001f);
            Assert.AreEqual(0.25f, SmiteMath.ChampionSpeedSteal, 0.0001f);
            Assert.AreEqual(2f, SmiteMath.ChampionSpeedStealSeconds, 0.0001f);
        }

        [Test]
        public void SmiteMath_CampBurn_ScalesWithBonusStats()
        {
            Assert.AreEqual(22f, SmiteMath.CampBurnPerSecond(1, 0, 0, 0, 0, 0), 0.0001f);

            // 22 + 10% AD(100) + 12% AP(50) + 25% armor(40) + 25% MR(30) + 4% health(1000)
            Assert.AreEqual(95.5f, SmiteMath.CampBurnPerSecond(1, 100f, 50f, 40f, 30f, 1000f), 0.001f);
        }

        [Test]
        public void SmiteMath_HealPerSecond_Goes5To35()
        {
            Assert.AreEqual(5f, SmiteMath.HealPerSecondVsMonsters(1), 0.001f);
            Assert.AreEqual(35f, SmiteMath.HealPerSecondVsMonsters(15), 0.001f);
            Assert.AreEqual(35f, SmiteMath.HealPerSecondVsMonsters(40), 0.001f);
        }

        [Test]
        public void SmiteMath_JungleSpeed_OnlyWhenFullyUpgraded_InJungleOrRiver()
        {
            Assert.AreEqual(0.10f, SmiteMath.JungleMoveSpeedBonus(2, true, false), 0.0001f);
            Assert.AreEqual(0.05f, SmiteMath.JungleMoveSpeedBonus(2, true, true), 0.0001f);
            Assert.AreEqual(0f, SmiteMath.JungleMoveSpeedBonus(2, false, false), 0.0001f);
            Assert.AreEqual(0f, SmiteMath.JungleMoveSpeedBonus(1, true, false), 0.0001f);
        }

        [Test]
        public void SmiteProgress_StacksAccrueOverTime_AndAreCapped()
        {
            var p = new SmiteProgress();

            Assert.AreEqual(0, p.Stacks(0f));
            Assert.AreEqual(1, p.Stacks(SmiteProgress.StackIntervalSeconds));
            Assert.AreEqual(SmiteProgress.MaxStoredStacks, p.Stacks(10000f));
        }

        [Test]
        public void SmiteProgress_ClearingACamp_SpendsAStackForFortyGold()
        {
            var p = new SmiteProgress();

            Assert.AreEqual(0f, p.OnCampCleared(1f), 0.0001f);   // nothing earned yet
            Assert.AreEqual(40f, p.OnCampCleared(SmiteProgress.StackIntervalSeconds), 0.0001f);
            Assert.AreEqual(1, p.StacksConsumed);
            Assert.AreEqual(0, p.Stacks(SmiteProgress.StackIntervalSeconds));
        }

        [Test]
        public void SmiteProgress_UpgradesAtEightAndTwentyConsumedStacks()
        {
            var p = new SmiteProgress();
            float now = 0f;

            for (int i = 0; i < 7; i++) { now += SmiteProgress.StackIntervalSeconds; p.OnCampCleared(now); }
            Assert.AreEqual(0, p.Tier);
            Assert.IsFalse(p.CanTargetChampions);

            now += SmiteProgress.StackIntervalSeconds; p.OnCampCleared(now);
            Assert.AreEqual(8, p.StacksConsumed);
            Assert.AreEqual(1, p.Tier);
            Assert.IsTrue(p.CanTargetChampions);

            for (int i = 0; i < 11; i++) { now += SmiteProgress.StackIntervalSeconds; p.OnCampCleared(now); }
            Assert.AreEqual(1, p.Tier);

            now += SmiteProgress.StackIntervalSeconds; p.OnCampCleared(now);
            Assert.AreEqual(20, p.StacksConsumed);
            Assert.AreEqual(2, p.Tier);
        }
    }
}

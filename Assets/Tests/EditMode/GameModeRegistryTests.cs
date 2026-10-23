// Project D | Codename: Zantetsuken — GameModeRegistryTests.cs (the one-canonical-map fairness rule)
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectD.Modes;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class GameModeRegistryTests
    {
        private const string Canonical = GameModeRegistry.CanonicalMapId;

        [Test]
        public void CanonicalMapId_IsCrystallineRift()
        {
            Assert.AreEqual("crystalline_rift", Canonical);
        }

        [Test]
        public void EveryGameMode_HasADefinition()
        {
            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
            {
                var def = GameModeRegistry.Get(mode);
                Assert.AreEqual(mode, def.mode, mode.ToString());
                Assert.IsNotEmpty(def.ratingPool, mode.ToString());
            }
        }

        [TestCase(GameMode.RankedSolo)]
        [TestCase(GameMode.RankedFlex)]
        [TestCase(GameMode.NormalDraft)]
        [TestCase(GameMode.NormalBlind)]
        [TestCase(GameMode.CoOpVsAI)]
        [TestCase(GameMode.Practice)]
        public void NonWorldRiftModes_AcceptOnlyTheCanonicalMap(GameMode mode)
        {
            Assert.IsTrue(GameModeRegistry.IsValidMap(mode, Canonical));
            Assert.IsFalse(GameModeRegistry.IsValidMap(mode, "ashen_realm"));
            Assert.IsFalse(GameModeRegistry.IsValidMap(mode, ""));
            Assert.AreEqual(MapPolicy.CanonicalOnly, GameModeRegistry.Get(mode).mapPolicy);
        }

        [Test]
        public void WorldRift_RejectsTheCanonicalMapAndEmptyIds_ButAcceptsAnyRealm()
        {
            Assert.IsFalse(GameModeRegistry.IsValidMap(GameMode.WorldRift, Canonical));
            Assert.IsFalse(GameModeRegistry.IsValidMap(GameMode.WorldRift, ""));
            Assert.IsFalse(GameModeRegistry.IsValidMap(GameMode.WorldRift, null));
            Assert.IsTrue(GameModeRegistry.IsValidMap(GameMode.WorldRift, "ashen_realm"));
        }

        [Test]
        public void OnlyRankedModes_OnTheCanonicalMap_CanAwardRankedLp()
        {
            Assert.IsTrue(GameModeRegistry.CanAwardRankedLp(GameMode.RankedSolo, Canonical));
            Assert.IsTrue(GameModeRegistry.CanAwardRankedLp(GameMode.RankedFlex, Canonical));

            Assert.IsFalse(GameModeRegistry.CanAwardRankedLp(GameMode.RankedSolo, "ashen_realm"));
            Assert.IsFalse(GameModeRegistry.CanAwardRankedLp(GameMode.NormalDraft, Canonical));
            Assert.IsFalse(GameModeRegistry.CanAwardRankedLp(GameMode.Practice, Canonical));
        }

        [Test]
        public void WorldRift_NeverAwardsRankedLp_OnAnyMap()
        {
            Assert.IsFalse(GameModeRegistry.CanAwardRankedLp(GameMode.WorldRift, Canonical));
            Assert.IsFalse(GameModeRegistry.CanAwardRankedLp(GameMode.WorldRift, "ashen_realm"));
            Assert.IsFalse(GameModeRegistry.Get(GameMode.WorldRift).awardsRankedLp);
            Assert.IsFalse(GameModeRegistry.IsCompetitive(GameMode.WorldRift));
            Assert.AreEqual("worldrift_casual", GameModeRegistry.Get(GameMode.WorldRift).ratingPool);
        }

        [Test]
        public void IsCompetitive_OnlyForRankedModes()
        {
            Assert.IsTrue(GameModeRegistry.IsCompetitive(GameMode.RankedSolo));
            Assert.IsTrue(GameModeRegistry.IsCompetitive(GameMode.RankedFlex));
            Assert.IsFalse(GameModeRegistry.IsCompetitive(GameMode.NormalDraft));
            Assert.IsFalse(GameModeRegistry.IsCompetitive(GameMode.CoOpVsAI));
        }

        [Test]
        public void ModesForCanonicalMap_IsEveryModeExceptWorldRift()
        {
            var modes = GameModeRegistry.ModesForCanonicalMap().ToList();

            CollectionAssert.DoesNotContain(modes, GameMode.WorldRift);
            Assert.AreEqual(Enum.GetValues(typeof(GameMode)).Length - 1, modes.Count);
        }

        [Test]
        public void RankedModes_ShareNoRatingPoolWithNormals()
        {
            var ranked = new HashSet<string>
            {
                GameModeRegistry.Get(GameMode.RankedSolo).ratingPool,
                GameModeRegistry.Get(GameMode.RankedFlex).ratingPool
            };
            Assert.IsFalse(ranked.Contains(GameModeRegistry.Get(GameMode.NormalDraft).ratingPool));
            Assert.IsFalse(ranked.Contains(GameModeRegistry.Get(GameMode.WorldRift).ratingPool));
        }
    }
}

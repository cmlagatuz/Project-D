// Project D | Codename: Zantetsuken — AppFlowControllerTests.cs (the headless vertical slice, end to end)
using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectD.Modes;
using ProjectD.P15.App;
using ProjectD.P15.MatchLoop;
using ProjectD.P15.Save;
using ProjectD.P15.Session;
using ProjectD.P15.StateMachine;

namespace ProjectD.Tests
{
    using Loop = ProjectD.P15.MatchLoop.MatchLoop;

    [TestFixture]
    public sealed class AppFlowControllerTests
    {
        private static AppFlowController NewApp(SaveStore store = null, string playerId = "player1")
            => new AppFlowController(store ?? new SaveStore(), playerId);

        private static AppFlowController AtModeSelect()
        {
            var app = NewApp();
            Assert.IsTrue(app.ToMainMenu());
            Assert.IsTrue(app.OpenModeSelect());
            return app;
        }

        // ---- the whole loop ---- //

        [Test]
        public void FullVerticalSlice_RunsHeadlessAndReturnsToTheMenu()
        {
            var app = NewApp();
            var visited = new List<GameState>();
            app.OnEnterState += s => visited.Add(s);

            Assert.AreEqual(GameState.Boot, app.Fsm.Current);
            Assert.IsTrue(app.ToMainMenu());
            Assert.AreEqual("MainMenu", app.CurrentScene);
            Assert.IsTrue(app.OpenModeSelect());
            Assert.IsTrue(app.ChooseMode(FlowMode.RankedSolo));
            Assert.IsTrue(app.MatchFound());
            Assert.IsTrue(app.LockChampion("zantetsuken"));
            Assert.AreEqual("zantetsuken", app.Session.selectedChampionId);
            Assert.IsTrue(app.BeginMatch());
            Assert.AreEqual("Match", app.CurrentScene);

            var result = app.RunMatch(new Loop(new MatchLoopConfig()), t => (10f, 1f));

            Assert.AreEqual(GameState.PostGame, app.Fsm.Current);
            Assert.IsTrue(result.localWon);
            Assert.AreEqual("PostGame", app.CurrentScene);

            var progression = app.FinishMatch(firstWinOfDay: true);

            Assert.AreEqual(GameState.MainMenu, app.Fsm.Current);
            Assert.IsNull(app.Session.selectedChampionId);
            CollectionAssert.Contains(progression.rewardIds, "first_win_of_day");
            CollectionAssert.AreEqual(new[]
            {
                GameState.MainMenu, GameState.ModeSelect, GameState.Queue, GameState.ChampSelect,
                GameState.Loading, GameState.InMatch, GameState.PostGame, GameState.MainMenu
            }, visited);
        }

        [Test]
        public void FinishMatch_AppliesProgression_AndPersistsItForTheNextSession()
        {
            var store = new SaveStore();
            var app = NewApp(store);
            app.ToMainMenu(); app.OpenModeSelect(); app.ChooseMode(FlowMode.NormalDraft);
            app.MatchFound(); app.LockChampion("zantetsuken"); app.BeginMatch();
            var result = app.RunMatch(new Loop(new MatchLoopConfig()), t => (10f, 1f));

            var progression = app.FinishMatch(firstWinOfDay: true);

            int expectedXp = 300 + (int)(result.durationSeconds / 60f) * 12;
            Assert.AreEqual(expectedXp, progression.xpGained);
            Assert.AreEqual(400L, progression.kpGained);   // first-win bonus, no level-up at this XP
            Assert.AreEqual(400L, app.Profile.kp);

            var nextSession = NewApp(store);
            Assert.AreEqual(400L, nextSession.Profile.kp);
            Assert.AreEqual(app.Profile.xpIntoLevel, nextSession.Profile.xpIntoLevel);
            Assert.AreEqual("player1", nextSession.Profile.playerId);
        }

        [Test]
        public void ALoss_GivesNoFirstWinBonus()
        {
            var app = NewApp();
            app.ToMainMenu(); app.OpenModeSelect(); app.ChooseMode(FlowMode.NormalBlind);
            app.MatchFound(); app.LockChampion("zantetsuken"); app.BeginMatch();
            var result = app.RunMatch(new Loop(new MatchLoopConfig()), t => (1f, 10f));

            var progression = app.FinishMatch(firstWinOfDay: true);

            Assert.IsFalse(result.localWon);
            Assert.AreEqual(0L, progression.kpGained);
            CollectionAssert.DoesNotContain(progression.rewardIds, "first_win_of_day");
        }

        // ---- transitions ---- //

        [Test]
        public void StepsOutOfOrder_AreRejected()
        {
            var app = NewApp();

            Assert.IsFalse(app.OpenModeSelect());     // still at Boot
            Assert.IsFalse(app.BeginMatch());
            Assert.AreEqual(GameState.Boot, app.Fsm.Current);

            app.ToMainMenu();
            Assert.IsFalse(app.MatchFound());          // not queued yet
            Assert.IsFalse(app.LockChampion("x"));
            Assert.AreEqual(GameState.MainMenu, app.Fsm.Current);
        }

        [Test]
        public void Dodging_ChampSelect_ReturnsToTheQueue()
        {
            var app = AtModeSelect();
            app.ChooseMode(FlowMode.RankedFlex);
            app.MatchFound();

            Assert.IsTrue(app.Fsm.Transition(GameState.Queue));
            Assert.AreEqual(GameState.Queue, app.Fsm.Current);
        }

        // ---- fairness rule (one canonical map) ---- //

        [Test]
        public void EveryNonWorldRiftMode_QueuesOnTheCanonicalMap_AcceptedByTheRegistry()
        {
            foreach (FlowMode flow in Enum.GetValues(typeof(FlowMode)))
            {
                if (flow == FlowMode.WorldRift) continue;

                var app = AtModeSelect();
                Assert.IsTrue(app.ChooseMode(flow), flow.ToString());

                var mode = (GameMode)Enum.Parse(typeof(GameMode), flow.ToString());
                Assert.AreEqual(GameModeRegistry.CanonicalMapId, app.Session.mapId, flow.ToString());
                Assert.IsTrue(GameModeRegistry.IsValidMap(mode, app.Session.mapId), flow.ToString());
            }
        }

        [Test]
        public void OnlyRankedModes_AreFlaggedCompetitive()
        {
            foreach (FlowMode flow in Enum.GetValues(typeof(FlowMode)))
            {
                if (flow == FlowMode.WorldRift) continue;   // covered below, needs a realm
                var app = AtModeSelect();
                app.ChooseMode(flow);

                bool expected = flow == FlowMode.RankedSolo || flow == FlowMode.RankedFlex;
                Assert.AreEqual(expected, app.Session.isCompetitive, flow.ToString());
            }
        }

        [Test]
        public void WorldRift_UsesTheChosenRealm_IsNeverCompetitive_AndIsRegistryValid()
        {
            var app = AtModeSelect();

            Assert.IsTrue(app.ChooseMode(FlowMode.WorldRift, "ashen_realm"));

            Assert.AreEqual("ashen_realm", app.Session.mapId);
            Assert.IsFalse(app.Session.isCompetitive);
            Assert.IsTrue(GameModeRegistry.IsValidMap(GameMode.WorldRift, app.Session.mapId));
            Assert.IsFalse(GameModeRegistry.CanAwardRankedLp(GameMode.WorldRift, app.Session.mapId));
        }

        [Test]
        public void WorldRift_WithoutARealm_IsRefusedAndTheSessionIsLeftAlone()
        {
            var app = AtModeSelect();

            Assert.IsFalse(app.ChooseMode(FlowMode.WorldRift));
            Assert.IsFalse(app.ChooseMode(FlowMode.WorldRift, ""));

            Assert.AreEqual(GameState.ModeSelect, app.Fsm.Current);
            Assert.AreNotEqual(FlowMode.WorldRift, app.Session.mode);
        }

        [Test]
        public void WorldRiftWin_PaysCosmeticKpPerManifestationClaim_AndRaisesTheCasualRating()
        {
            var app = AtModeSelect();
            app.ChooseMode(FlowMode.WorldRift, "ashen_realm");
            app.MatchFound(); app.LockChampion("zantetsuken"); app.BeginMatch();

            var loop = new Loop(new MatchLoopConfig { worldRift = true });
            bool claimed = false;

            // RunMatch restarts the loop, so the claim has to happen while it is running.
            var result = app.RunMatch(loop, t =>
            {
                if (!claimed && t >= 5f) { loop.RecordManifestationClaim(); claimed = true; }
                return (10f, 1f);
            });

            Assert.IsTrue(result.localWon);
            Assert.AreEqual(1, result.manifestationsClaimed);

            var progression = app.FinishMatch(firstWinOfDay: false);

            Assert.AreEqual(60L + 15L, progression.kpGained);
            Assert.AreEqual(1012, app.Profile.worldRiftRating);
            CollectionAssert.Contains(progression.rewardIds, "worldrift_badge");
        }
    }
}

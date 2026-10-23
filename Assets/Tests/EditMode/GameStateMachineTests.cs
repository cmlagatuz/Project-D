// Project D | Codename: Zantetsuken — GameStateMachineTests.cs (legal transitions + scene routing)
using System.Collections.Generic;
using NUnit.Framework;
using ProjectD.P15.StateMachine;
using ProjectD.P15.Routing;

namespace ProjectD.Tests
{
    [TestFixture]
    public sealed class GameStateMachineTests
    {
        private GameStateMachine _fsm;

        [SetUp]
        public void SetUp() => _fsm = new GameStateMachine();

        private void WalkTo(params GameState[] path)
        {
            foreach (var s in path)
                Assert.IsTrue(_fsm.Transition(s), "expected a legal move to " + s);
        }

        [Test]
        public void StartsAtBoot_AndOnlyMainMenuIsReachable()
        {
            Assert.AreEqual(GameState.Boot, _fsm.Current);
            Assert.IsTrue(_fsm.CanTransition(GameState.MainMenu));
            Assert.IsFalse(_fsm.CanTransition(GameState.InMatch));
            Assert.IsFalse(_fsm.CanTransition(GameState.PostGame));
        }

        [Test]
        public void HappyPath_WalksTheWholeLoop()
        {
            WalkTo(GameState.MainMenu, GameState.ModeSelect, GameState.Queue, GameState.ChampSelect,
                   GameState.Loading, GameState.InMatch, GameState.PostGame, GameState.MainMenu);
            Assert.AreEqual(GameState.MainMenu, _fsm.Current);
        }

        [Test]
        public void IllegalTransition_IsRejectedAndLeavesStateUntouched()
        {
            WalkTo(GameState.MainMenu);

            Assert.IsFalse(_fsm.Transition(GameState.InMatch));   // can't skip the queue
            Assert.AreEqual(GameState.MainMenu, _fsm.Current);
        }

        [Test]
        public void InMatch_CanOnlyGoToPostGame()
        {
            WalkTo(GameState.MainMenu, GameState.ModeSelect, GameState.Queue, GameState.ChampSelect,
                   GameState.Loading, GameState.InMatch);

            Assert.IsFalse(_fsm.CanTransition(GameState.MainMenu));
            Assert.IsFalse(_fsm.CanTransition(GameState.Queue));
            Assert.IsTrue(_fsm.CanTransition(GameState.PostGame));
        }

        [Test]
        public void QueueAndChampSelect_AllowCancelAndDodge()
        {
            WalkTo(GameState.MainMenu, GameState.ModeSelect, GameState.Queue);
            Assert.IsTrue(_fsm.CanTransition(GameState.MainMenu));   // cancel queue

            WalkTo(GameState.ChampSelect);
            Assert.IsTrue(_fsm.CanTransition(GameState.Queue));      // dodge
        }

        [Test]
        public void PostGame_CanRequeueOrReturnToMenu()
        {
            WalkTo(GameState.MainMenu, GameState.ModeSelect, GameState.Queue, GameState.ChampSelect,
                   GameState.Loading, GameState.InMatch, GameState.PostGame);

            Assert.IsTrue(_fsm.CanTransition(GameState.ModeSelect));
            Assert.IsTrue(_fsm.CanTransition(GameState.MainMenu));
        }

        [Test]
        public void OnTransition_ReportsFromAndTo_OnlyForLegalMoves()
        {
            var seen = new List<(GameState prev, GameState next)>();
            _fsm.OnTransition += (prev, next) => seen.Add((prev, next));

            _fsm.Transition(GameState.PostGame);        // illegal, no event
            _fsm.Transition(GameState.MainMenu);
            _fsm.Transition(GameState.ModeSelect);

            Assert.AreEqual(2, seen.Count);
            Assert.AreEqual((GameState.Boot, GameState.MainMenu), seen[0]);
            Assert.AreEqual((GameState.MainMenu, GameState.ModeSelect), seen[1]);
        }

        [Test]
        public void Reset_ReturnsToBoot()
        {
            WalkTo(GameState.MainMenu, GameState.ModeSelect);
            _fsm.Reset();
            Assert.AreEqual(GameState.Boot, _fsm.Current);
        }

        // ---- SceneRouter ---- //

        [TestCase(GameState.Boot, "Boot")]
        [TestCase(GameState.MainMenu, "MainMenu")]
        [TestCase(GameState.ModeSelect, "ModeSelect")]
        [TestCase(GameState.Queue, "MainMenu")]          // queueing is an overlay on the menu
        [TestCase(GameState.ChampSelect, "ChampSelect")]
        [TestCase(GameState.Loading, "Loading")]
        [TestCase(GameState.InMatch, "Match")]
        [TestCase(GameState.PostGame, "PostGame")]
        public void SceneFor_MapsEveryState(GameState state, string scene)
        {
            Assert.AreEqual(scene, SceneRouter.SceneFor(state));
        }

        [Test]
        public void SceneFor_UnknownState_FallsBackToMainMenu()
        {
            Assert.AreEqual("MainMenu", SceneRouter.SceneFor((GameState)999));
        }

        [Test]
        public void RequiresSceneLoad_IsFalseForInSceneOverlays_TrueOtherwise()
        {
            Assert.IsFalse(SceneRouter.RequiresSceneLoad(GameState.MainMenu, GameState.Queue));
            Assert.IsFalse(SceneRouter.RequiresSceneLoad(GameState.Queue, GameState.MainMenu));
            Assert.IsTrue(SceneRouter.RequiresSceneLoad(GameState.ChampSelect, GameState.Loading));
            Assert.IsTrue(SceneRouter.RequiresSceneLoad(GameState.InMatch, GameState.PostGame));
        }
    }
}

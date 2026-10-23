// Project D | Codename: Zantetsuken — AppFlowController.cs (top-level orchestrator)
using System;
using ProjectD.P15.StateMachine; using ProjectD.P15.Session; using ProjectD.P15.MatchLoop;
using ProjectD.P15.Save; using ProjectD.P15.Progression; using ProjectD.P15.Routing;
namespace ProjectD.P15.App
{
    using MatchLoop = ProjectD.P15.MatchLoop.MatchLoop;
    // Sequences the whole vertical slice: MainMenu -> ModeSelect -> Queue -> ChampSelect
    // -> Loading -> InMatch -> PostGame -> (rewards) -> MainMenu. Engine-agnostic so the
    // full loop runs in a test; the client mounts scene views onto its transitions.
    public sealed class AppFlowController
    {
        public GameStateMachine Fsm{get;}=new();
        public SessionContext Session{get;}=new();
        public PlayerProfileSave Profile{get;private set;}
        public MatchResult LastResult{get;private set;}
        public ProgressionResult LastProgression{get;private set;}
        readonly SaveStore _store;

        public event Action<GameState> OnEnterState;

        public AppFlowController(SaveStore store,string playerId){ _store=store; Profile=_store.Load(playerId); Profile.playerId=playerId; Session.localPlayerId=playerId; Fsm.OnTransition+=(_,to)=>OnEnterState?.Invoke(to); }

        public bool ToMainMenu()=>Fsm.Transition(GameState.MainMenu);
        public bool OpenModeSelect()=>Fsm.Transition(GameState.ModeSelect);
        public bool ChooseMode(FlowMode mode,string worldRealmId=null)
        {
            // World Rift is the only mode with a non-canonical map, so it must name a realm;
            // otherwise EffectiveMap is "" and GameModeRegistry.IsValidMap would reject the match.
            if(mode==FlowMode.WorldRift&&string.IsNullOrEmpty(worldRealmId)) return false;
            Session.mode=mode; Session.worldRiftRealmId=worldRealmId;
            Session.isCompetitive=mode==FlowMode.RankedSolo||mode==FlowMode.RankedFlex;
            Session.mapId=Session.EffectiveMap; Session.matchSeed=DateTime.UtcNow.Ticks;
            return Fsm.Transition(GameState.Queue);
        }
        public bool MatchFound()=>Fsm.Transition(GameState.ChampSelect);
        public bool LockChampion(string championId){ Session.selectedChampionId=championId; return Fsm.Transition(GameState.Loading); }
        public bool BeginMatch()=>Fsm.Transition(GameState.InMatch);

        /// <summary>Run the match headless to completion and advance to PostGame.</summary>
        public MatchResult RunMatch(MatchLoop loop, Func<float,(float blue,float red)> pressureAt, float dt=1f)
        {
            loop.Start();
            while(loop.Running) { var (b,r)=pressureAt(loop.Clock); loop.Tick(dt,b,r); }
            LastResult=loop.Result(); Fsm.Transition(GameState.PostGame); return LastResult;
        }

        /// <summary>Apply progression from the last result, persist, then return to menu.</summary>
        public ProgressionResult FinishMatch(bool firstWinOfDay)
        {
            LastProgression=ProgressionGlue.Apply(Profile,LastResult,Session.IsWorldRift,firstWinOfDay);
            _store.Save(Profile.playerId,Profile);
            Session.Reset();
            Fsm.Transition(GameState.MainMenu);
            return LastProgression;
        }
        public string CurrentScene=>SceneRouter.SceneFor(Fsm.Current);
    }
}

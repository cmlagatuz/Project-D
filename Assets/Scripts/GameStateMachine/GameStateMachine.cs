// Project D | Codename: Zantetsuken — GameStateMachine.cs (app flow states + legal transitions)
using System; using System.Collections.Generic;
namespace ProjectD.P15.StateMachine
{
    public enum GameState { Boot, MainMenu, ModeSelect, Queue, ChampSelect, Loading, InMatch, PostGame }
    public sealed class GameStateMachine
    {
        static readonly Dictionary<GameState,GameState[]> Legal=new()
        {
            [GameState.Boot]=new[]{GameState.MainMenu},
            [GameState.MainMenu]=new[]{GameState.ModeSelect},
            [GameState.ModeSelect]=new[]{GameState.Queue,GameState.MainMenu},
            [GameState.Queue]=new[]{GameState.ChampSelect,GameState.MainMenu},        // can cancel
            [GameState.ChampSelect]=new[]{GameState.Loading,GameState.Queue},          // can dodge
            [GameState.Loading]=new[]{GameState.InMatch},
            [GameState.InMatch]=new[]{GameState.PostGame},
            [GameState.PostGame]=new[]{GameState.MainMenu,GameState.ModeSelect},       // continue or requeue
        };
        public GameState Current{get;private set;}=GameState.Boot;
        public event Action<GameState,GameState> OnTransition;
        public bool CanTransition(GameState to){ return Legal.TryGetValue(Current,out var next)&&Array.IndexOf(next,to)>=0; }
        public bool Transition(GameState to){ if(!CanTransition(to)) return false; var from=Current; Current=to; OnTransition?.Invoke(from,to); return true; }
        public void Reset(){ Current=GameState.Boot; }
    }
}

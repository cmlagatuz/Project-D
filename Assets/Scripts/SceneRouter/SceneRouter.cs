// Project D | Codename: Zantetsuken — SceneRouter.cs (state -> scene id + guards)
using System.Collections.Generic;
using ProjectD.P15.StateMachine;
namespace ProjectD.P15.Routing
{
    public static class SceneRouter
    {
        static readonly Dictionary<GameState,string> Scenes=new()
        {
            [GameState.Boot]="Boot", [GameState.MainMenu]="MainMenu", [GameState.ModeSelect]="ModeSelect",
            [GameState.Queue]="MainMenu", [GameState.ChampSelect]="ChampSelect", [GameState.Loading]="Loading",
            [GameState.InMatch]="Match", [GameState.PostGame]="PostGame",
        };
        public static string SceneFor(GameState s)=>Scenes.TryGetValue(s,out var scene)?scene:"MainMenu";
        /// <summary>Does moving to a state require a full scene load (vs an in-scene overlay)?</summary>
        public static bool RequiresSceneLoad(GameState from,GameState to)=>SceneFor(from)!=SceneFor(to);
    }
}

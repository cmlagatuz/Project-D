// Project D | Codename: Zantetsuken — DraftEngine.cs (pick/ban phase machine)
using System; using System.Collections.Generic;
namespace ProjectD.P5.Draft
{
    public enum DraftAction { Ban, Pick }
    public enum DraftTeam { Blue, Red }
    public struct DraftStep { public DraftTeam team; public DraftAction action; }
    public sealed class DraftEngine
    {
        // Standard tournament order: ban-ban-ban-ban-ban-ban, pick B,R,R,B,B,R,R,B... (simplified competitive).
        static readonly DraftStep[] Order = {
            new(){team=DraftTeam.Blue,action=DraftAction.Ban}, new(){team=DraftTeam.Red,action=DraftAction.Ban},
            new(){team=DraftTeam.Blue,action=DraftAction.Ban}, new(){team=DraftTeam.Red,action=DraftAction.Ban},
            new(){team=DraftTeam.Blue,action=DraftAction.Ban}, new(){team=DraftTeam.Red,action=DraftAction.Ban},
            new(){team=DraftTeam.Blue,action=DraftAction.Pick}, new(){team=DraftTeam.Red,action=DraftAction.Pick},
            new(){team=DraftTeam.Red,action=DraftAction.Pick}, new(){team=DraftTeam.Blue,action=DraftAction.Pick},
            new(){team=DraftTeam.Blue,action=DraftAction.Pick}, new(){team=DraftTeam.Red,action=DraftAction.Pick},
            new(){team=DraftTeam.Red,action=DraftAction.Pick}, new(){team=DraftTeam.Blue,action=DraftAction.Pick},
            new(){team=DraftTeam.Blue,action=DraftAction.Pick}, new(){team=DraftTeam.Red,action=DraftAction.Pick},
            new(){team=DraftTeam.Red,action=DraftAction.Pick}, new(){team=DraftTeam.Blue,action=DraftAction.Pick},
            new(){team=DraftTeam.Red,action=DraftAction.Pick}, new(){team=DraftTeam.Blue,action=DraftAction.Pick},
        };
        int _i;
        public readonly HashSet<string> Bans=new(); public readonly List<string> BluePicks=new(), RedPicks=new();
        public bool Complete=>_i>=Order.Length;
        public DraftStep? Current=>Complete?(DraftStep?)null:Order[_i];
        public bool IsChampionTaken(string champ)=>Bans.Contains(champ)||BluePicks.Contains(champ)||RedPicks.Contains(champ);
        public bool Submit(DraftTeam team, string champ)
        {
            if(Complete) return false; var step=Order[_i];
            if(step.team!=team||string.IsNullOrEmpty(champ)) return false;
            if(step.action==DraftAction.Pick && IsChampionTaken(champ)) return false; // can't pick taken
            if(step.action==DraftAction.Ban){ if(!Bans.Add(champ)) return false; }
            else { if(IsChampionTaken(champ)) return false; (team==DraftTeam.Blue?BluePicks:RedPicks).Add(champ); }
            _i++; return true;
        }
        public int StepIndex=>_i;
    }
}

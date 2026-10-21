// Project D | Codename: Zantetsuken — PostGameService.cs (MVP, rewards, results build) + MvpVoteTally
using System; using System.Collections.Generic;
namespace ProjectD.PostGame
{
    public static class PostGameService
    {
        public const int BaseWinXp=300, BaseLossXp=180, PerMinuteXp=12, FirstWinOfDayBonusKp=400, MvpBonusXp=60;
        public static PostGameResult Build(Team winningTeam, float durationSeconds, IReadOnlyList<PlayerMatchStats> blue, IReadOnlyList<PlayerMatchStats> red, string localPlayerId, HashSet<string> firstWinOfDayPlayers=null)
        {
            var r = new PostGameResult { WinningTeam=winningTeam, DurationSeconds=durationSeconds, Blue=new List<PlayerMatchStats>(blue), Red=new List<PlayerMatchStats>(red) };
            var local = Find(r, localPlayerId);
            r.Outcome = local==null ? MatchOutcome.Remake : (local.team==winningTeam ? MatchOutcome.Victory : MatchOutcome.Defeat);
            PlayerMatchStats best=null, bestLoser=null; float bs=float.MinValue, bls=float.MinValue;
            foreach(var p in r.AllPlayers()){ float s=p.PerformanceScore(); if(s>bs){bs=s;best=p;} if(p.team!=winningTeam && s>bls){bls=s;bestLoser=p;} }
            r.MvpPlayerId=best?.playerId; r.TeamMvpPlayerId=bestLoser?.playerId;
            int minutes=Math.Max(0,(int)(durationSeconds/60f));
            foreach(var p in r.AllPlayers()){
                bool won=p.team==winningTeam; int xp=(won?BaseWinXp:BaseLossXp)+minutes*PerMinuteXp; if(p.playerId==r.MvpPlayerId) xp+=MvpBonusXp;
                bool fw=won && firstWinOfDayPlayers!=null && firstWinOfDayPlayers.Contains(p.playerId);
                r.Rewards[p.playerId]=new MatchRewards{ playerId=p.playerId, accountXp=xp, bonusKp=fw?FirstWinOfDayBonusKp:0, firstWinOfDay=fw };
            }
            return r;
        }
        public static List<PlayerMatchStats> ScoreboardFor(PostGameResult r, Team team){ var l=new List<PlayerMatchStats>(team==Team.Blue?r.Blue:r.Red); l.Sort((a,b)=>b.PerformanceScore().CompareTo(a.PerformanceScore())); return l; }
        static PlayerMatchStats Find(PostGameResult r, string id){ foreach(var p in r.AllPlayers()) if(p.playerId==id) return p; return null; }
    }
    public sealed class MvpVoteTally
    {
        readonly Dictionary<string,int> _votes=new(); readonly HashSet<string> _voters=new();
        public bool CastVote(string voterId, string targetId){ if(string.IsNullOrEmpty(voterId)||string.IsNullOrEmpty(targetId)) return false; if(_voters.Contains(voterId)) return false; _voters.Add(voterId); _votes.TryGetValue(targetId,out int c); _votes[targetId]=c+1; return true; }
        public int VotesFor(string id)=>_votes.TryGetValue(id,out var c)?c:0;
        public string Winner(){ string b=null; int bc=-1; foreach(var kv in _votes) if(kv.Value>bc){bc=kv.Value;b=kv.Key;} return b; }
        public int TotalVotes=>_voters.Count;
    }
}

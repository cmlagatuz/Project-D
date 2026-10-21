// Project D | Codename: Zantetsuken — ClanWar.cs (scheduled clan-vs-clan seasons)
using System; using System.Collections.Generic;
namespace ProjectD.P12.ClanWars
{
    public enum WarState { Scheduled, Active, Ended }
    public sealed class ClanEntry { public string clanId; public int warRating=1000; public int warPoints; public int wins, losses; }
    public sealed class ClanWarSeason
    {
        public string id; public long startUnix, endUnix; public WarState state=WarState.Scheduled;
        readonly Dictionary<string,ClanEntry> _clans=new();
        public void Register(string clanId,int rating){ if(!_clans.ContainsKey(clanId)) _clans[clanId]=new ClanEntry{clanId=clanId,warRating=rating}; }
        public ClanEntry Get(string id)=>_clans.TryGetValue(id,out var c)?c:null;
        public void Advance(long now){ if(state==WarState.Scheduled&&now>=startUnix) state=WarState.Active; if(state==WarState.Active&&now>=endUnix) state=WarState.Ended; }
        /// <summary>War points for a battle outcome, scaled by opponent strength (upset bonus).</summary>
        public static int PointsFor(bool win,int myRating,int oppRating)
        { if(!win) return 0; int baseP=30; int diff=oppRating-myRating; return Math.Max(15,baseP+diff/20); }
        public void ReportBattle(string clanId,bool win,int oppRating){ var c=Get(clanId); if(c==null||state!=WarState.Active) return; int p=PointsFor(win,c.warRating,oppRating); c.warPoints+=p; if(win){ c.wins++; c.warRating+=15; } else { c.losses++; c.warRating=Math.Max(0,c.warRating-12); } }
        /// <summary>War-rating-based matchmaking: nearest opponent within a widening window.</summary>
        public string FindOpponent(string clanId,int window=150)
        { var me=Get(clanId); if(me==null) return null; string best=null; int bestDiff=int.MaxValue; foreach(var c in _clans.Values){ if(c.clanId==clanId) continue; int d=Math.Abs(c.warRating-me.warRating); if(d<=window&&d<bestDiff){ bestDiff=d; best=c.clanId; } } return best; }
        public List<ClanEntry> Leaderboard(){ var l=new List<ClanEntry>(_clans.Values); l.Sort((a,b)=>b.warPoints!=a.warPoints?b.warPoints-a.warPoints:b.warRating-a.warRating); return l; }
    }
}

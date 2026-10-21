// Project D | Codename: Zantetsuken — ChampionLeaderboard.cs (per-champion Server No.1)
using System; using System.Collections.Generic;
namespace ProjectD.LeaderboardX
{
    public struct ChampionRating { public string PlayerId, DisplayName, ChampionId, Region, RankLabel; public int Rating, Games, Wins; public long LastPlayedUnix; }
    public struct ChampionTopEntry { public string ChampionId; public int Rank; public ChampionRating Entry; }
    public static class ChampionLeaderboard
    {
        public static Dictionary<string,ChampionTopEntry> ServerNumberOne(IReadOnlyList<ChampionRating> pool, string region=null)
        {
            var best=new Dictionary<string,ChampionRating>();
            foreach(var r in pool){ if(!string.IsNullOrEmpty(region)&&!string.Equals(r.Region,region,StringComparison.OrdinalIgnoreCase)) continue; if(!best.TryGetValue(r.ChampionId,out var cur)||Better(r,cur)) best[r.ChampionId]=r; }
            var res=new Dictionary<string,ChampionTopEntry>(best.Count);
            foreach(var kv in best) res[kv.Key]=new ChampionTopEntry{ChampionId=kv.Key,Rank=1,Entry=kv.Value};
            return res;
        }
        public static List<ChampionTopEntry> TopForChampion(IReadOnlyList<ChampionRating> pool, string championId, int topN=100, string region=null)
        {
            var list=new List<ChampionRating>();
            foreach(var r in pool){ if(r.ChampionId!=championId) continue; if(!string.IsNullOrEmpty(region)&&!string.Equals(r.Region,region,StringComparison.OrdinalIgnoreCase)) continue; list.Add(r); }
            list.Sort((a,b)=>Better(a,b)?-1:(Better(b,a)?1:0));
            var outl=new List<ChampionTopEntry>();
            for(int i=0;i<Math.Min(topN,list.Count);i++) outl.Add(new ChampionTopEntry{ChampionId=championId,Rank=i+1,Entry=list[i]});
            return outl;
        }
        static bool Better(in ChampionRating a, in ChampionRating b){ if(a.Rating!=b.Rating) return a.Rating>b.Rating; if(a.Games!=b.Games) return a.Games>b.Games; if(a.LastPlayedUnix!=b.LastPlayedUnix) return a.LastPlayedUnix>b.LastPlayedUnix; return string.CompareOrdinal(a.PlayerId??"",b.PlayerId??"")<0; }
    }
}

// Project D | Codename: Zantetsuken — ChampionMastery.cs
using System; using System.Collections.Generic;
namespace ProjectD.Mastery
{
    public sealed class MasteryEntry { public string championId; public long points; public int level=1, tokens; }
    public static class ChampionMastery
    {
        static readonly long[] G={0,1800,4200,7800,12600}; public const int MaxPointLevel=5;
        public static int PointsForMatch(float perf,bool win)=>Math.Max(20,(int)(perf*8f)+(win?60:20));
        public static MasteryEntry GetOrCreate(Dictionary<string,MasteryEntry> map,string id){ if(!map.TryGetValue(id,out var e)){ e=new MasteryEntry{championId=id}; map[id]=e; } return e; }
        public static int AddPoints(MasteryEntry e,int p){ if(p<=0) return 0; e.points+=p; int g=0; while(e.level<MaxPointLevel&&e.points>=G[e.level]){ e.level++; g++; } return g; }
        public static bool GrantToken(MasteryEntry e){ if(e.level<MaxPointLevel) return false; e.tokens++; if(e.tokens>=2+(e.level-MaxPointLevel)){ e.tokens=0; e.level++; return true; } return false; }
        public static long PointsToNext(MasteryEntry e)=>e.level<MaxPointLevel?Math.Max(0,G[e.level]-e.points):0;
    }
}

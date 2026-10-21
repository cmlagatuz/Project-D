// Project D | Codename: Zantetsuken — RotationScheduler.cs (weekly free rotation, deterministic)
using System; using System.Collections.Generic;
namespace ProjectD.P11.Rotation
{
    public static class RotationScheduler
    {
        /// <summary>Deterministically pick the free-rotation champions for a given week from the roster.</summary>
        public static List<string> FreeRotation(IReadOnlyList<string> roster,int weekNumber,int count)
        {
            var picks=new List<string>(); if(roster==null||roster.Count==0) return picks;
            var pool=new List<string>(roster); pool.Sort(StringComparer.Ordinal);
            uint h=(uint)weekNumber*2654435761u ^ 0x9E3779B9u;
            var used=new HashSet<int>();
            for(int i=0;i<count&&i<pool.Count;i++){ int idx; int guard=0; do{ h^=h<<13; h^=h>>17; h^=h<<5; idx=(int)(h%(uint)pool.Count); guard++; } while(used.Contains(idx)&&guard<64); used.Add(idx); picks.Add(pool[idx]); }
            return picks;
        }
        public static bool IsFreeThisWeek(IReadOnlyList<string> roster,int weekNumber,int count,string championId)
            => FreeRotation(roster,weekNumber,count).Contains(championId);
        /// <summary>A separate, newbie-friendly rotation (lower-difficulty pool) for accounts under a level.</summary>
        public static List<string> NewPlayerRotation(IReadOnlyList<string> easyRoster,int weekNumber,int count)
            => FreeRotation(easyRoster,weekNumber+1000,count);
    }
}

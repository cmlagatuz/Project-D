// Project D | Codename: Zantetsuken — ChampionRecommender.cs (suggest champs to try)
using System; using System.Collections.Generic;
namespace ProjectD.P10.Recommend
{
    public struct ChampionSignal { public string championId, role; public int gamesPlayed; public float winRate; public int masteryLevel; }
    public struct Recommendation { public string championId; public float score; public string reason; }
    public static class ChampionRecommender
    {
        /// <summary>Score candidates by fit: fills a role gap, has upside win-rate, low games (novelty), decent mastery synergy.</summary>
        public static List<Recommendation> Recommend(IReadOnlyList<ChampionSignal> pool, IReadOnlyDictionary<string,int> roleCounts, int topN=3)
        {
            var outl=new List<Recommendation>();
            int maxRole=1; foreach(var kv in roleCounts) if(kv.Value>maxRole) maxRole=kv.Value;
            foreach(var c in pool)
            {
                roleCounts.TryGetValue(c.role,out int rc);
                float roleGap = 1f - (rc/(float)maxRole);            // under-played roles score higher
                float upside = Clamp01(c.winRate);                    // higher wr = comfort/upside
                float novelty = Clamp01((10-c.gamesPlayed)/10f);      // new champs to explore
                float synergy = Clamp01(c.masteryLevel/5f)*0.3f;
                float score = roleGap*0.4f + upside*0.3f + novelty*0.2f + synergy;
                string reason = roleGap>0.5f?$"Fills your under-played {c.role} role" : upside>0.6f?"You win often on this style" : novelty>0.6f?"Fresh pick to expand your pool" : "Good fit for your profile";
                outl.Add(new Recommendation{championId=c.championId,score=score,reason=reason});
            }
            outl.Sort((a,b)=>b.score.CompareTo(a.score));
            if(outl.Count>topN) outl.RemoveRange(topN,outl.Count-topN);
            return outl;
        }
        static float Clamp01(float v)=>v<0?0:(v>1?1:v);
    }
}

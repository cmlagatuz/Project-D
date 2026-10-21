// Project D | Codename: Zantetsuken — ContentRecommender.cs (next-best content by interest)
using System; using System.Collections.Generic;
namespace ProjectD.P10.ContentRec
{
    public enum ContentType { Mission, Event, Mode, Tutorial, SocialActivity }
    public sealed class ContentItem { public string id, name; public ContentType type; public string[] tags=new string[0]; public int rewardValue; }
    public struct InterestProfile { public Dictionary<string,float> tagAffinity; public bool isNewPlayer; public float churnRisk; }
    public static class ContentRecommender
    {
        /// <summary>Rank content by tag affinity + reward + safety nudges (new player -> tutorials; churn -> quick wins).</summary>
        public static List<(ContentItem item,float score,string reason)> Recommend(IReadOnlyList<ContentItem> pool, in InterestProfile p, int topN=3)
        {
            var outl=new List<(ContentItem,float,string)>();
            foreach(var c in pool)
            {
                float affinity=0f; if(p.tagAffinity!=null) foreach(var tag in c.tags) if(p.tagAffinity.TryGetValue(tag,out float a)) affinity+=a;
                float reward=Clamp01(c.rewardValue/1000f)*0.3f;
                float score=affinity*0.5f+reward;
                string reason="Matches your recent activity";
                if(p.isNewPlayer && c.type==ContentType.Tutorial){ score+=0.5f; reason="Learn the ropes"; }
                if(p.churnRisk>=0.5f && c.rewardValue>=300){ score+=0.4f; reason="Quick, rewarding pick"; }
                outl.Add((c,score,reason));
            }
            outl.Sort((a,b)=>b.Item2.CompareTo(a.Item2));
            if(outl.Count>topN) outl.RemoveRange(topN,outl.Count-topN);
            return outl;
        }
        static float Clamp01(float v)=>v<0?0:(v>1?1:v);
    }
}

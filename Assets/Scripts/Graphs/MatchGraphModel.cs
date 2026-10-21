// Project D | Codename: Zantetsuken — MatchGraphModel.cs (gold/xp graphs + breakdown)
using System; using System.Collections.Generic;
namespace ProjectD.Graphs
{
    public enum Team { Blue, Red }
    public enum GoldSource { Minions, Kills, Assists, Objectives, Structures, Passive, Items }
    public enum XpSource { Minions, Kills, Assists, Objectives, Sharing }
    public struct GraphSample { public float TimeSeconds; public int BlueValue, RedValue; public int Diff=>BlueValue-RedValue; }
    public sealed class MatchGraphModel
    {
        readonly List<GraphSample> _gold=new(), _xp=new();
        public IReadOnlyList<GraphSample> Gold=>_gold; public IReadOnlyList<GraphSample> Xp=>_xp;
        public void SampleGold(float t,int b,int r)=>_gold.Add(new GraphSample{TimeSeconds=t,BlueValue=b,RedValue=r});
        public void SampleXp(float t,int b,int r)=>_xp.Add(new GraphSample{TimeSeconds=t,BlueValue=b,RedValue=r});
        public int PeakGoldLeadBlue(){ int p=0; foreach(var s in _gold) p=Math.Max(p,s.Diff); return p; }
        public int PeakGoldLeadRed(){ int p=0; foreach(var s in _gold) p=Math.Max(p,-s.Diff); return p; }
        public int FinalGoldDiff=>_gold.Count==0?0:_gold[_gold.Count-1].Diff;
    }
    public sealed class EconomyBreakdown
    {
        readonly Dictionary<GoldSource,int> _g=new(); readonly Dictionary<XpSource,int> _x=new();
        public void AddGold(GoldSource s,int a){ _g.TryGetValue(s,out int c); _g[s]=c+a; }
        public void AddXp(XpSource s,int a){ _x.TryGetValue(s,out int c); _x[s]=c+a; }
        public int GoldFrom(GoldSource s)=>_g.TryGetValue(s,out var c)?c:0;
        public int TotalGold(){ int t=0; foreach(var v in _g.Values) t+=v; return t; }
        public Dictionary<GoldSource,float> GoldFractions(){ int tot=TotalGold(); var f=new Dictionary<GoldSource,float>(); if(tot<=0) return f; foreach(var kv in _g) f[kv.Key]=(float)kv.Value/tot; return f; }
    }
}

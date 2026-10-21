// Project D | Codename: Zantetsuken — WorldMap.cs (regions with shifting control)
using System; using System.Collections.Generic;
namespace ProjectD.P13.WorldMap
{
    public enum Faction { None, Order, Chaos, Balance }
    public sealed class Region { public string id, name; public Faction controller=Faction.None; public readonly Dictionary<Faction,int> influence=new(); public int Total(){ int t=0; foreach(var v in influence.Values) t+=v; return t; } }
    public sealed class WorldMap
    {
        readonly Dictionary<string,Region> _regions=new(); public readonly List<string> ChangeLog=new();
        public void AddRegion(Region r){ if(r!=null&&!string.IsNullOrEmpty(r.id)) _regions[r.id]=r; }
        public Region Get(string id)=>_regions.TryGetValue(id,out var r)?r:null;
        public void Contribute(string regionId,Faction f,int amount){ var r=Get(regionId); if(r==null||f==Faction.None||amount<=0) return; r.influence.TryGetValue(f,out int v); r.influence[f]=v+amount; }
        /// <summary>Recompute controllers: a faction controls a region if it holds >50% influence.</summary>
        public List<string> Resolve()
        {
            var flips=new List<string>();
            foreach(var r in _regions.Values)
            {
                int total=r.Total(); if(total==0) continue;
                Faction lead=Faction.None; int best=0; foreach(var kv in r.influence) if(kv.Value>best){ best=kv.Value; lead=kv.Key; }
                Faction newController = best*2>total ? lead : Faction.None;   // needs majority
                if(newController!=r.controller){ ChangeLog.Add($"{r.id}: {r.controller} -> {newController}"); r.controller=newController; flips.Add(r.id); }
            }
            return flips;
        }
        public int RegionsControlledBy(Faction f){ int n=0; foreach(var r in _regions.Values) if(r.controller==f) n++; return n; }
        public Faction DominantFaction(){ var counts=new Dictionary<Faction,int>(); foreach(var r in _regions.Values) if(r.controller!=Faction.None){ counts.TryGetValue(r.controller,out int c); counts[r.controller]=c+1; } Faction best=Faction.None; int bv=0; foreach(var kv in counts) if(kv.Value>bv){ bv=kv.Value; best=kv.Key; } return best; }
    }
}

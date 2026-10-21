// Project D | Codename: Zantetsuken — Emporium.cs (rotating featured cosmetics + bundles)
using System; using System.Collections.Generic;
namespace ProjectD.P9.Emporium
{
    public enum CosmeticKind { Skin, Border, Icon, Emote, Ward, Recall }
    public sealed class CosmeticOffer { public string id, name; public CosmeticKind kind; public int basePriceKp; public int discountPct; public bool featured; public string[] bundleItemIds=new string[0]; public int EffectivePrice=>(int)Math.Round(basePriceKp*(1-discountPct/100.0)); }
    public sealed class Emporium
    {
        readonly Dictionary<string,CosmeticOffer> _offers=new();
        public void Register(CosmeticOffer o){ if(o!=null&&!string.IsNullOrEmpty(o.id)) _offers[o.id]=o; }
        public CosmeticOffer Get(string id)=>_offers.TryGetValue(id,out var o)?o:null;
        public List<CosmeticOffer> Featured(){ var o=new List<CosmeticOffer>(); foreach(var v in _offers.Values) if(v.featured) o.Add(v); return o; }
        /// <summary>Deterministic daily rotation: pick N offers for a given day seed.</summary>
        public List<CosmeticOffer> DailyRotation(int daySeed,int count)
        {
            var all=new List<CosmeticOffer>(_offers.Values); all.Sort((a,b)=>string.CompareOrdinal(a.id,b.id));
            var picks=new List<CosmeticOffer>(); if(all.Count==0) return picks;
            uint h=(uint)daySeed*2654435761u;
            var used=new HashSet<int>();
            for(int i=0;i<count&&i<all.Count;i++){ int idx; int guard=0; do{ h^=h<<13; h^=h>>17; h^=h<<5; idx=(int)(h%(uint)all.Count); guard++; } while(used.Contains(idx)&&guard<50); used.Add(idx); picks.Add(all[idx]); }
            return picks;
        }
        /// <summary>Bundle price = discounted sum of contents (extra bundle discount applied).</summary>
        public int BundlePrice(string bundleId){ var b=Get(bundleId); if(b==null) return 0; if(b.bundleItemIds.Length==0) return b.EffectivePrice; int sum=0; foreach(var id in b.bundleItemIds){ var it=Get(id); if(it!=null) sum+=it.EffectivePrice; } return (int)Math.Round(sum*(1-b.discountPct/100.0)); }
    }
}

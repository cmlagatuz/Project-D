// Project D | Codename: Zantetsuken — ItemForge.cs (build tree + recipe validation)
using System; using System.Collections.Generic;
namespace ProjectD.P8.ItemForge
{
    public sealed class ItemDef { public string id, name; public int totalCost; public string[] componentIds=new string[0]; public bool isBasic=>componentIds==null||componentIds.Length==0; }
    public sealed class ItemCatalog
    {
        readonly Dictionary<string,ItemDef> _items=new();
        public void Register(ItemDef d){ if(d!=null&&!string.IsNullOrEmpty(d.id)) _items[d.id]=d; }
        public ItemDef Get(string id)=>_items.TryGetValue(id,out var d)?d:null;
        public bool Exists(string id)=>_items.ContainsKey(id);
        /// <summary>Recipe cost = sum of leaf component costs (build path). Detects missing components.</summary>
        public int RecipeCost(string id, out List<string> missing)
        {
            missing=new List<string>(); return Sum(id,missing,new HashSet<string>());
        }
        int Sum(string id,List<string> missing,HashSet<string> visiting)
        {
            var d=Get(id); if(d==null){ missing.Add(id); return 0; }
            if(d.isBasic) return d.totalCost;
            if(!visiting.Add(id)) return 0; // cycle guard
            int c=0; foreach(var comp in d.componentIds) c+=Sum(comp,missing,visiting); visiting.Remove(id); return c;
        }
        public bool CanAfford(string id,int gold){ var c=RecipeCost(id,out var missing); return missing.Count==0 && gold>=c; }
        /// <summary>Upgrade cost = total minus the value of components already owned.</summary>
        public int UpgradeCost(string id, ISet<string> owned)
        {
            var d=Get(id); if(d==null||d.isBasic) return d?.totalCost??0;
            int cost=RecipeCost(id,out _); foreach(var comp in d.componentIds) if(owned.Contains(comp)){ var cd=Get(comp); if(cd!=null) cost-=cd.totalCost; }
            return Math.Max(0,cost);
        }
    }
}

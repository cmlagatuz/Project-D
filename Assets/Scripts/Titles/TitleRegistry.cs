// Project D | Codename: Zantetsuken — TitleRegistry.cs (earnable titles + one active)
using System.Collections.Generic;
namespace ProjectD.P9.Titles
{
    public enum TitleRarity { Common, Rare, Epic, Legendary }
    public sealed class TitleDef { public string id, text; public TitleRarity rarity; public string unlockKey; }
    public sealed class TitleRegistry
    {
        readonly Dictionary<string,TitleDef> _defs=new(); readonly HashSet<string> _owned=new(); public string ActiveTitleId{get;private set;}
        public void Register(TitleDef d){ if(d!=null&&!string.IsNullOrEmpty(d.id)) _defs[d.id]=d; }
        public bool Grant(string id){ if(!_defs.ContainsKey(id)) return false; return _owned.Add(id); }
        public bool Owns(string id)=>_owned.Contains(id);
        public bool SetActive(string id){ if(!_owned.Contains(id)) return false; ActiveTitleId=id; return true; }
        public string ActiveText=>ActiveTitleId!=null&&_defs.TryGetValue(ActiveTitleId,out var d)?d.text:"";
        public IEnumerable<TitleDef> Owned(){ foreach(var id in _owned) if(_defs.TryGetValue(id,out var d)) yield return d; }
        public int OwnedCount=>_owned.Count;
    }
}

// Project D | Codename: Zantetsuken — CollectionAlbum.cs (collectibles + set bonuses)
using System.Collections.Generic;
namespace ProjectD.P11.Collections
{
    public sealed class CollectionSet { public string id, name; public readonly List<string> itemIds=new(); public string setBonusCosmeticId; }
    public sealed class Album
    {
        readonly Dictionary<string,CollectionSet> _sets=new(); readonly HashSet<string> _owned=new(); readonly HashSet<string> _claimedBonuses=new();
        public void RegisterSet(CollectionSet s){ if(s!=null&&!string.IsNullOrEmpty(s.id)) _sets[s.id]=s; }
        public void Collect(string itemId){ if(!string.IsNullOrEmpty(itemId)) _owned.Add(itemId); }
        public bool Owns(string itemId)=>_owned.Contains(itemId);
        public int OwnedInSet(string setId){ if(!_sets.TryGetValue(setId,out var s)) return 0; int n=0; foreach(var id in s.itemIds) if(_owned.Contains(id)) n++; return n; }
        public bool SetComplete(string setId){ if(!_sets.TryGetValue(setId,out var s)) return false; foreach(var id in s.itemIds) if(!_owned.Contains(id)) return false; return true; }
        public float SetProgress(string setId){ if(!_sets.TryGetValue(setId,out var s)||s.itemIds.Count==0) return 0f; return OwnedInSet(setId)/(float)s.itemIds.Count; }
        /// <summary>Claim a completed set's cosmetic bonus (idempotent). Returns the bonus id or null.</summary>
        public string ClaimSetBonus(string setId){ if(!SetComplete(setId)) return null; if(_claimedBonuses.Contains(setId)) return null; _claimedBonuses.Add(setId); return _sets[setId].setBonusCosmeticId; }
        public int TotalOwned=>_owned.Count;
    }
}

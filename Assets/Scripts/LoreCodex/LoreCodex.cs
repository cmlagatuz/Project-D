// Project D | Codename: Zantetsuken — LoreCodex.cs (discovery-driven encyclopedia + sets)
using System.Collections.Generic;
namespace ProjectD.P13.Lore
{
    public sealed class LoreEntry { public string id, title, body, category; public string[] linkedEntryIds=new string[0]; public bool discovered; }
    public sealed class LoreSet { public string id, name; public readonly List<string> entryIds=new(); public string completionRewardId; }
    public sealed class LoreCodex
    {
        readonly Dictionary<string,LoreEntry> _entries=new(); readonly Dictionary<string,LoreSet> _sets=new(); readonly HashSet<string> _claimedSets=new();
        public void AddEntry(LoreEntry e){ if(e!=null&&!string.IsNullOrEmpty(e.id)) _entries[e.id]=e; }
        public void AddSet(LoreSet s){ if(s!=null&&!string.IsNullOrEmpty(s.id)) _sets[s.id]=s; }
        public bool Discover(string id){ if(!_entries.TryGetValue(id,out var e)||e.discovered) return false; e.discovered=true; return true; }
        public IEnumerable<string> LinkedFrom(string id)=>_entries.TryGetValue(id,out var e)?e.linkedEntryIds:new string[0];
        public int DiscoveredCount(){ int n=0; foreach(var e in _entries.Values) if(e.discovered) n++; return n; }
        public int Total=>_entries.Count;
        public float SetProgress(string setId){ if(!_sets.TryGetValue(setId,out var s)||s.entryIds.Count==0) return 0f; int d=0; foreach(var id in s.entryIds) if(_entries.TryGetValue(id,out var e)&&e.discovered) d++; return (float)d/s.entryIds.Count; }
        public bool SetComplete(string setId)=>SetProgress(setId)>=1f;
        public string ClaimSetReward(string setId){ if(!SetComplete(setId)||_claimedSets.Contains(setId)) return null; _claimedSets.Add(setId); return _sets[setId].completionRewardId; }
    }
}

// Project D | Codename: Zantetsuken — AccountLinking.cs
using System.Collections.Generic;
namespace ProjectD.P4.Linking
{
    public enum Platform { Mobile, PC, Console }
    public sealed class LinkedAccount { public string masterId; readonly HashSet<Platform> _p=new(); public IReadOnlyCollection<Platform> Platforms=>_p; public bool Link(Platform p)=>_p.Add(p); public bool Unlink(Platform p){ if(_p.Count<=1) return false; return _p.Remove(p); } public bool IsLinked(Platform p)=>_p.Contains(p); }
    public static class ProgressionMerge
    {
        public static (HashSet<string> owned,long kp,int level) Merge(HashSet<string> a,long kpA,int lvlA,HashSet<string> b,long kpB,int lvlB){ var owned=new HashSet<string>(a); owned.UnionWith(b); return (owned, kpA>kpB?kpA:kpB, lvlA>lvlB?lvlA:lvlB); }
    }
}

// Project D | Codename: Zantetsuken — RuneSystem.cs (rune pages + path rules + stats)
using System.Collections.Generic;
namespace ProjectD.P8.Runes
{
    public enum RunePath { Precision, Domination, Sorcery, Resolve, Inspiration }
    public enum RuneStat { AttackDamage, AbilityPower, Armor, MagicResist, Health, MoveSpeed, CooldownReduction, LifeSteal }
    public sealed class Rune { public string id, name; public RunePath path; public bool keystone; public Dictionary<RuneStat,float> stats=new(); }
    public sealed class RunePage
    {
        public RunePath primaryPath, secondaryPath;
        public string keystoneId; public readonly List<string> primaryRunes=new(); public readonly List<string> secondaryRunes=new();
        public const int PrimarySlots=3, SecondarySlots=2;   // + 1 keystone
    }
    public static class RuneValidator
    {
        /// <summary>A page is valid if: keystone set, primary!=secondary, slot counts correct, all runes on their path.</summary>
        public static bool Validate(RunePage p, System.Func<string,Rune> lookup, out string error)
        {
            error=null;
            if(p.primaryPath==p.secondaryPath){ error="Primary and secondary paths must differ."; return false; }
            if(string.IsNullOrEmpty(p.keystoneId)){ error="No keystone selected."; return false; }
            var ks=lookup(p.keystoneId); if(ks==null||!ks.keystone||ks.path!=p.primaryPath){ error="Keystone must be a keystone on the primary path."; return false; }
            if(p.primaryRunes.Count!=RunePage.PrimarySlots){ error=$"Primary needs {RunePage.PrimarySlots} runes."; return false; }
            if(p.secondaryRunes.Count!=RunePage.SecondarySlots){ error=$"Secondary needs {RunePage.SecondarySlots} runes."; return false; }
            foreach(var id in p.primaryRunes){ var r=lookup(id); if(r==null||r.path!=p.primaryPath){ error=$"Rune {id} not on primary path."; return false; } }
            foreach(var id in p.secondaryRunes){ var r=lookup(id); if(r==null||r.path!=p.secondaryPath){ error=$"Rune {id} not on secondary path."; return false; } }
            return true;
        }
        public static Dictionary<RuneStat,float> AggregateStats(RunePage p, System.Func<string,Rune> lookup)
        {
            var total=new Dictionary<RuneStat,float>();
            void Add(string id){ var r=lookup(id); if(r==null) return; foreach(var kv in r.stats){ total.TryGetValue(kv.Key,out float v); total[kv.Key]=v+kv.Value; } }
            Add(p.keystoneId); foreach(var id in p.primaryRunes) Add(id); foreach(var id in p.secondaryRunes) Add(id);
            return total;
        }
    }
}

// Project D | Codename: Zantetsuken — Experiments.cs
using System; using System.Collections.Generic;
namespace ProjectD.P4.Experiments
{
    public sealed class Experiment { public string key; public string[] variants={"control","treatment"}; public int[] weights; public bool enabled=true; }
    public sealed class ExperimentService
    {
        readonly Dictionary<string,Experiment> _exp=new(); readonly HashSet<string> _flags=new(); readonly HashSet<string> _exposed=new();
        public int ExposureCount=>_exposed.Count;
        public void SetFlag(string key,bool on){ if(on) _flags.Add(key); else _flags.Remove(key); } public bool IsFlagOn(string key)=>_flags.Contains(key);
        public void Register(Experiment e){ if(e!=null&&!string.IsNullOrEmpty(e.key)) _exp[e.key]=e; }
        public string Variant(string key,string userId){ if(!_exp.TryGetValue(key,out var e)||!e.enabled||e.variants.Length==0) return "control"; uint h=Hash(key+"|"+userId); if(e.weights==null||e.weights.Length!=e.variants.Length) return e.variants[(int)(h%(uint)e.variants.Length)]; int total=0; foreach(var w in e.weights) total+=w; int pick=(int)(h%(uint)Math.Max(1,total)); for(int i=0;i<e.variants.Length;i++){ pick-=e.weights[i]; if(pick<0) return e.variants[i]; } return e.variants[0]; }
        public bool LogExposure(string key,string userId)=>_exposed.Add(key+"|"+userId);
        static uint Hash(string s){ uint h=2166136261; foreach(char c in s){ h^=c; h*=16777619; } return h; }
    }
}

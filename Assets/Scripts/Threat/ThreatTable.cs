// Project D | Codename: Zantetsuken — ThreatTable.cs (per-attacker threat with decay)
using System.Collections.Generic;
namespace ProjectD.P6.Threat
{
    public sealed class ThreatTable
    {
        public float DecayPerSecond=5f; readonly Dictionary<string,float> _t=new();
        public void AddThreat(string a,float amt){ if(string.IsNullOrEmpty(a)||amt<=0) return; _t.TryGetValue(a,out float v); _t[a]=v+amt; }
        public void Tick(float dt){ var ks=new List<string>(_t.Keys); foreach(var k in ks){ float v=_t[k]-DecayPerSecond*dt; if(v<=0) _t.Remove(k); else _t[k]=v; } }
        public string Target(){ string best=null; float bv=0; foreach(var kv in _t) if(kv.Value>bv){ bv=kv.Value; best=kv.Key; } return best; }
        public float ThreatOf(string id)=>_t.TryGetValue(id,out var v)?v:0f;
        public void ForceTarget(string id,float big=100000f)=>AddThreat(id,big);
    }
}

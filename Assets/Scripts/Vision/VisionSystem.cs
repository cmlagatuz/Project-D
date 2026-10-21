// Project D | Codename: Zantetsuken — VisionSystem.cs (wards, caps, expiry, sweep)
using System; using System.Collections.Generic;
namespace ProjectD.P5.Vision
{
    public enum WardType { Stealth, Control, Farsight }
    public sealed class Ward { public string id, ownerId; public WardType type; public float x,z,placedAt,expiresAt; public bool IsExpired(float now)=>type!=WardType.Control && now>=expiresAt; }
    public sealed class VisionSystem
    {
        public int StealthCapPerPlayer=3, ControlCapPerPlayer=1;
        public float StealthDuration=90f, FarsightDuration=9999f;
        readonly List<Ward> _wards=new(); int _seq;
        public IReadOnlyList<Ward> Wards=>_wards;
        int CountOf(string owner,WardType t){ int n=0; foreach(var w in _wards) if(w.ownerId==owner&&w.type==t) n++; return n; }
        public Ward Place(string owner,WardType type,float x,float z,float now)
        {
            int cap=type==WardType.Control?ControlCapPerPlayer:type==WardType.Stealth?StealthCapPerPlayer:99;
            if(CountOf(owner,type)>=cap) return null;
            float dur=type==WardType.Stealth?StealthDuration:type==WardType.Farsight?FarsightDuration:0f;
            var w=new Ward{id="w"+(_seq++),ownerId=owner,type=type,x=x,z=z,placedAt=now,expiresAt=now+dur};
            _wards.Add(w); return w;
        }
        public bool Sweep(float x,float z,float radius){ int before=_wards.Count; _wards.RemoveAll(w=>{ float dx=w.x-x,dz=w.z-z; return dx*dx+dz*dz<=radius*radius; }); return _wards.Count<before; }
        public void Tick(float now){ _wards.RemoveAll(w=>w.IsExpired(now)); }
        public bool HasVisionAt(float x,float z,float radius,float now){ foreach(var w in _wards){ if(w.IsExpired(now)) continue; float dx=w.x-x,dz=w.z-z; if(dx*dx+dz*dz<=radius*radius) return true; } return false; }
    }
}

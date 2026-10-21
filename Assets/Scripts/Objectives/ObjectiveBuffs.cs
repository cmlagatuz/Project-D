// Project D | Codename: Zantetsuken — ObjectiveBuffs.cs (blessings + team buff timers)
using System; using System.Collections.Generic;
namespace ProjectD.P5.Objectives
{
    public enum Blessing { Infernal, Glacial, Tempest, Earthen, Judgment, Tidal, ZantetsukenEdge, DivineFavor }
    public sealed class ActiveBuff { public Blessing blessing; public int stacks=1; public float expiresAt; public bool permanent; }
    public sealed class TeamBuffs
    {
        readonly Dictionary<Blessing,ActiveBuff> _b=new();
        public void Grant(Blessing b, float now, float duration, bool permanent=false, int maxStacks=3)
        {
            if(_b.TryGetValue(b,out var e)){ if(!permanent) e.expiresAt=now+duration; e.stacks=Math.Min(maxStacks,e.stacks+1); e.permanent|=permanent; }
            else _b[b]=new ActiveBuff{blessing=b,expiresAt=now+duration,permanent=permanent,stacks=1};
        }
        public bool Has(Blessing b, float now){ if(!_b.TryGetValue(b,out var e)) return false; return e.permanent||e.expiresAt>now; }
        public int Stacks(Blessing b, float now)=>Has(b,now)?_b[b].stacks:0;
        /// <summary>Seconds left on the buff, or -1 if permanent (caller shows an infinity glyph).</summary>
        public float Remaining(Blessing b, float now){ if(!_b.TryGetValue(b,out var e)) return 0f; return e.permanent?-1f:Math.Max(0f,e.expiresAt-now); }
        public void Tick(float now){ var expired=new List<Blessing>(); foreach(var kv in _b) if(!kv.Value.permanent&&kv.Value.expiresAt<=now) expired.Add(kv.Key); foreach(var b in expired) _b.Remove(b); }
        public int ActiveCount(float now){ Tick(now); return _b.Count; }
    }
    public static class ObjectiveBounty
    {
        /// <summary>Team-fight bounty for slaying an objective, scaling with match minute.</summary>
        public static int Gold(Blessing b, int matchMinute)
        {
            int baseGold = b switch { Blessing.ZantetsukenEdge=>350, Blessing.DivineFavor=>300, _=>150 };
            return baseGold + matchMinute * 8;
        }
    }
}

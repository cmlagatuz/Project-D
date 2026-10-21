// Project D | Codename: Zantetsuken — Achievements.cs (tiered trackable goals)
using System; using System.Collections.Generic;
namespace ProjectD.P9.Achievements
{
    public sealed class AchievementDef { public string id, name, description; public long[] tiers=new long[0]; public int rewardKpPerTier; }
    public sealed class AchievementState { public string id; public long progress; public int claimedTier; }
    public sealed class AchievementSystem
    {
        readonly Dictionary<string,AchievementDef> _defs=new(); readonly Dictionary<string,AchievementState> _state=new();
        public void Register(AchievementDef d){ if(d==null||string.IsNullOrEmpty(d.id)) return; _defs[d.id]=d; if(!_state.ContainsKey(d.id)) _state[d.id]=new AchievementState{id=d.id}; }
        public void AddProgress(string id,long amount){ if(_state.TryGetValue(id,out var s)&&amount>0) s.progress+=amount; }
        public int CurrentTier(string id){ if(!_defs.TryGetValue(id,out var d)||!_state.TryGetValue(id,out var s)) return 0; int t=0; foreach(var gate in d.tiers) if(s.progress>=gate) t++; return t; }
        /// <summary>Claim all newly-completed tiers; returns KP awarded.</summary>
        public int Claim(string id)
        {
            if(!_defs.TryGetValue(id,out var d)||!_state.TryGetValue(id,out var s)) return 0;
            int cur=CurrentTier(id); if(cur<=s.claimedTier) return 0;
            int newTiers=cur-s.claimedTier; s.claimedTier=cur; return newTiers*d.rewardKpPerTier;
        }
        public int UnclaimedTiers(string id){ var s=_state.GetValueOrDefault(id); return s==null?0:Math.Max(0,CurrentTier(id)-s.claimedTier); }
        public bool IsMaxed(string id){ if(!_defs.TryGetValue(id,out var d)) return false; return CurrentTier(id)>=d.tiers.Length; }
    }
}

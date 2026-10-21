// Project D | Codename: Zantetsuken — AbilitySystem.cs (server-side cast validation)
using System.Collections.Generic; using ProjectD.Data; using ProjectD.Gameplay.Combat; using ProjectD.Core;
namespace ProjectD.Gameplay.Champions
{
    public enum CastResult { Success, OnCooldown, NoResource, Interrupted, InvalidTarget, NotLearned }
    public sealed class AbilitySystem
    {
        private readonly Dictionary<AbilitySlot,float> _cdReadyAt = new();
        private readonly Dictionary<AbilitySlot,int> _rank = new();
        public float CurrentResource; public float ServerTime;
        public void LearnOrRankUp(AbilitySlot s){ _rank.TryGetValue(s,out int r); _rank[s]=r+1; }
        public int RankOf(AbilitySlot s)=>_rank.TryGetValue(s,out int r)?r:0;
        public bool IsReady(AbilitySlot s)=>!_cdReadyAt.TryGetValue(s,out float t)||ServerTime>=t;
        public float CooldownRemaining01(AbilitySlot s,float baseCd,StatBlock st)
        { if(!_cdReadyAt.TryGetValue(s,out float t)) return 0f; float full=st.CooldownAfterHaste(baseCd); float rem=t-ServerTime; return full<=0f?0f:System.Math.Max(0f,System.Math.Min(1f,rem/full)); }
        public CastResult TryCast(AbilityDefinition a, StatBlock st)
        {
            if(a==null) return CastResult.InvalidTarget;
            if(a.slot!=AbilitySlot.Passive && RankOf(a.slot)==0) return CastResult.NotLearned;
            if(!IsReady(a.slot)) return CastResult.OnCooldown;
            if(CurrentResource<a.resourceCost) return CastResult.NoResource;
            CurrentResource-=a.resourceCost; _cdReadyAt[a.slot]=ServerTime+st.CooldownAfterHaste(a.baseCooldown);
            return CastResult.Success;
        }
        public float ResolveDamage(AbilityDefinition a, StatBlock st)
        {
            int rank=System.Math.Max(1,RankOf(a.slot)); int idx=System.Math.Min(rank-1,a.baseDamagePerRank.Length-1);
            float bd=a.baseDamagePerRank.Length>0?a.baseDamagePerRank[idx]:0f;
            float scale=a.damageType==DamageType.Magical?st.MagicPower:st.AttackDamage;
            return bd + a.damageRatio*scale;
        }
    }
}

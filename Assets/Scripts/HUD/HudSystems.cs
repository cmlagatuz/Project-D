// Project D | Codename: Zantetsuken — HudSystems.cs (recall bar, ability level-up, status legend, targeting)
using System; using System.Collections.Generic;
namespace ProjectD.HUD
{
    public sealed class RecallChannel
    {
        public float ChannelSeconds=8f; public bool Active{get;private set;} public float Elapsed{get;private set;} public bool Completed{get;private set;}
        public event Action OnCompleted, OnInterrupted;
        public void Begin(){ Active=true; Elapsed=0f; Completed=false; }
        public bool Tick(float dt){ if(!Active) return false; Elapsed+=dt; if(Elapsed>=ChannelSeconds){ Active=false; Completed=true; OnCompleted?.Invoke(); return true; } return false; }
        public void Interrupt(){ if(!Active) return; Active=false; Completed=false; OnInterrupted?.Invoke(); }
        public float Progress=>ChannelSeconds<=0?0f:Math.Min(1f,Elapsed/ChannelSeconds);
    }
    public enum AbilitySlot { Q, W, E, Ultimate }
    public sealed class AbilityLevelUp
    {
        public int ChampionLevel{get;private set;}=1; public int SkillPoints{get;private set;}=1;
        readonly Dictionary<AbilitySlot,int> _r=new(){{AbilitySlot.Q,0},{AbilitySlot.W,0},{AbilitySlot.E,0},{AbilitySlot.Ultimate,0}};
        public int MaxBasicRank=5, MaxUltRank=3; public int[] UltUnlockLevels={5,9,13};
        public int RankOf(AbilitySlot s)=>_r[s];
        public void OnChampionLevelUp(){ ChampionLevel++; SkillPoints++; }
        public bool CanRankUp(AbilitySlot s){ if(SkillPoints<=0) return false; int r=_r[s]; if(s==AbilitySlot.Ultimate){ if(r>=MaxUltRank) return false; int need=UltUnlockLevels[Math.Min(r,UltUnlockLevels.Length-1)]; return ChampionLevel>=need; } return r<MaxBasicRank; }
        public bool RankUp(AbilitySlot s){ if(!CanRankUp(s)) return false; _r[s]++; SkillPoints--; return true; }
    }
    public enum StatusEffect { Stun, Root, Silence, Slow, Knockup, Airborne, Shield, Heal, Burn, Exposed, Untargetable, Cleansed }
    public static class StatusEffectLegend
    {
        public static string NameKey(StatusEffect e)=>$"status.{e.ToString().ToLowerInvariant()}";
        public static string DescKey(StatusEffect e)=>$"status.{e.ToString().ToLowerInvariant()}.desc";
        public static string IconKey(StatusEffect e)=>$"icon_status_{e.ToString().ToLowerInvariant()}";
        public static string Desc(StatusEffect e)=>e switch{StatusEffect.Stun=>"Cannot move, attack, or cast.",StatusEffect.Root=>"Cannot move; can still attack/cast.",StatusEffect.Silence=>"Cannot cast abilities.",StatusEffect.Slow=>"Reduced movement speed.",StatusEffect.Knockup=>"Launched into the air, briefly disabled.",StatusEffect.Airborne=>"Displaced; effects that stop movement apply.",StatusEffect.Shield=>"Absorbs incoming damage.",StatusEffect.Heal=>"Restores health over time.",StatusEffect.Burn=>"Takes damage over time.",StatusEffect.Exposed=>"Takes increased damage.",StatusEffect.Untargetable=>"Cannot be targeted by attacks or abilities.",StatusEffect.Cleansed=>"Negative effects removed.",_=>""};
        public static IEnumerable<StatusEffect> All(){ foreach(StatusEffect e in Enum.GetValues(typeof(StatusEffect))) yield return e; }
    }
    public sealed class TargetingModel
    {
        public int CurrentTargetId{get;private set;}=-1;
        public void SetTarget(int id)=>CurrentTargetId=id; public void ClearTarget()=>CurrentTargetId=-1;
        public static bool InLastHitWindow(float minionHp,float attackDamage)=>minionHp<=attackDamage;
    }
}

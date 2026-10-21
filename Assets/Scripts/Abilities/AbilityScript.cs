// Project D | Codename: Zantetsuken — AbilityScript.cs (data-driven ability effects)
using System; using System.Collections.Generic;
namespace ProjectD.P6.Abilities
{
    public enum EffectKind { Damage, Heal, Shield, Slow, Stun, Dash, Buff, Summon }
    public struct AbilityEffect { public EffectKind kind; public float magnitude, duration, scaleAd, scaleAp; }
    public sealed class AbilityDefinition { public string id, name; public float baseCooldown, manaCost, range; public List<AbilityEffect> effects=new(); }
    public struct CasterStats { public float attackDamage, abilityPower; }
    public struct ResolvedEffect { public EffectKind kind; public float value, duration; }
    public static class AbilityResolver
    {
        public static List<ResolvedEffect> Resolve(AbilityDefinition def, in CasterStats s){ var o=new List<ResolvedEffect>(); foreach(var e in def.effects) o.Add(new ResolvedEffect{kind=e.kind,value=e.magnitude+e.scaleAd*s.attackDamage+e.scaleAp*s.abilityPower,duration=e.duration}); return o; }
        public static float TotalDamage(List<ResolvedEffect> effects){ float t=0; foreach(var e in effects) if(e.kind==EffectKind.Damage) t+=e.value; return t; }
    }
    public static class ComboValidator { public static bool CanExecute(IReadOnlyList<string> combo, Func<string,bool> ready, Func<string,bool> exists){ foreach(var id in combo) if(!exists(id)||!ready(id)) return false; return true; } }
}

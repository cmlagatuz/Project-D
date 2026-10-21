// Project D | Codename: Zantetsuken — BalanceTuning.cs (stat curves + patch diffs)
using System.Collections.Generic;
namespace ProjectD.P6.Balance
{
    public struct StatCurve { public float baseValue, perLevel; public float At(int level)=>baseValue+perLevel*(level-1); }
    public sealed class ChampionStats { public string championId; public StatCurve health, attackDamage, armor, magicResist, attackSpeed; public float HealthAt(int l)=>health.At(l); public float AdAt(int l)=>attackDamage.At(l); }
    public struct BalanceChange { public string target, field; public float before, after; public string patch; }
    public sealed class PatchDiff
    {
        public readonly List<BalanceChange> Changes=new();
        public void Record(string t,string f,float b,float a,string patch){ if(b!=a) Changes.Add(new BalanceChange{target=t,field=f,before=b,after=a,patch=patch}); }
        public string Summary(){ var sb=new System.Text.StringBuilder(); foreach(var c in Changes){ string ar=c.after>c.before?"^":"v"; sb.AppendLine($"{c.target}.{c.field}: {c.before} -> {c.after} {ar}"); } return sb.ToString(); }
        public int Count=>Changes.Count;
    }
}

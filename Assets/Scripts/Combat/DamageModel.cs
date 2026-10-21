// Project D | Codename: Zantetsuken — DamageModel.cs (armor/MR reduction, pen, crit, true)
using System;
namespace ProjectD.P5.Combat
{
    public enum DamageType { Physical, Magic, True }
    public struct DamageInstance { public DamageType type; public float amount; public float flatPen, percentPen; public bool crit; public float critMultiplier; }
    public static class DamageModel
    {
        /// <summary>Standard MOBA mitigation: reduction = resist/(resist+100). True damage ignores resist.</summary>
        public static float Mitigate(DamageInstance d, float armor, float magicResist)
        {
            float amount=d.amount; if(d.crit) amount*=Math.Max(1f,d.critMultiplier<=0?1.6f:d.critMultiplier);
            if(d.type==DamageType.True) return amount;
            float resist = d.type==DamageType.Physical?armor:magicResist;
            resist*= (1f - Clamp01(d.percentPen)); resist-=d.flatPen; if(resist<0) resist=0;
            float mult = 100f/(100f+resist);
            return amount*mult;
        }
        /// <summary>Effective HP: how much raw damage a pool of HP survives at a given resist.</summary>
        public static float EffectiveHp(float hp, float resist)=>hp*(1f+resist/100f);
        static float Clamp01(float v)=>v<0?0:(v>1?1:v);
    }
}

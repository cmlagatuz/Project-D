// Project D | Codename: Zantetsuken — DamageSystem.cs (pure, engine-agnostic, server-side)
using System; using ProjectD.Core;
namespace ProjectD.Gameplay.Combat
{
    public static class DamageSystem
    {
        public static float Mitigate(float raw, float resist)
        {
            if (resist >= 0f) return raw * (100f / (100f + resist));
            float amp = 2f - (100f / (100f - resist)); return raw * amp;
        }
        public static float Resolve(in DamageInstance dmg, float armor, float mr, float curHp, float maxHp)
        {
            float m = dmg.Type switch {
                DamageType.Physical => Mitigate(dmg.Amount, armor),
                DamageType.Magical  => Mitigate(dmg.Amount, mr),
                DamageType.True     => dmg.Amount,
                DamageType.Adaptive => Mitigate(dmg.Amount, Math.Min(armor, mr)),
                _ => dmg.Amount };
            if (dmg.ExecuteThreshold > 0f && maxHp > 0f && curHp / maxHp <= dmg.ExecuteThreshold) m += dmg.ExecuteBonus;
            return Math.Max(0f, m);
        }
    }
}

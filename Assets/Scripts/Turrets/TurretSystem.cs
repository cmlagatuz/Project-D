// Project D | Codename: Zantetsuken — TurretSystem.cs (plating gold + aggro rules)
using System;
namespace ProjectD.P5.Turrets
{
    public sealed class Turret
    {
        public int plating=5; public bool platingActive=true; public float health=5000, maxHealth=5000;
        public const int PlatingGold=125; public const int FirstTurretBonus=200;
        public float PlatingFallsAtMinute=14f;
        /// <summary>Break one plate if plating is active + damage threshold met; returns gold awarded.</summary>
        public int TryBreakPlate(float matchMinute){ if(!platingActive||plating<=0||matchMinute>=PlatingFallsAtMinute){ platingActive=matchMinute<PlatingFallsAtMinute; return 0; } plating--; if(plating==0) platingActive=false; return PlatingGold; }
        public void ExpirePlatingByTime(float matchMinute){ if(matchMinute>=PlatingFallsAtMinute) platingActive=false; }
        public bool IsDestroyed=>health<=0;
        public float Damage(float amount){ health=Math.Max(0,health-amount); return health; }
    }
    public static class TurretAggro
    {
        /// <summary>Turret targets: a champion who attacked an enemy champ under tower takes aggro over minions.</summary>
        public static bool ChampionTakesAggro(bool championAttackedEnemyChampUnderTower)=>championAttackedEnemyChampUnderTower;
    }
}

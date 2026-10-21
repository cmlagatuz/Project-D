// Project D | Codename: Zantetsuken — WorldModifierEngine.cs (map-wide rule -> concrete deltas)
using System;
namespace ProjectD.Worlds
{
    // Concrete, bounded gameplay parameters a world modifier applies map-wide. These are
    // MULTIPLIERS/deltas the sim reads; they are symmetric (affect both teams equally) so
    // the mode stays fair-within-the-match even though it varies match-to-match.
    public struct ModifierProfile
    {
        public float visionRadiusMult;   // Umbral shrinks, default 1
        public float dashRangeMult;      // LowGravity extends
        public float moveSpeedMult;      // Gale streams
        public float killGoldMult;       // WildRose bonus
        public float turretDamageMult;   // Dominion empowers structures
        public float periodicTeamHeal;   // Radiant heal per pulse (both teams)
        public bool  dayNightCycle;      // Twilight flips a bonus over time
        public bool  terrainShifts;      // Unstable brush/wall movement
        public bool  creepingRoots;      // Rootbound hazard zones
        public bool  periodicStatShuffle;// JobShift reallocation pulse
        public static ModifierProfile Default => new ModifierProfile{ visionRadiusMult=1f, dashRangeMult=1f, moveSpeedMult=1f, killGoldMult=1f, turretDamageMult=1f };
    }
    public static class WorldModifierEngine
    {
        public static ModifierProfile Resolve(WorldModifierType t)
        {
            var p=ModifierProfile.Default;
            switch(t)
            {
                case WorldModifierType.Radiant:    p.periodicTeamHeal=40f; break;
                case WorldModifierType.Unstable:   p.terrainShifts=true; break;
                case WorldModifierType.WildRose:   p.killGoldMult=1.25f; break;
                case WorldModifierType.Dominion:   p.turretDamageMult=1.35f; break;
                case WorldModifierType.JobShift:   p.periodicStatShuffle=true; break;
                case WorldModifierType.Umbral:     p.visionRadiusMult=0.7f; break;
                case WorldModifierType.Twilight:   p.dayNightCycle=true; break;
                case WorldModifierType.LowGravity: p.dashRangeMult=1.4f; break;
                case WorldModifierType.Gale:       p.moveSpeedMult=1.15f; break;
                case WorldModifierType.Rootbound:  p.creepingRoots=true; break;
            }
            return p;
        }
        public static string Describe(WorldModifierType t)=>t switch{
            WorldModifierType.Radiant=>"Radiant: pulsing sanctuary heals both teams.",
            WorldModifierType.Unstable=>"Unstable: terrain shifts unpredictably.",
            WorldModifierType.WildRose=>"Wild Rose: +25% gold on champion kills.",
            WorldModifierType.Dominion=>"Dominion: turrets strike 35% harder.",
            WorldModifierType.JobShift=>"Job Shift: periodic stat reallocation.",
            WorldModifierType.Umbral=>"Umbral: vision is dimmed across the realm.",
            WorldModifierType.Twilight=>"Twilight: day/night flips the empowered stat.",
            WorldModifierType.LowGravity=>"Low Gravity: dashes and leaps reach 40% farther.",
            WorldModifierType.Gale=>"Gale: wind currents grant +15% move speed.",
            WorldModifierType.Rootbound=>"Rootbound: creeping roots ensnare the careless.",
            _=>"" };
    }
}

// Project D  |  Codename: Zantetsuken
// MapEnums.cs  (Map — shared enums for the objective environment)
//
// The map's objective pits, lanes, and monster tiers. The summon pit is DYNAMIC:
// it sits in the pit next to the bottom duo (marksman + support) — which can be the
// top or bottom river pit depending on which side that duo occupies (like Wild Rift).
// The divine pit is always on the OPPOSITE side from the summon pit that match.

namespace ProjectD.Map
{
    public enum Team { Blue = 0, Red = 1, Neutral = 2 }

    // The two major river pits (top and bottom of the river).
    public enum RiverPit { Top = 0, Bottom = 1 }

    // What currently occupies a pit.
    public enum PitObjective { Summon, Divine }

    // Lanes/roles used to decide which pit the duo is near.
    public enum Lane { Top, Mid, Bottom, Jungle }
    public enum Role { Vanguard, Fighter, Assassin, Marksman, Mage, Support }

    // Jungle camp sizes (spawn cadence differs per size).
    public enum MonsterTier { Small, Medium, Large }
}

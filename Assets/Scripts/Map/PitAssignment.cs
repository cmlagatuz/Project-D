// Project D  |  Codename: Zantetsuken
// PitAssignment.cs  (Map — dynamic summon/divine pit placement)
//
// The summon pit is placed in the river pit NEXT TO the bottom duo (marksman +
// support). Which river pit that is (Top or Bottom) depends on which side of the
// map the duo occupies for the match. The divine manifestation pit is ALWAYS the
// opposite river pit. Determined once per match; pure C# + unit-testable.

namespace ProjectD.Map
{
    public readonly struct PitLayout
    {
        public readonly RiverPit SummonPit;   // where elemental summons + Odin spawn
        public readonly RiverPit DivinePit;   // where the divine manifestations spawn
        public PitLayout(RiverPit summonPit, RiverPit divinePit)
        { SummonPit = summonPit; DivinePit = divinePit; }

        public PitObjective ObjectiveAt(RiverPit pit) => pit == SummonPit ? PitObjective.Summon : PitObjective.Divine;
        public RiverPit Opposite(RiverPit pit) => pit == RiverPit.Top ? RiverPit.Bottom : RiverPit.Top;
    }

    public static class PitAssignment
    {
        /// <summary>Summon pit = the duo's river pit; divine pit = the opposite.</summary>
        public static PitLayout FromDuoPit(RiverPit duoRiverPit)
        {
            var divine = duoRiverPit == RiverPit.Top ? RiverPit.Bottom : RiverPit.Top;
            return new PitLayout(duoRiverPit, divine);
        }

        /// <summary>Bottom lane -> Bottom river pit; else Top.</summary>
        public static PitLayout FromDuoLane(Lane duoLane)
            => FromDuoPit(duoLane == Lane.Bottom ? RiverPit.Bottom : RiverPit.Top);

        /// <summary>Resolve from a team's role->lane map (finds the Marksman, else Support).</summary>
        public static PitLayout FromRoleLanes(System.Collections.Generic.IReadOnlyDictionary<Role, Lane> roleLanes)
        {
            Lane duoLane = Lane.Bottom;
            if (roleLanes != null)
            {
                if (roleLanes.TryGetValue(Role.Marksman, out var ml)) duoLane = ml;
                else if (roleLanes.TryGetValue(Role.Support, out var sl)) duoLane = sl;
            }
            return FromDuoLane(duoLane);
        }
    }
}

// Project D  |  Codename: Zantetsuken
// RankFormatter.cs  (Ranked — display strings for a rank)
//
// Formats a RankState into player-facing text and localization keys, e.g.
//   "Gold A - 75 LP", "Adamantite E - 30 LP", "Crystal A - 50 LP".
// Localization keys (rank.tier.*) are provided so the UI can translate tier names;
// division letters and LP are locale-neutral. Pure C# — unit-testable.

namespace ProjectD.Ranked
{
    public static class RankFormatter
    {
        public static string TierName(RankTier tier) => tier switch
        {
            RankTier.Bronze => "Bronze",
            RankTier.Silver => "Silver",
            RankTier.Gold => "Gold",
            RankTier.Platinum => "Platinum",
            RankTier.Mythril => "Mythril",
            RankTier.Adamantite => "Adamantite",
            RankTier.Crystal => "Crystal",
            _ => tier.ToString()
        };

        /// <summary>Localization key for the tier name, e.g. "rank.tier.mythril".</summary>
        public static string TierNameKey(RankTier tier) => "rank.tier." + tier.ToString().ToLowerInvariant();

        public static string DivisionLetter(Division d) => d.ToString(); // E/D/C/B/A

        /// <summary>e.g. "Gold A".</summary>
        public static string Short(RankState s) => $"{TierName(s.Tier)} {DivisionLetter(s.Division)}";

        /// <summary>e.g. "Gold A - 75 LP".</summary>
        public static string Full(RankState s) => $"{Short(s)} - {s.lp} LP";

        /// <summary>e.g. "75 / 100 LP" (uses the step's threshold).</summary>
        public static string LpOfThreshold(RankState s) => $"{s.lp} / {s.LpThreshold} LP";

        /// <summary>Progress within the current step, 0..1, for a bar.</summary>
        public static float StepProgress(RankState s)
            => s.LpThreshold <= 0 ? 0f : (float)s.lp / s.LpThreshold;

        /// <summary>Placement progress text, e.g. "Placement 3 / 7".</summary>
        public static string PlacementProgress(RankState s)
            => $"Placement {s.placementGamesPlayed} / {PlacementService.PlacementGames}";

        /// <summary>A hex-ish accent color per tier (for badges), as 0xRRGGBB.</summary>
        public static uint TierColor(RankTier tier) => tier switch
        {
            RankTier.Bronze => 0xB08D57,
            RankTier.Silver => 0xC0C0C8,
            RankTier.Gold => 0xE6B422,
            RankTier.Platinum => 0x3FB7B0,
            RankTier.Mythril => 0x7FA9E6,
            RankTier.Adamantite => 0x9B6BE0,
            RankTier.Crystal => 0x8FE3F0,
            _ => 0x888888
        };
    }
}

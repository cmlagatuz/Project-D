// Project D  |  Codename: Zantetsuken
// RankEnums.cs  (Ranked — tiers, divisions, results)
//
// The ranked ladder, low -> high:
//   Bronze, Silver, Gold, Platinum, Mythril, Adamantite, Crystal
// Divisions climb E (lowest) -> A (highest) within a tier. Not every tier uses all
// five divisions (see RankLadder): Bronze-Gold use D..A, the rest use E..A.
// Kept UnityEngine-free so all rank math is server-authoritative and unit-testable.

namespace ProjectD.Ranked
{
    // Ordered low -> high; the int value IS the climb order for tiers.
    public enum RankTier
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Platinum = 3,
        Mythril = 4,
        Adamantite = 5,
        Crystal = 6
    }

    // Division letters. Higher enum value = higher division (A is the top).
    // Stored this way so "climb" == increasing value, matching tiers.
    public enum Division
    {
        E = 0,
        D = 1,
        C = 2,
        B = 3,
        A = 4
    }

    public enum MatchResult { Loss = 0, Win = 1, Remake = 2 }
}

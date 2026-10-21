// Project D  |  Codename: Zantetsuken
// MatchupCallout.cs  (Loading — auto-pick the tip/trivia for your specific laner)
//
// Given the local champion, the enemy team, and the local player's lane, this finds
// the ENEMY LANER (same lane, or the closest match) and produces:
//   1) a targeted "how to play against this exact champion" callout card, and
//   2) the resolved first-encounter voice-line reference for that matchup.
//
// Pure C# (no UnityEngine) so it is deterministic and unit-testable. The screen
// uses it to front-load a personalized callout and to trigger the voice line.

using System.Collections.Generic;

namespace ProjectD.Loading
{
    public struct EnemyPick
    {
        public string ChampionId;
        public string Lane;         // Mid / Baron / Dragon / Support / Jungle (or "")
    }

    public struct MatchupResult
    {
        public bool HasLaner;
        public string EnemyChampionId;
        public string EnemyDisplayName;
        public string Lane;
        public TipCard Callout;      // the personalized "facing X" card
    }

    public static class MatchupCallout
    {
        /// <summary>
        /// Find the enemy laner and build the callout. lane is the local player's lane.
        /// If no same-lane enemy exists, falls back to the first enemy pick.
        /// </summary>
        public static MatchupResult Resolve(
            string localLane,
            IReadOnlyList<EnemyPick> enemyPicks,
            IReadOnlyDictionary<string, ChampionTipView> champions)
        {
            var result = new MatchupResult { HasLaner = false };
            if (enemyPicks == null || enemyPicks.Count == 0) return result;

            // 1) exact same-lane opponent; 2) otherwise the first enemy pick.
            EnemyPick chosen = default;
            bool found = false;
            if (!string.IsNullOrEmpty(localLane))
                foreach (var e in enemyPicks)
                    if (!string.IsNullOrEmpty(e.ChampionId) && e.Lane == localLane) { chosen = e; found = true; break; }
            if (!found)
                foreach (var e in enemyPicks)
                    if (!string.IsNullOrEmpty(e.ChampionId)) { chosen = e; found = true; break; }
            if (!found) return result;

            result.HasLaner = true;
            result.EnemyChampionId = chosen.ChampionId;
            result.Lane = chosen.Lane;

            string body;
            string name = chosen.ChampionId;
            if (champions != null && champions.TryGetValue(chosen.ChampionId, out var ec))
            {
                name = ec.DisplayName;
                // Prefer a real "playing against" tip; fall back to trivia, then a stub.
                if (ec.AgainstTips != null && ec.AgainstTips.Count > 0)
                    body = ec.AgainstTips[0];
                else if (ec.Trivia != null && ec.Trivia.Count > 0)
                    body = ec.Trivia[0];
                else
                    body = $"Study {name} before the fight.";
            }
            else
            {
                body = $"You'll be facing {name} this match.";
            }

            result.EnemyDisplayName = name;
            result.Callout = new TipCard
            {
                Kind = TipKind.MatchupCallout,
                Heading = string.IsNullOrEmpty(chosen.Lane)
                    ? $"YOUR OPPONENT — {name}"
                    : $"YOUR LANE ({chosen.Lane}) — {name}",
                Body = body,
                ChampionId = chosen.ChampionId
            };
            return result;
        }
    }
}

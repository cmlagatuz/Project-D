// Project D  |  Codename: Zantetsuken
// PostGameDemoDriver.cs  (PostGame UI — sandbox that feeds a sample match result)
//
// Builds a believable 5v5 PostGameResult (Blue wins) and presents it so you can see
// the Victory banner, both scoreboards, the MVP crown, cast an MVP vote, and view the
// rewards. In a real build the SERVER sends the authoritative result; the vote goes
// back to the server which tallies and returns the community MVP.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.PostGame;

namespace ProjectD.PostGameUI
{
    public sealed class PostGameDemoDriver : MonoBehaviour
    {
        [SerializeField] private PostGameResultsView view;
        [SerializeField] private string localPlayerId = "me";

        private void Start()
        {
            if (view == null) return;
            view.SetLocalPlayer(localPlayerId);

            var blue = new List<PlayerMatchStats>
            {
                Stat("me",    "kain",   Team.Blue, "Jungle",  12, 3, 8, 28000, 4200, 0,     14200, 210, 22, 3, 2),
                Stat("b_mid", "emperor",Team.Blue, "Mid",     8,  4, 9, 24000, 1800, 0,     13100, 235, 30, 2, 1),
                Stat("b_bar", "wol",    Team.Blue, "Baron",   3,  5, 14, 9000,  6100, 4200,  10500, 190, 26, 0, 1),
                Stat("b_dra", "firion", Team.Blue, "Dragon",  9,  6, 6, 26000, 900,  0,      12800, 245, 12, 1, 0),
                Stat("b_sup", "cecil",  Team.Blue, "Support", 1,  7, 20, 6000,  1200, 18500, 8200,  40,  55, 0, 0),
            };
            var red = new List<PlayerMatchStats>
            {
                Stat("r_jg",  "onion_knight", Team.Red, "Jungle",  7,  8, 6,  19000, 3200, 0,     11500, 195, 20, 1, 0),
                Stat("r_mid", "cloud_of_darkness", Team.Red, "Mid", 10, 6, 5, 27000, 1500, 0,     12900, 230, 24, 2, 1),
                Stat("r_bar", "garland", Team.Red, "Baron",   5,  7, 7,  15000, 5200, 3100,  11000, 205, 22, 0, 0),
                Stat("r_dra", "bartz",   Team.Red, "Dragon",  6,  9, 4,  21000, 800,  0,      11800, 240, 10, 1, 0),
                Stat("r_sup", "exdeath", Team.Red, "Support", 2,  8, 12, 7000,  1400, 14000, 8500,  55,  48, 0, 0),
            };

            var firstWin = new HashSet<string> { "me" };  // demo: local player's first win of day
            var result = PostGameService.Build(Team.Blue, durationSeconds: 1875f, blue, red, localPlayerId, firstWin);

            view.OnLocalVote += id => Debug.Log($"[Project D] MVP vote cast for {id} (sent to server).");
            view.OnContinue += _ => Debug.Log("[Project D] Continue -> hand off match XP to account leveling / return to menu.");
            view.Present(result);
        }

        private static PlayerMatchStats Stat(string id, string champ, Team team, string lane,
            int k, int d, int a, int dmg, int objDmg, int heal, int gold, int cs, int vision, int turrets, int mk)
            => new PlayerMatchStats
            {
                playerId = id, displayName = Pretty(id), championId = champ, team = team, lane = lane,
                kills = k, deaths = d, assists = a, championDamage = dmg, objectiveDamage = objDmg,
                healingShielding = heal, goldEarned = gold, minionsSlain = cs, visionScore = vision,
                turretsDestroyed = turrets, largestMultiKill = mk
            };

        private static string Pretty(string id) => id == "me" ? "You" : id.Replace("_", " ");
    }
}

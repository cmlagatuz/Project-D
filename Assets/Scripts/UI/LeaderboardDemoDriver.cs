// Project D  |  Codename: Zantetsuken
// LeaderboardDemoDriver.cs  (Leaderboard UI — play-in-editor sandbox)
//
// Feeds the LeaderboardScreen a deterministic mock apex pool so you can browse the
// board and try the Server/Friends scopes and the tier/champion/lane filters. In a
// real build, replace the mock pool with the server's ladder snapshot fetch.

using UnityEngine;
using ProjectD.Leaderboard;

namespace ProjectD.LeaderboardUI
{
    public sealed class LeaderboardDemoDriver : MonoBehaviour
    {
        [SerializeField] private LeaderboardScreen screen;
        [SerializeField] private string viewerId = "you";
        [SerializeField] private int entryCount = 60;

        private void Start()
        {
            var pool = MockApexData.Generate(viewerId, entryCount);
            if (screen) screen.InitializeLocal(viewerId, pool);
        }
    }
}

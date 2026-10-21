// Project D | Codename: Zantetsuken — EngagementValue.cs (engagement score + healthy caps)
using System;
namespace ProjectD.P10.LTV
{
    public struct EngagementInputs { public int sessionsLast30; public int minutesLast30; public int friendsPlayedWith; public int daysActiveLast30; public int seasonsPlayed; }
    public static class EngagementValue
    {
        /// <summary>Non-monetary engagement score 0..100 (breadth + consistency + social).</summary>
        public static int Score(in EngagementInputs x)
        {
            float sessions = Clamp01(x.sessionsLast30/40f)*30f;
            float minutes = Clamp01(x.minutesLast30/1200f)*25f;
            float days = Clamp01(x.daysActiveLast30/30f)*25f;
            float social = Clamp01(x.friendsPlayedWith/10f)*10f;
            float loyalty = Clamp01(x.seasonsPlayed/6f)*10f;
            return (int)Math.Round(sessions+minutes+days+social+loyalty);
        }
        public static string Tier(int score)=>score>=80?"Champion":score>=60?"Core":score>=35?"Casual":score>=15?"Returning":"Lapsing";
        /// <summary>Healthy-play cap: flag if a player is over a daily-hours threshold (wellbeing).</summary>
        public static bool ExceedsHealthyPlay(int minutesToday,int capMinutes=360)=>minutesToday>capMinutes;

        // Local engine-agnostic clamp (no UnityEngine reference in this file, so no Mathf.Clamp01).
        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}

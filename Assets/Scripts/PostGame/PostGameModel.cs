// Project D | Codename: Zantetsuken — PostGameModel.cs (scoreboard, stats, MVP, rewards)
using System; using System.Collections.Generic;
namespace ProjectD.PostGame
{
    public enum MatchOutcome { Victory, Defeat, Remake }
    public enum Team { Blue, Red }
    [Serializable] public sealed class PlayerMatchStats
    {
        public string playerId, displayName, championId, lane; public Team team;
        public int kills, deaths, assists, championDamage, objectiveDamage, healingShielding, goldEarned, minionsSlain, visionScore, largestMultiKill, turretsDestroyed;
        public float Kda => deaths==0 ? (kills+assists) : (float)(kills+assists)/deaths;
        public float PerformanceScore() => kills*3f + assists*1.5f - deaths*1.5f + championDamage/250f + objectiveDamage/400f + healingShielding/300f + goldEarned/500f + visionScore*0.8f + turretsDestroyed*2f + largestMultiKill*1.5f;
    }
    [Serializable] public sealed class MatchRewards { public string playerId; public int accountXp, bonusKp; public bool firstWinOfDay; }
    public sealed class PostGameResult
    {
        public MatchOutcome Outcome; public Team WinningTeam; public float DurationSeconds;
        public List<PlayerMatchStats> Blue=new(), Red=new();
        public string MvpPlayerId, TeamMvpPlayerId;
        public Dictionary<string,MatchRewards> Rewards=new();
        public IEnumerable<PlayerMatchStats> AllPlayers(){ foreach(var p in Blue) yield return p; foreach(var p in Red) yield return p; }
    }
}

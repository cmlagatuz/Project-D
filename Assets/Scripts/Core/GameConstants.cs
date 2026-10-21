// Project D | Codename: Zantetsuken
// GameConstants.cs — tunable defaults; competitive values overridable via remote-config.
namespace ProjectD.Core
{
    public static class GameConstants
    {
        public const int PlayersPerTeam = 5;
        public const int LaneCount = 3;
        public const int TeamBlue = 0;
        public const int TeamRed = 1;
        public const int ElementalSummonPoolSize = 6;
        public const int ElementalSummonsPerMatch = 3;
        public const int ServerTickRate = 30;
        public const int SnapshotSendRate = 20;
        public const int MinimumFrameRate = 30;
        public const int PreferredFrameRate = 60;
        public const float AdaptiveToAttackDamage = 1.0f;
        public const float AdaptiveToMagicPower = 1.0f;
    }
    public enum Team { Blue = 0, Red = 1, Neutral = 2 }
    public enum ChampionRole { Vanguard, Fighter, Assassin, Marksman, Mage, Support }
    public enum DamageType { Physical, Magical, True, Adaptive }
}

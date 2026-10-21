// Project D | Codename: Zantetsuken — BotDifficulty.cs (difficulty tiers)
namespace ProjectD.P6.Difficulty
{
    public enum DifficultyTier { Beginner, Easy, Medium, Hard, Expert }
    public struct DifficultyProfile { public float reactionSeconds, skillshotAccuracy, econMultiplier, aggression; }
    public static class BotDifficulty
    {
        public static DifficultyProfile For(DifficultyTier t)=>t switch{
            DifficultyTier.Beginner=>new(){reactionSeconds=0.9f,skillshotAccuracy=0.35f,econMultiplier=0.8f,aggression=0.3f},
            DifficultyTier.Easy=>new(){reactionSeconds=0.6f,skillshotAccuracy=0.5f,econMultiplier=0.9f,aggression=0.45f},
            DifficultyTier.Medium=>new(){reactionSeconds=0.4f,skillshotAccuracy=0.65f,econMultiplier=1.0f,aggression=0.6f},
            DifficultyTier.Hard=>new(){reactionSeconds=0.25f,skillshotAccuracy=0.8f,econMultiplier=1.1f,aggression=0.75f},
            _=>new(){reactionSeconds=0.12f,skillshotAccuracy=0.92f,econMultiplier=1.2f,aggression=0.9f}, };
        public static bool LandsSkillshot(DifficultyTier t,float roll)=>roll<=For(t).skillshotAccuracy;
    }
}

// Project D | Codename: Zantetsuken — ObjectiveEscalation.cs (objective power scales over time)
namespace ProjectD.P8.Objectives
{
    public enum ObjectiveStage { EarlyElemental, LateElemental, Elder }
    public static class ObjectiveEscalation
    {
        public const float LateElementalMinute=15f, ElderMinute=25f;
        public static ObjectiveStage StageAt(float minute)=>minute>=ElderMinute?ObjectiveStage.Elder:minute>=LateElementalMinute?ObjectiveStage.LateElemental:ObjectiveStage.EarlyElemental;
        /// <summary>Objective health scales with the match minute (tankier later).</summary>
        public static int Health(float minute)=>3000+(int)(minute*180f);
        /// <summary>Buff strength multiplier by stage (Elder is the game-ender).</summary>
        public static float BuffMultiplier(ObjectiveStage s)=>s switch{ObjectiveStage.Elder=>2.0f,ObjectiveStage.LateElemental=>1.4f,_=>1.0f};
        public static int TeamGold(float minute)=>150+(int)(minute*10f);
    }
}

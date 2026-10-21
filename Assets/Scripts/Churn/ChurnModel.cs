// Project D | Codename: Zantetsuken — ChurnModel.cs (explainable churn risk)
using System; using System.Collections.Generic;
namespace ProjectD.P10.Churn
{
    public struct ChurnInputs { public int daysSinceLastPlay; public int gamesLast7Days; public int currentLossStreak; public int friendsOnlineRecently; public float honorScore; public int accountAgeDays; }
    public struct ChurnResult { public float risk; public List<(string factor,float contribution)> factors; }
    public static class ChurnModel
    {
        /// <summary>Risk 0..1 blended from weighted, bounded factors. Higher = more likely to churn.</summary>
        public static ChurnResult Score(in ChurnInputs x)
        {
            var f=new List<(string,float)>();
            float recency = Clamp01(x.daysSinceLastPlay/14f) * 0.35f;                 // idle 14d -> full weight
            float frequency = Clamp01((5-x.gamesLast7Days)/5f) * 0.20f;               // <5 games/wk raises risk
            float tilt = Clamp01(x.currentLossStreak/5f) * 0.15f;                     // long loss streaks
            float social = Clamp01((3-x.friendsOnlineRecently)/3f) * 0.15f;           // few friends around
            float behavior = Clamp01((0.5f-x.honorScore)/0.5f) * 0.10f;              // low honor -> friction
            float newbie = (x.accountAgeDays<7?0.05f:0f);                            // early-life churn bump
            f.Add(("recency",recency)); f.Add(("frequency",frequency)); f.Add(("loss_streak",tilt));
            f.Add(("social",social)); f.Add(("behavior",behavior)); f.Add(("new_account",newbie));
            float risk=Clamp01(recency+frequency+tilt+social+behavior+newbie);
            f.Sort((a,b)=>b.Item2.CompareTo(a.Item2));
            return new ChurnResult{risk=risk, factors=f};
        }
        public static string Band(float risk)=>risk>=0.75f?"Critical":risk>=0.5f?"High":risk>=0.25f?"Moderate":"Low";
        static float Clamp01(float v)=>v<0?0:(v>1?1:v);
    }
}

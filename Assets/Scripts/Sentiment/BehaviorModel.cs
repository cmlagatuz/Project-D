// Project D | Codename: Zantetsuken — BehaviorModel.cs (behavior trend + health band)
using System;
namespace ProjectD.P10.Sentiment
{
    public sealed class BehaviorTrend
    {
        public float score=0.75f;              // 0..1 rolling behavior health
        public float Alpha=0.15f;              // EMA smoothing per game
        public void Report(int commends,int reports,bool positiveChat)
        {
            float sample = 0.6f + commends*0.1f - reports*0.2f + (positiveChat?0.1f:0f);
            sample = Clamp01(sample);
            score = Clamp01(score + Alpha*(sample-score));   // exponential moving average
        }
        public string Band=>score>=0.85f?"Exemplary":score>=0.65f?"Positive":score>=0.45f?"Neutral":score>=0.25f?"Watch":"At Risk";
        static float Clamp01(float v)=>v<0?0:(v>1?1:v);
    }
}

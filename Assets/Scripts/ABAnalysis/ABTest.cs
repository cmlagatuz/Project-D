// Project D | Codename: Zantetsuken — ABTest.cs (two-proportion significance)
using System;
namespace ProjectD.P10.ABAnalysis
{
    public struct ABResult { public float controlRate, treatmentRate, lift, zScore; public bool significant; public string verdict; }
    public static class ABTest
    {
        /// <summary>Two-proportion z-test. significant at |z|>=1.96 (~95% confidence).</summary>
        public static ABResult Evaluate(int controlConversions,int controlN,int treatmentConversions,int treatmentN)
        {
            var r=new ABResult();
            if(controlN<=0||treatmentN<=0) { r.verdict="Insufficient data"; return r; }
            float p1=controlConversions/(float)controlN, p2=treatmentConversions/(float)treatmentN;
            float pPool=(controlConversions+treatmentConversions)/(float)(controlN+treatmentN);
            float se=(float)Math.Sqrt(pPool*(1-pPool)*(1.0/controlN+1.0/treatmentN));
            r.controlRate=p1; r.treatmentRate=p2; r.lift=p1<=0?0:(p2-p1)/p1;
            r.zScore = se<=0?0:(p2-p1)/se;
            r.significant = Math.Abs(r.zScore)>=1.96f;
            r.verdict = !r.significant ? "No significant difference" : r.zScore>0 ? "Treatment wins" : "Control wins";
            return r;
        }
        /// <summary>Minimum sample per arm for a target detectable effect (rough rule-of-thumb).</summary>
        public static int RecommendedSamplePerArm(float baselineRate,float minDetectableLift)
        {
            float p=baselineRate, delta=Math.Max(0.001f,baselineRate*minDetectableLift);
            return (int)Math.Ceiling(16.0*p*(1-p)/(delta*delta));   // ~z=1.96,power=0.8 approximation
        }
    }
}

// Project D | Codename: Zantetsuken — ExperimentRollout.cs (staged ramp + kill-switch)
using System; using System.Collections.Generic;
namespace ProjectD.P12.Rollout
{
    public enum RolloutStage { Off, Canary, Quarter, Half, Full, KilledSwitch }
    public sealed class Rollout
    {
        public string key; public RolloutStage stage=RolloutStage.Off; public float guardrailThreshold=0.05f;   // max tolerated regression
        static readonly Dictionary<RolloutStage,float> Pct=new(){{RolloutStage.Off,0f},{RolloutStage.Canary,0.05f},{RolloutStage.Quarter,0.25f},{RolloutStage.Half,0.5f},{RolloutStage.Full,1f},{RolloutStage.KilledSwitch,0f}};
        public float Percent=>Pct[stage];
        /// <summary>Deterministic bucketing: is a user in the rollout at the current stage?</summary>
        public bool IsIn(string userId){ if(stage==RolloutStage.Off||stage==RolloutStage.KilledSwitch) return false; uint h=Hash(key+"|"+userId); float f=(h%10000)/10000f; return f<Percent; }
        /// <summary>Advance one stage IF the guardrail metric hasn't regressed beyond the threshold.</summary>
        public bool Advance(float guardrailRegression)
        {
            if(stage==RolloutStage.KilledSwitch) return false;
            if(guardrailRegression>guardrailThreshold){ stage=RolloutStage.KilledSwitch; return false; }   // auto kill-switch
            stage = stage switch{RolloutStage.Off=>RolloutStage.Canary,RolloutStage.Canary=>RolloutStage.Quarter,RolloutStage.Quarter=>RolloutStage.Half,RolloutStage.Half=>RolloutStage.Full,_=>RolloutStage.Full};
            return true;
        }
        public void Kill(){ stage=RolloutStage.KilledSwitch; }
        static uint Hash(string s){ uint h=2166136261; foreach(char c in s){ h^=c; h*=16777619; } return h; }
    }
}

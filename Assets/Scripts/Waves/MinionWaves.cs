// Project D | Codename: Zantetsuken — MinionWaves.cs (wave composition + push/freeze + cannon)
using System;
namespace ProjectD.P5.Waves
{
    public enum LanePush { Frozen, PushingBlue, PushingRed, Neutral }
    public struct Wave { public int melee, ranged, cannon, superMinion; public int Count=>melee+ranged+cannon+superMinion; }
    public static class MinionWaves
    {
        public const float WaveIntervalSeconds=30f; public const int CannonEveryNthWave=3;
        /// <summary>The wave that spawns at a given wave number (1-based). Cannon every 3rd; super after a seal falls.</summary>
        public static Wave Compose(int waveNumber, bool laneSealBroken)
        {
            var w=new Wave{melee=3,ranged=3};
            if(waveNumber%CannonEveryNthWave==0) w.cannon=1;
            if(laneSealBroken) w.superMinion=1;
            return w;
        }
        public static int CsGold(bool cannon)=>cannon?60:20;
        /// <summary>Net lane push from the minion-count difference (positive = blue ahead).</summary>
        public static LanePush PushState(int blueMinions,int redMinions,int deadzone=2)
        {
            int diff=blueMinions-redMinions;
            if(Math.Abs(diff)<=deadzone) return diff==0?LanePush.Frozen:LanePush.Neutral;
            return diff>0?LanePush.PushingRed:LanePush.PushingBlue; // more blue minions -> pushes into red
        }
        public static int WaveNumberAt(float matchSeconds)=>Math.Max(1,(int)(matchSeconds/WaveIntervalSeconds)+1);
    }
}

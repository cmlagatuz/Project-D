// Project D | Codename: Zantetsuken — FlowChannel.cs (adaptive PvE difficulty)
using System;
namespace ProjectD.P10.DynamicDifficulty
{
    // Keep the player in the "flow channel": challenged but not crushed. Adjusts a
    // difficulty level (0..4) from recent PvE outcomes toward a target win rate.
    public sealed class FlowChannel
    {
        public int MinLevel=0, MaxLevel=4; public float TargetWinRate=0.6f; public int Window=5;
        int _level=1; readonly System.Collections.Generic.Queue<bool> _recent=new();
        public int Level=>_level;
        public void ReportResult(bool win)
        {
            _recent.Enqueue(win); while(_recent.Count>Window) _recent.Dequeue();
            if(_recent.Count<Window) return;
            int wins=0; foreach(var w in _recent) if(w) wins++;
            float wr=wins/(float)_recent.Count;
            if(wr>TargetWinRate+0.15f) { _level=Math.Min(MaxLevel,_level+1); _recent.Clear(); }   // too easy -> harder
            else if(wr<TargetWinRate-0.15f) { _level=Math.Max(MinLevel,_level-1); _recent.Clear(); } // too hard -> easier
        }
        /// <summary>A continuous 0..1 handicap the bots apply within the current level (fine-grained).</summary>
        public float MicroAdjust(float lastMatchDurationSec,float expectedSec=1500f)
        {
            // Short stomps (player crushed the AI) nudge difficulty up a touch; long grinds nudge down.
            float ratio=lastMatchDurationSec/expectedSec;
            return Clamp01(1f-ratio); // faster win -> higher extra difficulty
        }
        static float Clamp01(float v)=>v<0?0:(v>1?1:v);
    }
}

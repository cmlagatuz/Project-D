// Project D | Codename: Zantetsuken — SeasonPipeline.cs (season lifecycle + soft reset)
using System;
namespace ProjectD.P8.Season
{
    public enum SeasonPhase { Preseason, Active, EndOfSeason, Offseason }
    public sealed class SeasonState { public int seasonId=1; public SeasonPhase phase=SeasonPhase.Active; public long startUnix, endUnix; public int peakRankStep; public bool rewardsClaimed; }
    public static class SeasonPipeline
    {
        public const double SoftResetPull=0.35;   // pull MMR toward the midpoint
        public static int SoftResetMmr(int prevMmr,int midpoint)=>(int)Math.Round(prevMmr+(midpoint-prevMmr)*SoftResetPull);
        public static SeasonPhase PhaseAt(long now,long start,long end,long preDays=7*86400,long offDays=7*86400)
        { if(now<start-preDays) return SeasonPhase.Offseason; if(now<start) return SeasonPhase.Preseason; if(now<=end) return SeasonPhase.Active; if(now<=end+offDays) return SeasonPhase.EndOfSeason; return SeasonPhase.Offseason; }
        /// <summary>Retro rewards need: reached a min rank step AND played a min number of games.</summary>
        public static bool RewardEligible(int peakRankStep,int gamesPlayed,int minStep=8,int minGames=20)=>peakRankStep>=minStep&&gamesPlayed>=minGames;
        public static SeasonState BeginNext(SeasonState prev,long now,long lengthSeconds)
        { return new SeasonState{seasonId=prev.seasonId+1,phase=SeasonPhase.Preseason,startUnix=now,endUnix=now+lengthSeconds}; }
    }
}

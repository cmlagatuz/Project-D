// Project D | Codename: Zantetsuken — SeasonalCycle.cs (weather/season cycle, presentation)
using System;
namespace ProjectD.P13.Seasonal
{
    public enum Season { Spring, Summer, Autumn, Winter }
    public enum Weather { Clear, Rain, Storm, Snow, Fog }
    public static class SeasonalCycle
    {
        public const long SecondsPerSeason=90L*86400;   // ~quarter-year seasons
        public static Season SeasonAt(long unix){ long idx=(unix/SecondsPerSeason)%4; return (Season)(int)idx; }
        /// <summary>Deterministic weather for a given day + region seed, weighted by season.</summary>
        public static Weather WeatherFor(long dayIndex,int regionSeed)
        {
            var season=SeasonAt(dayIndex*86400);
            uint h=(uint)(dayIndex*2654435761L ^ (regionSeed*40503L)); h^=h<<13; h^=h>>17; h^=h<<5;
            float r=(h%1000)/1000f;
            return season switch{
                Season.Winter => r<0.5f?Weather.Snow:r<0.8f?Weather.Clear:Weather.Fog,
                Season.Summer => r<0.6f?Weather.Clear:r<0.85f?Weather.Rain:Weather.Storm,
                Season.Autumn => r<0.4f?Weather.Fog:r<0.7f?Weather.Rain:Weather.Clear,
                _ => r<0.6f?Weather.Clear:Weather.Rain };
        }
        public static string AmbientBgmKey(Season s)=>$"bgm_ambient_{s.ToString().ToLowerInvariant()}";
    }
}

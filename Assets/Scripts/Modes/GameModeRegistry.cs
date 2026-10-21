// Project D | Codename: Zantetsuken — GameModeRegistry.cs (mode policy + fairness enforcement)
using System; using System.Collections.Generic;
namespace ProjectD.Modes
{
    public enum GameMode { RankedSolo, RankedFlex, NormalDraft, NormalBlind, CoOpVsAI, Practice, WorldRift }
    public enum MapPolicy { CanonicalOnly, RotatingWorld }
    public sealed class ModeDef { public GameMode mode; public MapPolicy mapPolicy; public bool rankedEligible; public bool awardsRankedLp; public int teamSize=5; public string ratingPool; }
    public static class GameModeRegistry
    {
        static readonly Dictionary<GameMode,ModeDef> _defs=new()
        {
            [GameMode.RankedSolo]=new(){mode=GameMode.RankedSolo,mapPolicy=MapPolicy.CanonicalOnly,rankedEligible=true,awardsRankedLp=true,ratingPool="ranked_solo"},
            [GameMode.RankedFlex]=new(){mode=GameMode.RankedFlex,mapPolicy=MapPolicy.CanonicalOnly,rankedEligible=true,awardsRankedLp=true,ratingPool="ranked_flex"},
            [GameMode.NormalDraft]=new(){mode=GameMode.NormalDraft,mapPolicy=MapPolicy.CanonicalOnly,rankedEligible=false,awardsRankedLp=false,ratingPool="normal"},
            [GameMode.NormalBlind]=new(){mode=GameMode.NormalBlind,mapPolicy=MapPolicy.CanonicalOnly,rankedEligible=false,awardsRankedLp=false,ratingPool="normal"},
            [GameMode.CoOpVsAI]=new(){mode=GameMode.CoOpVsAI,mapPolicy=MapPolicy.CanonicalOnly,rankedEligible=false,awardsRankedLp=false,ratingPool="coop"},
            [GameMode.Practice]=new(){mode=GameMode.Practice,mapPolicy=MapPolicy.CanonicalOnly,rankedEligible=false,awardsRankedLp=false,ratingPool="none"},
            [GameMode.WorldRift]=new(){mode=GameMode.WorldRift,mapPolicy=MapPolicy.RotatingWorld,rankedEligible=false,awardsRankedLp=false,ratingPool="worldrift_casual"},
        };
        public const string CanonicalMapId="crystalline_rift";
        public static ModeDef Get(GameMode m)=>_defs[m];
        /// <summary>THE FAIRNESS GUARDRAIL: is a (mode,map) pairing legal? Ranked/Normal must be canonical.</summary>
        public static bool IsValidMap(GameMode mode,string mapId)
        {
            var d=_defs[mode];
            if(d.mapPolicy==MapPolicy.CanonicalOnly) return mapId==CanonicalMapId;
            return mapId!=CanonicalMapId && !string.IsNullOrEmpty(mapId);   // World Rift uses a rotating (non-canonical) realm
        }
        /// <summary>Can this match award Ranked LP? Only ranked-eligible modes on the canonical map.</summary>
        public static bool CanAwardRankedLp(GameMode mode,string mapId)=>_defs[mode].awardsRankedLp && IsValidMap(mode,mapId);
        public static bool IsCompetitive(GameMode m)=>_defs[m].rankedEligible;
        public static IEnumerable<GameMode> ModesForCanonicalMap(){ foreach(var kv in _defs) if(kv.Value.mapPolicy==MapPolicy.CanonicalOnly) yield return kv.Key; }
    }
}

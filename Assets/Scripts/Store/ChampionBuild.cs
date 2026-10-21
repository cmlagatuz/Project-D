// Project D  |  Codename: Zantetsuken
// ChampionBuild.cs  (Store UI — serializable recommended build)
//
// A serializable per-champion recommended purchase order, used by the quick-buy row
// and the "next recommended" highlight. Populated from build_recommendations.seed.json
// (or assigned in the inspector). Order is: starter -> boots -> core (incl. exclusive)
// -> situational.

using System;

namespace ProjectD.UI.Store
{
    [Serializable]
    public sealed class ChampionBuild
    {
        public string championId;
        public string role;
        public string[] order = new string[0];
    }
}

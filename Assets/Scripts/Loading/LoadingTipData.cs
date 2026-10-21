// Project D  |  Codename: Zantetsuken
// LoadingTipData.cs  (Loading — champion tips + trivia data)
//
// Champion names/lore are Final Fantasy placeholders; all tip TEXT is original
// Project D copy (see docs/LICENSE-NOTICE.md).

using System;
using UnityEngine;

namespace ProjectD.Loading
{
    [CreateAssetMenu(fileName = "LoadingTips_", menuName = "Project D/Loading Tip Set")]
    public sealed class ChampionTipSet : ScriptableObject
    {
        public string championId;
        public string displayName;
        public string sourceWorld;
        public string role;
        public string defaultLane;                 // Mid / Baron / Dragon / Support / Jungle

        [TextArea(1, 3)] public string[] playingTips = new string[0];
        [TextArea(1, 3)] public string[] playingAgainstTips = new string[0];
        [TextArea(1, 3)] public string[] trivia = new string[0];
    }

    [CreateAssetMenu(fileName = "GeneralLoadingTips", menuName = "Project D/General Loading Tips")]
    public sealed class GeneralTipSet : ScriptableObject
    {
        [TextArea(1, 3)] public string[] tips = new string[0];
    }

    public enum TipKind { PlayingTip, AgainstTip, Trivia, General, MatchupCallout }

    [Serializable]
    public struct TipCard
    {
        public TipKind Kind;
        public string Heading;
        public string Body;
        public string ChampionId;
    }
}

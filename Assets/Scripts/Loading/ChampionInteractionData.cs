// Project D  |  Codename: Zantetsuken
// ChampionInteractionData.cs  (Loading — champion first-encounter voice lines)
//
// Per-champion voice interactions: a generic first-encounter line plus specific
// matchup lines keyed by enemy champion id. Each line references an AudioClip you
// supply (drop licensed .mp3/.wav/.ogg under GameData/Loading/Voice).
//
// RIGHTS: quote TEXT in the seed is ORIGINAL placeholder copy — NOT the Dissidia
// battle dialogue (Square Enix IP). Replace text + audio with licensed content.

using System;
using UnityEngine;

namespace ProjectD.Loading
{
    [Serializable]
    public sealed class VoiceLine
    {
        [Tooltip("Enemy champion id this line is for. Empty = generic first-encounter fallback.")]
        public string enemyChampionId = "";
        [Tooltip("Subtitle text shown while the line plays (original placeholder unless licensed).")]
        [TextArea(1, 2)] public string line;
        [Tooltip("Key of the audio file (GameData/Loading/Voice/<key>). Assign the AudioClip below.")]
        public string voiceClipKey;
        [Tooltip("The actual audio clip (drop your .mp3/.wav/.ogg import here).")]
        public AudioClip clip;
    }

    [CreateAssetMenu(fileName = "Interactions_", menuName = "Project D/Champion Interaction Set")]
    public sealed class ChampionInteractionSet : ScriptableObject
    {
        public string championId;
        public string displayName;

        [Tooltip("Played the first time this champion faces an enemy with no specific matchup line.")]
        public VoiceLine genericFirstEncounter = new VoiceLine();

        [Tooltip("Specific rivalry lines, keyed by enemy champion id (e.g. kain vs cecil).")]
        public VoiceLine[] matchups = new VoiceLine[0];

        /// <summary>Resolve the best line for a given enemy: specific matchup, else generic.</summary>
        public VoiceLine ResolveFor(string enemyChampionId)
        {
            if (!string.IsNullOrEmpty(enemyChampionId) && matchups != null)
                foreach (var m in matchups)
                    if (m != null && m.enemyChampionId == enemyChampionId) return m;
            return genericFirstEncounter;
        }
    }
}

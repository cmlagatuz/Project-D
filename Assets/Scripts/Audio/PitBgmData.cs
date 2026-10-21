// Project D  |  Codename: Zantetsuken
// PitBgmData.cs  (Audio — per-objective BGM clips)
//
// Each elemental summon, the elder (Odin), and each divine manifestation gets its
// OWN background music track. You import the audio files and assign them here; the
// proximity system plays the clip that matches whatever objective is currently in
// the pit the local player is near. Data-driven so audio designers set clips w/o code.
// The music is LOCAL to the player near the pit (ProximityBgmController) — not broadcast.

using System;
using UnityEngine;

namespace ProjectD.Audio
{
    [Serializable]
    public struct ObjectiveBgm
    {
        [Tooltip("Objective id, e.g. summon_ifrit, summon_odin, divine_materia.")]
        public string objectiveId;
        [Tooltip("Human label, e.g. 'Ifrit Theme'.")]
        public string displayName;
        [Tooltip("The imported music track (assign your audio file).")]
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Tooltip("Fade-in seconds when entering the pit.")]
        public float fadeInSeconds;
        [Tooltip("Fade-out seconds when leaving the pit.")]
        public float fadeOutSeconds;
        public bool loop;
    }

    [CreateAssetMenu(fileName = "PitBgmSet", menuName = "Project D/Pit BGM Set")]
    public sealed class PitBgmSet : ScriptableObject
    {
        [Header("Elemental summons + Odin")]
        public ObjectiveBgm[] summonThemes = new ObjectiveBgm[0];

        [Header("Divine manifestations")]
        public ObjectiveBgm[] divineThemes = new ObjectiveBgm[0];

        private System.Collections.Generic.Dictionary<string, ObjectiveBgm> _map;

        private void BuildIndex()
        {
            _map = new System.Collections.Generic.Dictionary<string, ObjectiveBgm>();
            foreach (var t in summonThemes) if (!string.IsNullOrEmpty(t.objectiveId)) _map[t.objectiveId] = t;
            foreach (var t in divineThemes) if (!string.IsNullOrEmpty(t.objectiveId)) _map[t.objectiveId] = t;
        }

        public bool TryGet(string objectiveId, out ObjectiveBgm bgm)
        {
            if (_map == null) BuildIndex();
            return _map.TryGetValue(objectiveId ?? "", out bgm);
        }

#if UNITY_EDITOR
        private void OnValidate() => _map = null;
#endif
    }
}

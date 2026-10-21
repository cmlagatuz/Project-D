// Project D  |  Codename: Zantetsuken
// VoiceLinePlayer.cs  (Loading — plays a champion's first-encounter voice line)
//
// Plays the local champion's voice line the FIRST time they face a given enemy,
// showing an optional subtitle. Tracks which (local, enemy) encounters have already
// fired so a line never repeats in a session. Wire it to the loading screen (play on
// the resolved laner) or to the in-match "first time you see this enemy" trigger.
//
// The clip is whatever AudioClip you assigned to the matchup (drop your licensed
// .mp3/.wav/.ogg import). Subtitle text is the (placeholder unless licensed) line.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.Loading
{
    public sealed class VoiceLinePlayer : MonoBehaviour
    {
        [SerializeField] private List<ChampionInteractionSet> interactionSets = new();
        [SerializeField] private AudioSource audioSource;
        [Header("Subtitle (optional)")]
        [SerializeField] private CanvasGroup subtitleGroup;
        [SerializeField] private Text subtitleText;
        [SerializeField] private float subtitleSeconds = 3.5f;
        [SerializeField] private float fadeSeconds = 0.3f;
        [Header("Options")]
        [Tooltip("If true, a specific (local vs enemy) line plays at most once per session.")]
        [SerializeField] private bool oncePerSession = true;

        private Dictionary<string, ChampionInteractionSet> _byId;
        private readonly HashSet<string> _fired = new();  // "local>enemy" keys
        private float _subTimer, _fade;

        public event System.Action<string, string> OnLinePlayed; // (localId, enemyId)

        private void Awake()
        {
            _byId = new Dictionary<string, ChampionInteractionSet>();
            foreach (var s in interactionSets) if (s && !string.IsNullOrEmpty(s.championId)) _byId[s.championId] = s;
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            if (subtitleGroup) subtitleGroup.alpha = 0f;
        }

        private void Update()
        {
            if (_subTimer > 0f)
            {
                _subTimer -= Time.deltaTime;
                if (_fade < 1f) { _fade = Mathf.Min(1f, _fade + Time.deltaTime / Mathf.Max(0.01f, fadeSeconds)); if (subtitleGroup) subtitleGroup.alpha = _fade; }
                if (_subTimer <= 0f) _fade = 0f;
            }
            else if (subtitleGroup && subtitleGroup.alpha > 0f)
            {
                subtitleGroup.alpha = Mathf.Max(0f, subtitleGroup.alpha - Time.deltaTime / Mathf.Max(0.01f, fadeSeconds));
            }
        }

        /// <summary>
        /// Play the local champion's first-encounter line for this enemy. Returns true
        /// if a line played (false if none found or already fired this session).
        /// </summary>
        public bool PlayEncounter(string localChampionId, string enemyChampionId)
        {
            if (string.IsNullOrEmpty(localChampionId)) return false;

            string key = localChampionId + ">" + (enemyChampionId ?? "");
            if (oncePerSession && _fired.Contains(key)) return false;

            if (_byId == null || !_byId.TryGetValue(localChampionId, out var set) || set == null) return false;

            var line = set.ResolveFor(enemyChampionId);
            if (line == null) return false;

            _fired.Add(key);

            if (line.clip != null) audioSource.PlayOneShot(line.clip);
            // else: no audio assigned yet — subtitle still shows so the matchup reads.

            if (subtitleText && !string.IsNullOrEmpty(line.line))
            {
                subtitleText.text = line.line;
                _subTimer = subtitleSeconds; _fade = 0f;
                if (subtitleGroup) subtitleGroup.alpha = 0f;
            }

            OnLinePlayed?.Invoke(localChampionId, enemyChampionId);
            return true;
        }

        /// <summary>Reset the "already fired" set (e.g. new match).</summary>
        public void ResetSession() => _fired.Clear();

        public void SetSets(List<ChampionInteractionSet> sets)
        {
            interactionSets = sets;
            _byId = new Dictionary<string, ChampionInteractionSet>();
            foreach (var s in sets) if (s && !string.IsNullOrEmpty(s.championId)) _byId[s.championId] = s;
        }
    }
}

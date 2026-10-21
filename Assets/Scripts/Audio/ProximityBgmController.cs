// Project D  |  Codename: Zantetsuken
// ProximityBgmController.cs  (Audio — play a pit's theme for the LOCAL player only)
//
// Each frame it checks the LOCAL player's position against the two river pits, asks
// PitProximity which objective theme should play, and crossfades to that clip on a
// local AudioSource (2D music, honoring the player's music-volume setting). When the
// player leaves the pit it fades back to the default map/lane music.
//
// Because this runs on each client for its OWN player only, a theme is heard just by
// players inside/near the pit — never broadcast to everyone. The pit's current
// objective is set by ObjectiveDirector.SetPitObjective on spawn/kill/rotate.

using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.Audio
{
    public sealed class ProximityBgmController : MonoBehaviour
    {
        [Header("Local player + pits")]
        [SerializeField] private Transform localPlayer;
        [SerializeField] private PitBgmSet bgmSet;

        [Header("Music sources (2D)")]
        [SerializeField] private AudioSource defaultMusic;
        [SerializeField] private AudioSource pitSourceA;
        [SerializeField] private AudioSource pitSourceB;

        [Header("Tuning")]
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float defaultDuckWhilePit = 0.15f;
        [SerializeField] private float pollInterval = 0.15f;

        private readonly Dictionary<string, PitZone> _pits = new();
        private string _activePitId;
        private string _activeObjectiveId;
        private AudioSource _current, _next, _fadeOutSource;
        private float _pollTimer;
        private float _targetPitVolume, _targetDefaultVolume = -1f;
        private float _fadeInSeconds = 0.5f, _fadeOutSeconds = 0.6f;

        private void Awake()
        {
            _current = pitSourceA; _next = pitSourceB;
            if (pitSourceA) { pitSourceA.spatialBlend = 0f; pitSourceA.playOnAwake = false; pitSourceA.volume = 0f; }
            if (pitSourceB) { pitSourceB.spatialBlend = 0f; pitSourceB.playOnAwake = false; pitSourceB.volume = 0f; }
        }

        public void SetLocalPlayer(Transform t) => localPlayer = t;
        public void SetMusicVolume(float v) => musicVolume = Mathf.Clamp01(v);

        public void ConfigurePit(string pitId, Vector3 center, float enterRadius, float exitRadius)
        {
            _pits[pitId] = new PitZone
            {
                PitId = pitId,
                ObjectiveId = _pits.TryGetValue(pitId, out var old) ? old.ObjectiveId : null,
                CenterX = center.x, CenterZ = center.z,
                EnterRadius = enterRadius, ExitRadius = Mathf.Max(exitRadius, enterRadius)
            };
        }

        public void SetPitObjective(string pitId, string objectiveId)
        {
            if (_pits.TryGetValue(pitId, out var z)) { z.ObjectiveId = objectiveId; _pits[pitId] = z; }
        }

        private void Update()
        {
            _pollTimer += Time.deltaTime;
            if (_pollTimer < pollInterval) { UpdateFades(); return; }
            _pollTimer = 0f;

            if (localPlayer == null || bgmSet == null) { UpdateFades(); return; }

            var zones = new List<PitZone>(_pits.Values);
            var pos = localPlayer.position;
            var result = PitProximity.Resolve(pos.x, pos.z, zones, _activePitId);

            if (result.HasTheme)
            {
                if (result.PitId != _activePitId || result.ObjectiveId != _activeObjectiveId)
                    SwitchTo(result.PitId, result.ObjectiveId);
            }
            else if (_activePitId != null)
            {
                SwitchTo(null, null);
            }

            UpdateFades();
        }

        private void SwitchTo(string pitId, string objectiveId)
        {
            _activePitId = pitId;
            _activeObjectiveId = objectiveId;

            if (string.IsNullOrEmpty(objectiveId) || !bgmSet.TryGet(objectiveId, out var bgm) || bgm.clip == null)
            {
                _targetPitVolume = 0f;
                _targetDefaultVolume = musicVolume;
                return;
            }

            var incoming = _next;
            incoming.clip = bgm.clip;
            incoming.loop = bgm.loop;
            incoming.volume = 0f;
            incoming.Play();

            _fadeOutSource = _current;
            _current = incoming;
            _next = _fadeOutSource;

            _targetPitVolume = Mathf.Clamp01(bgm.volume <= 0f ? 1f : bgm.volume) * musicVolume;
            _fadeInSeconds = Mathf.Max(0.01f, bgm.fadeInSeconds);
            _fadeOutSeconds = Mathf.Max(0.01f, bgm.fadeOutSeconds);
            _targetDefaultVolume = musicVolume * defaultDuckWhilePit;
        }

        private void UpdateFades()
        {
            float dt = Time.deltaTime;

            if (_current)
                _current.volume = Mathf.MoveTowards(_current.volume, _targetPitVolume, dt / _fadeInSeconds);

            if (_fadeOutSource && _fadeOutSource != _current)
            {
                _fadeOutSource.volume = Mathf.MoveTowards(_fadeOutSource.volume, 0f, dt / _fadeOutSeconds);
                if (_fadeOutSource.volume <= 0.001f && _fadeOutSource.isPlaying) _fadeOutSource.Stop();
            }

            if (defaultMusic && _targetDefaultVolume >= 0f)
                defaultMusic.volume = Mathf.MoveTowards(defaultMusic.volume, _targetDefaultVolume, dt / 0.5f);
        }
    }
}

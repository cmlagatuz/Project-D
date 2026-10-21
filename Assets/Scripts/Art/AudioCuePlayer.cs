// Project D  |  Codename: Zantetsuken
// AudioCuePlayer.cs  (Art runtime — turns cue keys into played SFX)
//
// Subscribe to an animator's OnSfxCue event. Resolves the key via the EffectCatalog
// and plays the AudioClip with the definition's volume/pitch/3D/priority. A small
// per-source voice limiter drops the lowest-priority active sound when the pool is
// full, so ults/deaths/low-HP cues always win over footsteps and minion chatter.
//
// Uses a pool of AudioSources parented to this object (3D positioned at the socket
// or the caster root). Non-3D cues (UI) play flat.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Art.Effects;

namespace ProjectD.Art.Effects
{
    public sealed class AudioCuePlayer : MonoBehaviour
    {
        [SerializeField] private EffectCatalog catalog;
        [SerializeField, Range(2, 32)] private int voices = 8;
        [SerializeField] private float masterVolume = 1f;

        private readonly List<Voice> _voices = new();

        private struct Voice { public AudioSource src; public int priority; }

        private void Awake()
        {
            for (int i = 0; i < voices; i++)
            {
                var go = new GameObject($"Voice_{i}");
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _voices.Add(new Voice { src = src, priority = -1 });
            }
        }

        public void SetCatalog(EffectCatalog c) => catalog = c;

        /// <summary>Hook to an animator: animator.OnSfxCue += audio.Play; (shares the vfx signature)</summary>
        public void Play(string key, string socket)
        {
            if (catalog == null || !catalog.TryResolve(key, out var def) || def.kind != EffectKind.Sfx) return;
            if (def.audioClip == null) return; // artist hasn't bound a clip yet

            var voice = AcquireVoice(def.priority);
            if (voice == null) return; // nothing lower-priority to steal

            var src = voice.Value.src;
            src.clip = def.audioClip;
            src.volume = def.volume * masterVolume;
            src.spatialBlend = def.is3D ? 1f : 0f;
            src.pitch = Random.Range(def.pitchRandom.x <= 0 ? 1f : def.pitchRandom.x,
                                     def.pitchRandom.y <= 0 ? 1f : def.pitchRandom.y);
            src.transform.position = ResolveSocket(socket).position;
            src.Play();

            SetVoicePriority(src, def.priority);
        }

        // ---- voice limiter ---- //
        private Voice? AcquireVoice(int priority)
        {
            // Free voice first.
            for (int i = 0; i < _voices.Count; i++)
                if (!_voices[i].src.isPlaying) return _voices[i];

            // Otherwise steal the lowest-priority active voice, if we outrank it.
            int lowIdx = -1, lowPrio = int.MaxValue;
            for (int i = 0; i < _voices.Count; i++)
                if (_voices[i].priority < lowPrio) { lowPrio = _voices[i].priority; lowIdx = i; }

            if (lowIdx >= 0 && priority >= lowPrio) return _voices[lowIdx];
            return null;
        }

        private void SetVoicePriority(AudioSource src, int priority)
        {
            for (int i = 0; i < _voices.Count; i++)
                if (_voices[i].src == src) { var v = _voices[i]; v.priority = priority; _voices[i] = v; break; }
        }

        private Transform ResolveSocket(string socket)
        {
            if (string.IsNullOrEmpty(socket) || socket == "root" || socket == "ground") return transform;
            string needle = socket.ToLowerInvariant();
            foreach (var t in GetComponentsInChildren<Transform>(true))
                if (t.name.ToLowerInvariant().Contains(needle)) return t;
            return transform;
        }
    }
}

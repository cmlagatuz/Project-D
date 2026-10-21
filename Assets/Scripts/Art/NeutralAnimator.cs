// Project D  |  Codename: Zantetsuken
// NeutralAnimator.cs  (Art runtime — summons, monsters, minions)
//
// Drives a neutral unit's animation: spawn -> idle/move -> basic attacks ->
// signature abilities (server-triggered) -> optional enrage -> death, and for
// objectives, the reward-grant flourish when the killing team claims the blessing.
// Like champions, all combat outcomes are server-authoritative; this only plays.

using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.Art.Animation
{
    [RequireComponent(typeof(Animator))]
    public sealed class NeutralAnimator : MonoBehaviour
    {
        [SerializeField] private NeutralAnimationSet set;

        private Animator _animator;
        private bool _enraged;

        public event System.Action<string, string> OnVfxCue; // key, socket

        private void Awake() => _animator = GetComponent<Animator>();
        public void SetAnimationSet(NeutralAnimationSet s) => set = s;

        public void PlaySpawn()
        {
            if (set != null && !string.IsNullOrEmpty(set.spawnClip))
                _animator.CrossFadeInFixedTime(set.spawnClip, 0.05f);
        }

        public void PlayBasicAttack(int index = 0)
        {
            if (set == null || set.basicAttacks == null || set.basicAttacks.Length == 0) return;
            var clip = set.basicAttacks[index % set.basicAttacks.Length];
            _animator.CrossFadeInFixedTime(clip, 0.05f);
        }

        public enum Sig { A, B, Ultimate }

        /// <summary>Server triggers a signature ability; fire its authored cues.</summary>
        public void PlaySignature(Sig which)
        {
            var anim = which switch
            {
                Sig.A => set?.signatureA,
                Sig.B => set?.signatureB,
                Sig.Ultimate => set?.signatureUltimate,
                _ => null
            };
            if (anim == null || string.IsNullOrEmpty(anim.clipName)) return;
            _animator.CrossFadeInFixedTime(anim.clipName, 0.05f);
            StartCoroutine(FireCues(anim));
        }

        private System.Collections.IEnumerator FireCues(AbilityAnimation anim)
        {
            float t = 0f, total = anim.TotalDuration;
            int vi = 0, si = 0;
            while (t < total)
            {
                t += Time.deltaTime;
                while (anim.vfxCues != null && vi < anim.vfxCues.Length && t >= anim.vfxCues[vi].time)
                { var c = anim.vfxCues[vi++]; OnVfxCue?.Invoke(c.key, c.socket); }
                while (anim.sfxCues != null && si < anim.sfxCues.Length && t >= anim.sfxCues[si].time)
                { var c = anim.sfxCues[si++]; OnVfxCue?.Invoke(c.key, c.socket); }
                yield return null;
            }
        }

        public void Enrage()
        {
            if (_enraged || set == null || string.IsNullOrEmpty(set.enrageClip)) return;
            _enraged = true;
            _animator.CrossFadeInFixedTime(set.enrageClip, 0.15f);
        }

        public void PlayDeath()
        {
            if (set == null) return;
            if (!string.IsNullOrEmpty(set.deathClip)) _animator.CrossFadeInFixedTime(set.deathClip, 0.05f);
            if (!string.IsNullOrEmpty(set.deathVfxKey)) OnVfxCue?.Invoke(set.deathVfxKey, "root");
        }

        /// <summary>Objectives only: the flourish that grants the blessing to the killers.</summary>
        public void PlayRewardGrant()
        {
            if (set == null) return;
            if (!string.IsNullOrEmpty(set.rewardGrantClip)) _animator.CrossFadeInFixedTime(set.rewardGrantClip, 0.1f);
            if (!string.IsNullOrEmpty(set.rewardGrantVfxKey)) OnVfxCue?.Invoke(set.rewardGrantVfxKey, "root");
        }
    }
}

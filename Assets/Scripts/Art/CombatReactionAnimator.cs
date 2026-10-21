// Project D  |  Codename: Zantetsuken
// CombatReactionAnimator.cs  (Art runtime — reactions, attacks, healing overlays)
//
// Plays SERVER-driven reactions (crowd control), the basic-attack chain, and
// additive healing/shield/cleanse overlays. Reactions and CC are never guessed on
// the client — they play only when the authoritative server reports the event, so
// the animation always matches the real game state.
//
// Wire alongside AbilityAnimator + DeathRespawnController on the champion prefab.

using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.Art.Animation
{
    [RequireComponent(typeof(Animator))]
    public sealed class CombatReactionAnimator : MonoBehaviour
    {
        [SerializeField] private ChampionAnimationSet animationSet;
        [SerializeField] private int upperLayerIndex = 1; // additive layer for overlays

        private Animator _animator;
        private int _comboIndex;
        private float _lastAttackTime;

        public event System.Action<string, string> OnVfxCue; // key, socket

        private void Awake() => _animator = GetComponent<Animator>();
        public void SetAnimationSet(ChampionAnimationSet set) => animationSet = set;

        // ---- Basic attacks (combo chain) ---- //
        /// <summary>Play the next swing in the chain. attackSpeed scales playback.</summary>
        public void PlayBasicAttack(float attackSpeed = 1f)
        {
            var chain = animationSet != null ? animationSet.attackChain : null;
            if (chain == null || chain.swings == null || chain.swings.Length == 0) return;

            // Reset combo if too long since the last swing.
            if (Time.time - _lastAttackTime > chain.comboResetSeconds) _comboIndex = 0;
            _lastAttackTime = Time.time;

            var swing = chain.swings[_comboIndex % chain.swings.Length];
            float rate = Mathf.Max(0.1f, attackSpeed);
            _animator.SetFloat("AttackSpeed", rate);
            _animator.CrossFadeInFixedTime(swing.clipName, 0.04f);

            if (!string.IsNullOrEmpty(swing.impactVfxKey))
                Invoke(nameof(FirePendingImpact), swing.hitFrame / rate);
            _pendingImpact = swing.impactVfxKey;

            _comboIndex = (_comboIndex + 1) % chain.swings.Length;
        }

        private string _pendingImpact;
        private void FirePendingImpact()
        {
            if (!string.IsNullOrEmpty(_pendingImpact)) OnVfxCue?.Invoke(_pendingImpact, "weapon_tip");
        }

        // ---- Server-driven crowd-control reactions ---- //
        public enum CcType { Flinch, HitReact, Knockback, Knockup, Stun, Root, Silence, Slow }

        /// <summary>Play a reaction because the SERVER reported this CC/damage event.</summary>
        public void PlayReaction(CcType type, float serverDuration = -1f)
        {
            var r = animationSet != null ? animationSet.reactions : null;
            if (r == null) return;
            ReactionClip clip = type switch
            {
                CcType.Flinch => r.flinch,
                CcType.HitReact => r.hitReact,
                CcType.Knockback => r.knockback,
                CcType.Knockup => r.knockup,
                CcType.Stun => r.stun,
                CcType.Root => r.root,
                CcType.Silence => r.silence,
                CcType.Slow => r.slowOverlay,
                _ => r.hitReact
            };
            if (clip == null || string.IsNullOrEmpty(clip.clipName)) return;

            if (type == CcType.Slow)
            {
                // Slow is an additive overlay, not a hard interrupt.
                _animator.CrossFadeInFixedTime(clip.clipName, 0.1f, upperLayerIndex);
            }
            else if (type == CcType.Knockup)
            {
                _animator.CrossFadeInFixedTime(clip.clipName, 0.02f);
                if (!string.IsNullOrEmpty(r.airborneLoop?.clipName))
                    Invoke(nameof(GoAirborne), clip.duration);
            }
            else
            {
                _animator.CrossFadeInFixedTime(clip.clipName, clip.interrupts ? 0.02f : 0.08f);
            }

            if (!string.IsNullOrEmpty(clip.reactVfxKey)) OnVfxCue?.Invoke(clip.reactVfxKey, "root");
        }

        private void GoAirborne()
        {
            var loop = animationSet?.reactions?.airborneLoop;
            if (loop != null && !string.IsNullOrEmpty(loop.clipName))
                _animator.CrossFadeInFixedTime(loop.clipName, 0.05f);
        }

        // ---- Healing / shield / buff / cleanse overlays (additive) ---- //
        public void PlayHealReceive()
        {
            var h = animationSet != null ? animationSet.healing : null;
            if (h == null) return;
            if (!string.IsNullOrEmpty(h.healReceiveClip))
                _animator.CrossFadeInFixedTime(h.healReceiveClip, 0.1f, upperLayerIndex);
            if (!string.IsNullOrEmpty(h.healReceiveVfxKey)) OnVfxCue?.Invoke(h.healReceiveVfxKey, "chest");
        }

        public void PlayShieldGain()
        {
            var h = animationSet != null ? animationSet.healing : null;
            if (h == null) return;
            if (!string.IsNullOrEmpty(h.shieldGainVfxKey)) OnVfxCue?.Invoke(h.shieldGainVfxKey, "root");
        }

        public void PlayShieldBreak()
        {
            var h = animationSet != null ? animationSet.healing : null;
            if (h != null && !string.IsNullOrEmpty(h.shieldBreakVfxKey)) OnVfxCue?.Invoke(h.shieldBreakVfxKey, "root");
        }

        public void PlayCleanse()
        {
            var h = animationSet != null ? animationSet.healing : null;
            if (h == null) return;
            if (!string.IsNullOrEmpty(h.cleanseClip))
                _animator.CrossFadeInFixedTime(h.cleanseClip, 0.08f, upperLayerIndex);
            if (!string.IsNullOrEmpty(h.cleanseVfxKey)) OnVfxCue?.Invoke(h.cleanseVfxKey, "feet");
        }
    }
}

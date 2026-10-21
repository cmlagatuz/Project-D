// Project D  |  Codename: Zantetsuken
// AbilityAnimator.cs  (Art pipeline runtime — plays skill animations)
//
// Bridges the animation data to Unity's Animator. On a LOCAL cast prediction it
// starts the ability clip immediately (responsive feel); when the SERVER confirms
// or rejects, it either lets it play out or snaps back to locomotion. It fires the
// authored VFX/SFX cues at their times and respects cancel windows.
//
// Wiring: put this next to the champion's Animator on the assembled prefab, assign
// the ChampionAnimationSet. Call PlayAbility(slot, predicted:true) from the pawn's
// cast path; call OnServerCastResult(slot, accepted) from the cast feedback event.
//
// Uses Animator state names equal to the clip names, so the generated
// AnimatorController (or a hand-built one) just needs matching states.

using System.Collections;
using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.Art.Animation
{
    [RequireComponent(typeof(Animator))]
    public sealed class AbilityAnimator : MonoBehaviour
    {
        [SerializeField] private ChampionAnimationSet animationSet;
        [SerializeField] private WeaponSocket weaponSocket; // optional; for trail toggles
        [Tooltip("Crossfade time into ability states.")]
        [SerializeField] private float crossfade = 0.06f;

        private Animator _animator;
        private Coroutine _current;
        private AbilitySlotLite _currentSlot;
        private bool _inAbility;

        /// <summary>Raised at each authored VFX cue: (key, socket).</summary>
        public event System.Action<string, string> OnVfxCue;
        /// <summary>Raised at each authored SFX cue: (key, socket).</summary>
        public event System.Action<string, string> OnSfxCue;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (weaponSocket == null) weaponSocket = GetComponentInChildren<WeaponSocket>();
        }

        public void SetAnimationSet(ChampionAnimationSet set) => animationSet = set;

        /// <summary>Start an ability animation (call on local prediction for instant feel).</summary>
        public void PlayAbility(AbilitySlotLite slot)
        {
            var anim = animationSet != null ? animationSet.ForSlot(slot) : null;
            if (anim == null || string.IsNullOrEmpty(anim.clipName)) return;

            if (_current != null) StopCoroutine(_current);
            _currentSlot = slot;
            _inAbility = true;
            _current = StartCoroutine(RunAbility(anim));
        }

        /// <summary>Server verdict for the most recent cast of this slot.</summary>
        public void OnServerCastResult(AbilitySlotLite slot, bool accepted)
        {
            if (!_inAbility || slot != _currentSlot) return;
            if (!accepted)
            {
                // Rejected: abort the mispredicted animation and return to locomotion.
                if (_current != null) StopCoroutine(_current);
                _inAbility = false;
                ToLocomotion();
            }
            // Accepted: let it play out naturally.
        }

        private IEnumerator RunAbility(AbilityAnimation anim)
        {
            _animator.CrossFadeInFixedTime(anim.clipName, crossfade);
            ToggleWeaponTrail(anim, true);

            float total = anim.loop ? Mathf.Max(anim.startup + anim.active, 0.2f) : anim.TotalDuration;
            float t = 0f;
            int vfxIdx = 0, sfxIdx = 0;

            while (t < total)
            {
                t += Time.deltaTime;

                while (anim.vfxCues != null && vfxIdx < anim.vfxCues.Length && t >= anim.vfxCues[vfxIdx].time)
                { var c = anim.vfxCues[vfxIdx++]; OnVfxCue?.Invoke(c.key, c.socket); }

                while (anim.sfxCues != null && sfxIdx < anim.sfxCues.Length && t >= anim.sfxCues[sfxIdx].time)
                { var c = anim.sfxCues[sfxIdx++]; OnSfxCue?.Invoke(c.key, c.socket); }

                yield return null;
            }

            ToggleWeaponTrail(anim, false);
            if (!anim.loop)
            {
                _inAbility = false;
                ToLocomotion();
            }
        }

        /// <summary>True while inside the committed (non-cancelable) part of the ability.</summary>
        public bool IsCommitted(AbilitySlotLite slot, float elapsed)
        {
            var anim = animationSet != null ? animationSet.ForSlot(slot) : null;
            return anim != null && elapsed < anim.cancelWindow;
        }

        private void ToLocomotion()
        {
            if (animationSet != null)
                _animator.CrossFadeInFixedTime(animationSet.idleClip, crossfade);
        }

        private void ToggleWeaponTrail(AbilityAnimation anim, bool on)
        {
            if (weaponSocket == null || string.IsNullOrEmpty(anim.weaponTrailKey)) return;
            // Effects system listens for these too; here we just forward as a cue.
            if (on) OnVfxCue?.Invoke(anim.weaponTrailKey + (on ? "_on" : "_off"), "weapon_tip");
        }
    }
}

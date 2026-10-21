// Project D  |  Codename: Zantetsuken
// DeathRespawnController.cs  (Art runtime — death + respawn countdown)
//
// Plays the death animation, holds the respawn-wait loop while a SERVER-authored
// countdown ticks down, shows a floating countdown number, then plays the revive.
// The timer VALUE is owned by the server (death timers scale with match time in a
// MOBA); this component only visualizes what the server tells it.
//
// Wire: OnServerDeath(seconds) when the server reports this unit died; the server
// also sends periodic time-remaining sync so the number stays authoritative.

using System;
using UnityEngine;
using ProjectD.Art.Animation;

namespace ProjectD.Art.Animation
{
    [RequireComponent(typeof(Animator))]
    public sealed class DeathRespawnController : MonoBehaviour
    {
        [SerializeField] private ChampionAnimationSet animationSet;
        [Tooltip("Optional UI text (world-space) that shows the countdown number.")]
        [SerializeField] private TMPHook countdownLabel;

        private Animator _animator;
        private bool _dead;
        private float _remaining;

        /// <summary>Fired when the revive completes.</summary>
        public event Action OnRevived;
        /// <summary>Fired each second with the integer seconds remaining (for HUD).</summary>
        public event Action<int> OnCountdownTick;

        public bool IsDead => _dead;
        public float Remaining => _remaining;

        private void Awake() => _animator = GetComponent<Animator>();

        public void SetAnimationSet(ChampionAnimationSet set) => animationSet = set;

        /// <summary>Server reports death with the authoritative respawn time.</summary>
        public void OnServerDeath(float respawnSeconds)
        {
            _dead = true;
            _remaining = Mathf.Max(0f, respawnSeconds);
            var dr = animationSet != null ? animationSet.deathRespawn : null;
            if (dr != null && !string.IsNullOrEmpty(dr.deathClip))
                _animator.CrossFadeInFixedTime(dr.deathClip, 0.05f);
            ShowNumber(dr == null || dr.showCountdownNumber);
        }

        /// <summary>Server periodically corrects the remaining time (anti-drift).</summary>
        public void SyncRemaining(float serverRemaining)
        {
            if (_dead) _remaining = Mathf.Max(0f, serverRemaining);
        }

        private int _lastWhole = -1;

        private void Update()
        {
            if (!_dead) return;

            _remaining -= Time.deltaTime;

            int whole = Mathf.CeilToInt(Mathf.Max(0f, _remaining));
            if (whole != _lastWhole)
            {
                _lastWhole = whole;
                if (countdownLabel != null) countdownLabel.SetText(whole.ToString());
                OnCountdownTick?.Invoke(whole);
            }

            // Transition into the respawn-wait loop shortly after the death clip.
            var dr = animationSet != null ? animationSet.deathRespawn : null;
            if (dr != null && !string.IsNullOrEmpty(dr.countdownLoopClip))
            {
                var st = _animator.GetCurrentAnimatorStateInfo(0);
                if (!st.IsName(dr.countdownLoopClip) && st.normalizedTime > 0.9f && st.IsName(dr.deathClip))
                    _animator.CrossFadeInFixedTime(dr.countdownLoopClip, 0.2f);
            }

            if (_remaining <= 0f)
                Revive();
        }

        private void Revive()
        {
            _dead = false;
            _lastWhole = -1;
            ShowNumber(false);
            var dr = animationSet != null ? animationSet.deathRespawn : null;
            if (dr != null && !string.IsNullOrEmpty(dr.reviveClip))
                _animator.CrossFadeInFixedTime(dr.reviveClip, 0.1f);
            OnRevived?.Invoke();
        }

        private void ShowNumber(bool show)
        {
            if (countdownLabel != null) countdownLabel.SetVisible(show);
        }
    }

    // Tiny abstraction so this file has no hard dependency on TextMeshPro; assign
    // any component that implements SetText/SetVisible, or adapt to a UI.Text.
    public interface ITextHook { void SetText(string s); void SetVisible(bool v); }

    [Serializable]
    public sealed class TMPHook
    {
        public UnityEngine.Object target; // a component implementing ITextHook, or a UI.Text
        public void SetText(string s)
        {
            if (target is ITextHook h) h.SetText(s);
            else if (target is UnityEngine.UI.Text t) t.text = s;
        }
        public void SetVisible(bool v)
        {
            if (target is ITextHook h) h.SetVisible(v);
            else if (target is UnityEngine.UI.Text t) t.enabled = v;
        }
    }
}

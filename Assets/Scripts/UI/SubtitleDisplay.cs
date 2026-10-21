// Project D  |  Codename: Zantetsuken
// SubtitleDisplay.cs  (Settings UI — VO subtitle renderer driven by settings)
//
// Shows champion VO / announcer subtitles, honoring the accessibility settings:
//   - SubtitleMode Off hides everything.
//   - TextOnly hides the speaker name; SpeakerAndText shows "Speaker: line".
//   - text scale + background opacity are applied live.
// Any voice system calls Show(speaker, line); this component decides what/if to draw.
// Subscribes to SettingsService so toggling in the menu updates it immediately.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Settings;

namespace ProjectD.SettingsUI
{
    public sealed class SubtitleDisplay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text label;
        [SerializeField] private Image background;
        [SerializeField] private RectTransform panel;
        [SerializeField] private float holdSeconds = 3.2f;
        [SerializeField] private float fadeSeconds = 0.3f;

        private AccessibilitySettings _a;
        private float _timer, _fade;
        private float _baseFontSize;

        private void Awake()
        {
            if (label) _baseFontSize = label.fontSize;
            if (group) group.alpha = 0f;
        }

        /// <summary>Hook to SettingsService.OnSettingsApplied so changes apply live.</summary>
        public void ApplySettings(GameSettings s)
        {
            _a = s.accessibility;
            if (label)
            {
                label.fontSize = Mathf.RoundToInt(_baseFontSize * Mathf.Clamp(_a.subtitleTextScale, 0.6f, 2f));
            }
            if (background)
            {
                var c = background.color; c.a = Mathf.Clamp01(_a.subtitleBackgroundOpacity); background.color = c;
            }
            // If subtitles were turned off mid-line, hide immediately.
            if (!SubtitlesOn() && group) { group.alpha = 0f; _timer = 0f; }
        }

        /// <summary>Show a subtitle line. Ignored if subtitles are Off.</summary>
        public void Show(string speaker, string line)
        {
            if (!SubtitlesOn() || string.IsNullOrEmpty(line)) return;

            bool showName = _a != null && _a.subtitleSpeakerName && _a.subtitleMode == SubtitleMode.SpeakerAndText;
            label.text = (showName && !string.IsNullOrEmpty(speaker)) ? $"{speaker}: {line}" : line;

            _timer = holdSeconds; _fade = 0f;
            if (group) group.alpha = 0f;
        }

        private bool SubtitlesOn()
            => _a != null && _a.subtitlesEnabled && _a.subtitleMode != SubtitleMode.Off;

        private void Update()
        {
            if (group == null) return;
            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                if (_fade < 1f) { _fade = Mathf.Min(1f, _fade + Time.deltaTime / Mathf.Max(0.01f, fadeSeconds)); group.alpha = _fade; }
                if (_timer <= 0f) _fade = 0f;
            }
            else if (group.alpha > 0f)
            {
                group.alpha = Mathf.Max(0f, group.alpha - Time.deltaTime / Mathf.Max(0.01f, fadeSeconds));
            }
        }
    }
}

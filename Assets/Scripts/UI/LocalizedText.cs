// Project D  |  Codename: Zantetsuken
// LocalizedText.cs  (Localization UI — bind a UI.Text to a localization key)
//
// Attach to any UI Text and set a Key; it displays the localized string for the
// active UI language and re-localizes instantly when the language changes. Also
// applies RTL alignment for right-to-left languages (e.g. Arabic). Register the
// active Localizer once (via LocalizedText.Bind) and all instances stay in sync.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Localization;

namespace ProjectD.LocalizationUI
{
    [RequireComponent(typeof(Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [Tooltip("Localization key, e.g. settings.title")]
        public string key;
        [Tooltip("Optional format arguments resolved at runtime by code (via SetArgs).")]
        [SerializeField] private bool autoRefreshOnEnable = true;

        private Text _text;
        private object[] _args;

        // Shared active localizer + RTL flag (set once by the app bootstrap).
        private static Localizer _localizer;
        private static readonly System.Collections.Generic.List<LocalizedText> _instances = new();

        /// <summary>Bind the active UI localizer; all LocalizedText refresh on change.</summary>
        public static void Bind(Localizer localizer)
        {
            if (_localizer != null) _localizer.OnLanguageChanged -= RefreshAll;
            _localizer = localizer;
            if (_localizer != null) _localizer.OnLanguageChanged += RefreshAll;
            RefreshAll();
        }

        private static void RefreshAll()
        {
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                if (_instances[i] == null) { _instances.RemoveAt(i); continue; }
                _instances[i].Refresh();
            }
        }

        private void Awake() { _text = GetComponent<Text>(); }
        private void OnEnable() { if (!_instances.Contains(this)) _instances.Add(this); if (autoRefreshOnEnable) Refresh(); }
        private void OnDisable() { _instances.Remove(this); }

        /// <summary>Change the key at runtime (e.g. dynamic labels).</summary>
        public void SetKey(string newKey) { key = newKey; Refresh(); }

        /// <summary>Provide format arguments for a templated string ({0}, {1}...).</summary>
        public void SetArgs(params object[] args) { _args = args; Refresh(); }

        public void Refresh()
        {
            if (_text == null) _text = GetComponent<Text>();
            if (_localizer == null || string.IsNullOrEmpty(key)) return;

            _text.text = (_args != null && _args.Length > 0) ? _localizer.Format(key, _args) : _localizer.Get(key);

            // RTL: right-align and flip common left alignments.
            if (_localizer.IsRightToLeft)
            {
                if (_text.alignment == TextAnchor.UpperLeft) _text.alignment = TextAnchor.UpperRight;
                else if (_text.alignment == TextAnchor.MiddleLeft) _text.alignment = TextAnchor.MiddleRight;
                else if (_text.alignment == TextAnchor.LowerLeft) _text.alignment = TextAnchor.LowerRight;
            }
        }
    }
}

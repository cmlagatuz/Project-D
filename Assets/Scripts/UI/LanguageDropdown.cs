// Project D  |  Codename: Zantetsuken
// LanguageDropdown.cs  (Localization UI — the language selector)
//
// Populates a UnityEngine.UI.Dropdown with the available languages (native names),
// shows the current selection, and applies the chosen language to the
// LocalizationService when changed. Works for BOTH the UI language dropdown and the
// SUBTITLE language dropdown (set 'target' accordingly). Persists via the settings
// service the menu passes in.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Localization;

namespace ProjectD.LocalizationUI
{
    public sealed class LanguageDropdown : MonoBehaviour
    {
        public enum Target { UiLanguage, SubtitleLanguage }

        [SerializeField] private Dropdown dropdown;
        [SerializeField] private Target target = Target.UiLanguage;

        private LocalizationService _loc;
        private List<string> _codes = new();
        private System.Action<string> _onChanged;

        /// <summary>Bind the dropdown to the localization service. onChanged persists the choice.</summary>
        public void Bind(LocalizationService loc, string currentCode, System.Action<string> onChanged)
        {
            _loc = loc;
            _onChanged = onChanged;
            if (dropdown == null) dropdown = GetComponent<Dropdown>();

            _codes = new List<string>(loc.AvailableCodes());

            var options = new List<Dropdown.OptionData>(_codes.Count);
            foreach (var code in _codes)
            {
                var info = SupportedLanguages.Get(code);
                // Show native name; append English name for clarity in dev builds.
                options.Add(new Dropdown.OptionData(info.NativeName));
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(options);

            int idx = _codes.IndexOf(SupportedLanguages.IsSupported(currentCode) ? currentCode : SupportedLanguages.Default);
            if (idx < 0) idx = 0;
            dropdown.SetValueWithoutNotify(idx);

            dropdown.onValueChanged.RemoveListener(HandleChanged);
            dropdown.onValueChanged.AddListener(HandleChanged);
        }

        private void HandleChanged(int index)
        {
            if (index < 0 || index >= _codes.Count || _loc == null) return;
            string code = _codes[index];

            if (target == Target.UiLanguage) _loc.ApplyUiLanguage(code);
            else _loc.ApplySubtitleLanguage(code);

            _onChanged?.Invoke(code);
        }

        /// <summary>Refresh the selected index (e.g. after "match UI" toggles subtitles).</summary>
        public void SetSelected(string code)
        {
            int idx = _codes.IndexOf(code);
            if (idx >= 0 && dropdown) dropdown.SetValueWithoutNotify(idx);
        }
    }
}

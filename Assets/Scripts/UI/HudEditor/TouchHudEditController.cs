// Project D  |  Codename: Zantetsuken
// TouchHudEditController.cs  (Touch HUD customization — edit mode controller)
//
// Sits on the Touch HUD canvas. A small always-visible edit toggle (placed off to a
// screen edge — never blocking center, per the mobile HUD design guardrail) opens the
// edit panel. While open, tapping any registered HudElementHandle selects it and binds
// the Size / Opacity sliders to it; dragging repositions it. SAVE persists the layout
// (TouchHudLayoutStore) and keeps the changes; QUIT reverts everything back to whatever
// was active when edit mode was entered. RESET (selected / all) reverts to the
// builder-authored defaults, still requires SAVE to persist.
//
// On scene start, any previously-saved layout is applied automatically so a player's
// customization carries across matches.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.UI.HudEditor
{
    public sealed class TouchHudEditController : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private HudElementHandle[] handles = System.Array.Empty<HudElementHandle>();

        [Header("Toggle")]
        [SerializeField] private Button editToggleButton;

        [Header("Edit panel")]
        [SerializeField] private GameObject editPanelRoot;
        [SerializeField] private Text selectedLabel;
        [SerializeField] private Slider sizeSlider;
        [SerializeField] private Slider opacitySlider;
        [SerializeField] private Button resetSelectedButton;
        [SerializeField] private Button resetAllButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button quitButton;

        /// <summary>True while the player is actively customizing the HUD — gates dragging on every HudElementHandle.</summary>
        public static bool EditModeActive { get; private set; }

        private HudElementHandle _selected;
        private Dictionary<string, HudElementLayout> _layoutAtEntry;

        private void Awake()
        {
            foreach (var h in handles) { if (h == null) continue; h.CaptureDefaults(); h.OnSelected += Select; }
            ApplySavedLayout();

            if (editToggleButton) editToggleButton.onClick.AddListener(EnterEditMode);
            if (saveButton) saveButton.onClick.AddListener(SaveAndExit);
            if (quitButton) quitButton.onClick.AddListener(DiscardAndExit);
            if (resetSelectedButton) resetSelectedButton.onClick.AddListener(ResetSelected);
            if (resetAllButton) resetAllButton.onClick.AddListener(ResetAll);
            if (sizeSlider) sizeSlider.onValueChanged.AddListener(OnSizeChanged);
            if (opacitySlider) opacitySlider.onValueChanged.AddListener(OnOpacityChanged);

            EditModeActive = false;
            if (editPanelRoot) editPanelRoot.SetActive(false);
        }

        private void OnDestroy() => EditModeActive = false; // don't leak edit-mode state across scene loads

        private void ApplySavedLayout()
        {
            var saved = TouchHudLayoutStore.Load();
            if (saved.Count == 0) return;
            foreach (var h in handles)
                if (h != null && saved.TryGetValue(h.ElementId, out var l)) h.ApplyLayout(l);
        }

        // ---- edit-mode lifecycle ---- //
        private void EnterEditMode()
        {
            EditModeActive = true;
            _layoutAtEntry = CaptureAll();
            if (editPanelRoot) editPanelRoot.SetActive(true);
            Select(handles.Length > 0 ? handles[0] : null);
        }

        private void SaveAndExit()
        {
            TouchHudLayoutStore.Save(CaptureAll().Values);
            ExitEditMode();
        }

        private void DiscardAndExit()
        {
            if (_layoutAtEntry != null)
                foreach (var h in handles)
                    if (h != null && _layoutAtEntry.TryGetValue(h.ElementId, out var l)) h.ApplyLayout(l);
            ExitEditMode();
        }

        private void ExitEditMode()
        {
            EditModeActive = false;
            if (editPanelRoot) editPanelRoot.SetActive(false);
            _selected = null;
        }

        // ---- selection + sliders ---- //
        private void Select(HudElementHandle h)
        {
            _selected = h;
            if (h == null) return;
            if (selectedLabel) selectedLabel.text = h.ElementId;
            var l = h.CurrentLayout();
            if (sizeSlider) sizeSlider.SetValueWithoutNotify(l.Scale);
            if (opacitySlider) opacitySlider.SetValueWithoutNotify(l.Alpha);
        }

        private void OnSizeChanged(float v) => _selected?.SetScale(v);
        private void OnOpacityChanged(float v) => _selected?.SetAlpha(v);

        // ---- reset ---- //
        private void ResetSelected()
        {
            _selected?.ResetToDefault();
            if (_selected != null) Select(_selected);
        }

        private void ResetAll()
        {
            foreach (var h in handles) if (h != null) h.ResetToDefault();
            if (_selected != null) Select(_selected);
        }

        private Dictionary<string, HudElementLayout> CaptureAll()
        {
            var d = new Dictionary<string, HudElementLayout>();
            foreach (var h in handles) if (h != null) d[h.ElementId] = h.CurrentLayout();
            return d;
        }
    }
}

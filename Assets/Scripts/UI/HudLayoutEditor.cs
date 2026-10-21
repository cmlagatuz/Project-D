// Project D  |  Codename: Zantetsuken
// HudLayoutEditor.cs  (Settings UI — the "move your buttons" editor)
//
// Enters/exits edit mode, applies a HudLayout onto the registered DraggableHudElements
// (position + scale + visibility), listens for drags to update the layout in memory,
// and saves through the SettingsService. Also offers per-element scale + show/hide and
// a "reset to default" that respects the current handedness.

using System.Collections.Generic;
using UnityEngine;
using ProjectD.Settings;

namespace ProjectD.SettingsUI
{
    public sealed class HudLayoutEditor : MonoBehaviour
    {
        [SerializeField] private List<DraggableHudElement> elements = new();
        [SerializeField] private GameObject editModeOverlay;   // dimmer + "Editing HUD" banner
        private SettingsService _service;
        private HudLayout _working;      // edited copy; committed on Save
        public bool IsEditing { get; private set; }

        public event System.Action<bool> OnEditModeChanged;

        public void Bind(SettingsService service)
        {
            _service = service;
            foreach (var el in elements)
            {
                if (el == null) continue;
                el.OnChanged += HandleElementChanged;
            }
            ApplyLayout(service.Layout);
        }

        /// <summary>Apply a layout to all elements (positions/scale/visibility).</summary>
        public void ApplyLayout(HudLayout layout)
        {
            if (layout == null) return;
            foreach (var el in elements)
            {
                if (el == null) continue;
                var e = layout.Get(el.ElementId);
                if (e == null) continue;
                el.gameObject.SetActive(e.visible);
                el.SetNormalized(e.x, e.y, e.scale);
            }
        }

        public void EnterEditMode()
        {
            if (_service == null) return;
            IsEditing = true;
            _working = _service.Layout.Clone();
            foreach (var el in elements) if (el) el.EditMode = true;
            if (editModeOverlay) editModeOverlay.SetActive(true);
            OnEditModeChanged?.Invoke(true);
        }

        /// <summary>Commit the edited layout and leave edit mode.</summary>
        public void SaveAndExit()
        {
            if (_service != null && _working != null)
            {
                _service.Layout.elements = _working.elements;
                _service.SaveLayout();
            }
            ExitEditMode();
        }

        /// <summary>Discard edits and restore the saved layout.</summary>
        public void CancelAndExit()
        {
            if (_service != null) ApplyLayout(_service.Layout);
            ExitEditMode();
        }

        public void ResetToDefault()
        {
            if (_service == null) return;
            _service.ResetLayoutToDefault();
            _working = _service.Layout.Clone();
            ApplyLayout(_service.Layout);
        }

        public void SetElementVisible(HudElementId id, bool visible)
        {
            var e = (_working ?? _service.Layout).Get(id);
            if (e != null) e.visible = visible;
            foreach (var el in elements) if (el && el.ElementId == id) el.gameObject.SetActive(visible);
        }

        public void SetElementScale(HudElementId id, float scale)
        {
            foreach (var el in elements) if (el && el.ElementId == id) el.SetScale(scale);
        }

        private void HandleElementChanged(HudElementId id, float nx, float ny, float scale)
        {
            var layout = _working ?? _service?.Layout;
            layout?.Set(id, nx, ny, scale, layout.Get(id)?.visible ?? true);
        }

        private void ExitEditMode()
        {
            IsEditing = false;
            foreach (var el in elements) if (el) el.EditMode = false;
            if (editModeOverlay) editModeOverlay.SetActive(false);
            OnEditModeChanged?.Invoke(false);
        }
    }
}

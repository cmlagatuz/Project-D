// Project D  |  Codename: Zantetsuken
// DraggableHudElement.cs  (Settings UI — a movable/scalable on-screen control)
//
// Wraps one HUD control (move stick, ability button, item slot...) so that in EDIT
// MODE the player can drag it anywhere and pinch/slider-scale it, and in PLAY MODE
// it just sits at its saved normalized position. Positions are stored as 0..1 of the
// safe area so the layout is resolution/aspect independent. Reports changes back to
// the layout editor, which writes them into HudLayout.

using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectD.SettingsUI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class DraggableHudElement : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        public ProjectD.Settings.HudElementId ElementId;
        [Tooltip("Only draggable while the layout editor is active.")]
        public bool EditMode;
        [Range(0.6f, 1.5f)] public float Scale = 1f;

        private RectTransform _rt;
        private RectTransform _canvasRt;
        private Rect _safe;

        /// <summary>Raised when the player moves/scales this element (normalized x,y,scale).</summary>
        public event System.Action<ProjectD.Settings.HudElementId, float, float, float> OnChanged;

        private void Awake()
        {
            _rt = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas) _canvasRt = (RectTransform)canvas.transform;
        }

        /// <summary>Place at a normalized position (0..1 of the safe area) with a scale.</summary>
        public void SetNormalized(float nx, float ny, float scale)
        {
            Scale = Mathf.Clamp(scale, 0.6f, 1.5f);
            _rt.localScale = Vector3.one * Scale;
            if (_canvasRt == null) return;

            ComputeSafeArea();
            float px = _safe.xMin + nx * _safe.width;
            float py = _safe.yMin + ny * _safe.height;
            // Convert screen-space safe-area point to the canvas' local space.
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRt, new Vector2(px, py), null, out local);
            _rt.anchoredPosition = local;
        }

        public void OnPointerDown(PointerEventData e) { /* select handled by editor */ }

        public void OnDrag(PointerEventData e)
        {
            if (!EditMode || _canvasRt == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRt, e.position, e.pressEventCamera, out var local);
            _rt.anchoredPosition = local;

            // Report back as a normalized position within the safe area.
            ComputeSafeArea();
            Vector3 screen = RectTransformUtility.WorldToScreenPoint(null, _rt.position);
            float nx = _safe.width  > 0 ? Mathf.Clamp01((screen.x - _safe.xMin) / _safe.width)  : 0.5f;
            float ny = _safe.height > 0 ? Mathf.Clamp01((screen.y - _safe.yMin) / _safe.height) : 0.5f;
            OnChanged?.Invoke(ElementId, nx, ny, Scale);
        }

        public void SetScale(float scale)
        {
            Scale = Mathf.Clamp(scale, 0.6f, 1.5f);
            _rt.localScale = Vector3.one * Scale;
            ComputeSafeArea();
            Vector3 screen = RectTransformUtility.WorldToScreenPoint(null, _rt.position);
            float nx = _safe.width  > 0 ? Mathf.Clamp01((screen.x - _safe.xMin) / _safe.width)  : 0.5f;
            float ny = _safe.height > 0 ? Mathf.Clamp01((screen.y - _safe.yMin) / _safe.height) : 0.5f;
            OnChanged?.Invoke(ElementId, nx, ny, Scale);
        }

        private void ComputeSafeArea()
        {
            // Respect device notches/cutouts: place within Screen.safeArea.
            _safe = Screen.safeArea;
            if (_safe.width <= 0 || _safe.height <= 0) _safe = new Rect(0, 0, Screen.width, Screen.height);
        }
    }
}

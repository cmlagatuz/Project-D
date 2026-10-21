// Project D  |  Codename: Zantetsuken
// HudElementHandle.cs  (Touch HUD customization — one repositionable/resizable HUD element)
//
// Drop this on any Touch HUD element's root (the movement stick, each ability button,
// later the item cluster / ping wheel) with a stable elementId. While
// TouchHudEditController.EditModeActive is true, tapping selects it (for the edit
// panel's sliders) and dragging repositions it. Outside edit mode it's inert — normal
// gameplay input passes straight through to the element's own handlers underneath.

using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectD.UI.HudEditor
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class HudElementHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Tooltip("Stable id used for persistence — must be unique per HUD.")]
        [SerializeField] private string elementId;
        [SerializeField] private float minScale = 0.65f;
        [SerializeField] private float maxScale = 1.5f;

        public string ElementId => elementId;
        public event System.Action<HudElementHandle> OnSelected;

        private RectTransform _rt;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;

        private Vector2 _defaultPos;
        private float _defaultScale = 1f;
        private float _defaultAlpha = 1f;

        private void Awake()
        {
            _rt = (RectTransform)transform;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
        }

        /// <summary>Snapshot the builder-authored position/scale/alpha as the "factory" layout (call once at scene setup, before any saved layout is applied).</summary>
        public void CaptureDefaults()
        {
            _defaultPos = _rt.anchoredPosition;
            _defaultScale = _rt.localScale.x;
            _defaultAlpha = _canvasGroup.alpha;
        }

        public HudElementLayout CurrentLayout() => new HudElementLayout
        {
            ElementId = elementId, X = _rt.anchoredPosition.x, Y = _rt.anchoredPosition.y,
            Scale = _rt.localScale.x, Alpha = _canvasGroup.alpha
        };

        public void ApplyLayout(HudElementLayout l)
        {
            _rt.anchoredPosition = new Vector2(l.X, l.Y);
            _rt.localScale = Vector3.one * Mathf.Clamp(l.Scale, minScale, maxScale);
            _canvasGroup.alpha = Mathf.Clamp01(l.Alpha);
        }

        public void ResetToDefault() => ApplyLayout(new HudElementLayout
        { ElementId = elementId, X = _defaultPos.x, Y = _defaultPos.y, Scale = _defaultScale, Alpha = _defaultAlpha });

        public void SetScale(float s) => _rt.localScale = Vector3.one * Mathf.Clamp(s, minScale, maxScale);
        public void SetAlpha(float a) => _canvasGroup.alpha = Mathf.Clamp01(a);

        public void OnPointerDown(PointerEventData e)
        {
            if (!TouchHudEditController.EditModeActive) return;
            OnSelected?.Invoke(this);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!TouchHudEditController.EditModeActive) return;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            _rt.anchoredPosition += e.delta / Mathf.Max(scale, 0.0001f);
        }
    }
}

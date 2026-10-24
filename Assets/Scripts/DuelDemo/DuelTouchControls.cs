// Project D | Codename: Zantetsuken — DuelTouchControls.cs
// Two tiny uGUI input components used by the duel demo HUD:
//   DuelTouchStick  - floating movement stick (appears where the left thumb lands)
//   DuelAimButton   - tap = auto-aim, drag = aim in that direction, or hold-to-repeat (basic attack)
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectD.DuelDemo
{
    public sealed class DuelTouchStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Area;    // pivot (0,0); the stick lives inside it
        public RectTransform Ring;    // anchored to Area's bottom-left
        public RectTransform Knob;    // anchored to Area's bottom-left
        public float Radius = 110f;
        public System.Action<Vector2> Changed;

        private Vector2 _center;

        public void OnPointerDown(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Area, e.position, e.pressEventCamera, out var local)) return;
            _center = local;
            Ring.anchoredPosition = local;
            Knob.anchoredPosition = local;
            Ring.gameObject.SetActive(true);
            Knob.gameObject.SetActive(true);
            if (Changed != null) Changed(Vector2.zero);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Area, e.position, e.pressEventCamera, out var local)) return;
            var d = local - _center;
            if (d.magnitude > Radius) d = d.normalized * Radius;
            Knob.anchoredPosition = _center + d;
            var v = d / Radius;
            if (v.magnitude < 0.15f) v = Vector2.zero;   // dead zone
            if (Changed != null) Changed(v);
        }

        public void OnPointerUp(PointerEventData e) { Release(); }

        private void OnDisable() { Release(); }

        private void Release()
        {
            if (Ring != null) Ring.gameObject.SetActive(false);
            if (Knob != null) Knob.gameObject.SetActive(false);
            if (Changed != null) Changed(Vector2.zero);
        }
    }

    public sealed class DuelAimButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public bool HoldMode;                       // true: report held / released (basic attack)
        public System.Action<bool> Held;
        public System.Action<Vector2?> Released;    // null = tap (auto-aim); otherwise the drag direction on screen
        public float DragThresholdPixels = 40f;

        private Vector2 _start;
        private bool _dragged;
        private Vector3 _baseScale = Vector3.one;
        private bool _down;

        private void Awake() { _baseScale = transform.localScale; }

        public void OnPointerDown(PointerEventData e)
        {
            _down = true;
            _start = e.position;
            _dragged = false;
            transform.localScale = _baseScale * 0.93f;
            if (HoldMode && Held != null) Held(true);
        }

        public void OnDrag(PointerEventData e)
        {
            if ((e.position - _start).magnitude > DragThresholdPixels) _dragged = true;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!_down) return;
            _down = false;
            transform.localScale = _baseScale;
            if (HoldMode)
            {
                if (Held != null) Held(false);
                return;
            }
            if (Released == null) return;
            if (_dragged) Released((e.position - _start).normalized);
            else Released(null);
        }

        private void OnDisable()
        {
            if (!_down) return;
            _down = false;
            transform.localScale = _baseScale;
            if (HoldMode && Held != null) Held(false);
        }
    }
}

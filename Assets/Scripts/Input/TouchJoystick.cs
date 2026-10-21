// Project D | Codename: Zantetsuken — TouchJoystick.cs (GCD 10.1)
using UnityEngine; using UnityEngine.EventSystems; using UnityEngine.UI;
namespace ProjectD.Gameplay.InputSystem
{
    public sealed class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background; [SerializeField] private RectTransform handle;
        [Range(0f,0.5f)][SerializeField] private float deadZone = 0.12f; [SerializeField] private float handleRange = 70f; [SerializeField] private bool floating = true;
        public Vector2 Direction { get; private set; } public bool IsActive { get; private set; }
        private Vector2 _home; private Canvas _canvas;
        private void Awake(){ _canvas=GetComponentInParent<Canvas>(); if(background!=null) _home=background.anchoredPosition; }
        public void OnPointerDown(PointerEventData e){ IsActive=true; if(floating&&background!=null&&_canvas!=null){ RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform,e.position,e.pressEventCamera,out var l); background.anchoredPosition=l; } OnDrag(e); }
        public void OnDrag(PointerEventData e){ if(background==null)return; RectTransformUtility.ScreenPointToLocalPointInRectangle(background,e.position,e.pressEventCamera,out var pos); Vector2 raw=pos/handleRange; float mag=raw.magnitude; Vector2 dir=mag<deadZone?Vector2.zero:raw.normalized*Mathf.InverseLerp(deadZone,1f,Mathf.Min(mag,1f)); Direction=dir; if(handle!=null) handle.anchoredPosition=Vector2.ClampMagnitude(dir,1f)*handleRange; }
        public void OnPointerUp(PointerEventData e){ IsActive=false; Direction=Vector2.zero; if(handle!=null) handle.anchoredPosition=Vector2.zero; if(floating&&background!=null) background.anchoredPosition=_home; }
        public void SetLeftHanded(bool lh){ if(background==null)return; var ap=background.anchoredPosition; background.anchoredPosition=new Vector2(Mathf.Abs(ap.x)*(lh?1:-1),ap.y); _home=background.anchoredPosition; }
    }
}

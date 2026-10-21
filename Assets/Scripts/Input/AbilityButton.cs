// Project D | Codename: Zantetsuken — AbilityButton.cs (GCD 10.1 tap/drag-aim)
using UnityEngine; using UnityEngine.EventSystems; using UnityEngine.UI; using ProjectD.Data;
namespace ProjectD.Gameplay.InputSystem
{
    public sealed class AbilityButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private AbilitySlot slot = AbilitySlot.Q; [SerializeField] private Image cooldownOverlay; [SerializeField] private Text label; [SerializeField] private float aimDeadZonePixels = 24f;
        public AbilitySlot Slot => slot; public Vector2 AimDirection { get; private set; } public bool IsAiming { get; private set; }
        public event System.Action<AbilitySlot,Vector2,bool> OnCastRequested;
        private Vector2 _down;
        private void Awake(){ if(label!=null) label.text=slot.ToString(); SetCooldownFill(0f); }
        public void OnPointerDown(PointerEventData e){ _down=e.position; IsAiming=true; AimDirection=Vector2.zero; }
        public void OnDrag(PointerEventData e){ Vector2 d=e.position-_down; AimDirection=d.magnitude<aimDeadZonePixels?Vector2.zero:d.normalized; }
        public void OnPointerUp(PointerEventData e){ bool dragged=AimDirection!=Vector2.zero; OnCastRequested?.Invoke(slot,AimDirection,dragged); IsAiming=false; AimDirection=Vector2.zero; }
        public void SetCooldownFill(float n){ if(cooldownOverlay!=null) cooldownOverlay.fillAmount=Mathf.Clamp01(n); }
    }
}

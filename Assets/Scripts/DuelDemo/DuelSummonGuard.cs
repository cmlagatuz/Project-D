// Project D | Codename: Zantetsuken — DuelSummonGuard.cs
// Idle behaviour for the summons (Shiva, Garuda, Bahamut) that hold the pit. They are guardians contesting the arena:
// they stand their ground, breathe and sway, hover (Garuda), turn to keep the nearest champion in view, rear up and
// roar when someone enters their territory, and recoil when hit. They are single-mesh rips with no usable skeleton, so
// this moves the whole model (position, lean, roll, scale) rather than bones. The controller sets Yaw and AlertTarget.
using UnityEngine;

namespace ProjectD.DuelDemo
{
    public sealed class DuelSummonGuard : MonoBehaviour
    {
        public float Yaw;            // the library's Yaw Offset (set by the controller)
        public float AlertTarget;    // 1 while a champion is inside the summon's territory, else 0

        private float _hover, _bob, _bobHz, _breath, _sway, _alertLean, _alertRise, _alertScale, _flap;
        private float _seed;
        private float _alert, _flinchAt = -10f, _roarAt = -10f;
        private bool _wasAlert;

        public void Init(string id)
        {
            _seed = (id.GetHashCode() & 0xFF) * 0.37f;
            switch (id)
            {
                case "garuda":   // a hovering wind spirit: bobs on the air, rolls with the wingbeat, stoops to dive when alert
                    _hover = 0.9f; _bob = 0.2f; _bobHz = 1.15f; _breath = 0.01f; _sway = 3f;
                    _alertLean = 14f; _alertRise = 0.7f; _alertScale = 0.03f; _flap = 0f;
                    break;
                case "bahamut":   // the dragon king: heavy, planted, slow breathing, rears back and spreads wide to roar
                    _hover = 0f; _bob = 0f; _bobHz = 0.3f; _breath = 0.014f; _sway = 1.2f;
                    _alertLean = -5f; _alertRise = 0f; _alertScale = 0.04f; _flap = 0.01f;
                    break;
                default:   // shiva: composed and still, rises slightly and leans back haughtily when challenged
                    _hover = 0.05f; _bob = 0.04f; _bobHz = 0.4f; _breath = 0.01f; _sway = 1.6f;
                    _alertLean = -3f; _alertRise = 0.12f; _alertScale = 0.02f; _flap = 0f;
                    break;
            }
        }

        public float Alert01 { get { return _alert; } }   // 0..1, eased; the wing and cloth motion read it

        public void Flinch() { _flinchAt = Time.time; }

        public void Roar() { _roarAt = Time.time; }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            float now = Time.time;
            float t = now + _seed;

            bool alertNow = AlertTarget > 0.5f;
            if (alertNow && !_wasAlert) Roar();
            _wasAlert = alertNow;
            _alert = Mathf.MoveTowards(_alert, AlertTarget, dt * 2.2f);

            float breathe = Mathf.Sin(t * 1.6f);
            float flinch = Mathf.Max(0f, 1f - (now - _flinchAt) / 0.35f);
            flinch *= flinch;
            float roarT = (now - _roarAt) / 0.9f;
            float roar = roarT >= 0f && roarT < 1f ? Mathf.Sin(roarT * Mathf.PI) : 0f;

            float y = _hover * (1f + 0.3f * _alert) + _bob * Mathf.Sin(t * _bobHz * Mathf.PI * 2f) + _alertRise * _alert + 0.08f * roar;
            float pitch = _alertLean * _alert + 0.6f * breathe - 7f * flinch - 6f * roar * (_alertLean < 0f ? 1f : 0f);
            float roll = _sway * Mathf.Sin(t * 0.9f) * (1f - 0.5f * _alert) + 3f * flinch;
            float wobble = 2.5f * Mathf.Sin(t * 0.5f) * (1f - _alert);

            float s = 1f + _breath * breathe + _alertScale * _alert + 0.05f * roar + 0.03f * flinch;
            float sx = s * (1f + _flap * Mathf.Sin(t * 6.5f) * (1f + _alert));

            transform.localPosition = new Vector3(0f, y, 0f);
            transform.localRotation = Quaternion.Euler(0f, Yaw + wobble, 0f) * Quaternion.Euler(pitch, 0f, roll);
            transform.localScale = new Vector3(sx, s, sx);
        }
    }
}

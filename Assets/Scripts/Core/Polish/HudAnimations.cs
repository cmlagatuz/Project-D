// Project D  |  Codename: Zantetsuken
// HudAnimations.cs  (Polish — reusable animated HUD components)
//
// Drop-in behaviors built on UITween + ProceduralSprites that make the HUD feel alive:
//   - PulseWhenLow    : pulses a buff/cooldown icon when its timer is nearly up.
//   - AnimatedBarFill : smoothly tweens an Image fill toward a target (no snapping).
//   - CountUpNumber   : counts a number up/down over time (gold/XP/score readouts).
//   - MinimapPing     : a ripple ring that expands + fades at a position.
//   - ArcDamageNumber : a floating damage number that pops in, arcs up, and fades.
//   - GlowBehind      : inserts a soft procedural glow behind a highlighted element.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Polish.Art;

namespace ProjectD.Polish.Anim
{
    public sealed class PulseWhenLow : MonoBehaviour
    {
        [SerializeField] private Graphic target;
        [SerializeField] private float lowThreshold = 5f;
        [SerializeField] private float minScale = 1f, maxScale = 1.18f, speed = 6f;
        private float _remaining = 999f;
        public void SetRemaining(float seconds) => _remaining = seconds;
        private void Update()
        {
            if (target == null) return;
            if (_remaining <= lowThreshold && _remaining > 0f)
            {
                float wave = (Mathf.Sin(Time.unscaledTime * speed) + 1f) / 2f;
                target.transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, wave);
                var c = target.color; c.a = Mathf.Lerp(0.6f, 1f, wave); target.color = c;
            }
            else target.transform.localScale = Vector3.one;
        }
    }

    public sealed class AnimatedBarFill : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private float lerpSpeed = 8f;
        private float _target;
        public void SetTarget(float value01) => _target = Mathf.Clamp01(value01);
        private void Update() { if (fill) fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, _target, lerpSpeed * Time.unscaledDeltaTime); }
    }

    public sealed class CountUpNumber : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private string prefix = "", suffix = "";
        private int _shown;
        public void SetValue(int value, float duration = 0.5f)
        {
            int from = _shown; _shown = value;
            Tweens.Float(from, value, duration, v => { if (label) label.text = $"{prefix}{Mathf.RoundToInt(v)}{suffix}"; }, Ease.OutQuad);
        }
    }

    public static class MinimapPing
    {
        public static void Spawn(RectTransform parent, Vector2 anchoredPos, Color color, float maxSize = 40f, float dur = 0.8f)
        {
            var go = new GameObject("Ping", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.anchoredPosition = anchoredPos; rt.sizeDelta = Vector2.zero;
            var img = go.GetComponent<Image>(); img.sprite = ProceduralSprites.Ring(48, 4); img.color = color; img.raycastTarget = false;
            Tweens.Float(0f, 1f, dur, k =>
            {
                if (!go) return;
                rt.sizeDelta = Vector2.one * (maxSize * k);
                var c = img.color; c.a = 1f - k; img.color = c;
            }, Ease.OutQuad, () => { if (go) Object.Destroy(go); });
        }
    }

    public static class ArcDamageNumber
    {
        public static void Spawn(RectTransform parent, Text prefab, Vector2 startPos, string text, Color color, int fontSize, bool crit)
        {
            var t = Object.Instantiate(prefab, parent);
            t.gameObject.SetActive(true);
            t.text = text; t.color = color; t.fontSize = fontSize;
            var rt = (RectTransform)t.transform; rt.anchoredPosition = startPos; rt.localScale = Vector3.zero;
            float dirX = Random.Range(-40f, 40f), rise = crit ? 120f : 80f, life = crit ? 1.1f : 0.85f;
            Tweens.Scale(rt, Vector3.zero, Vector3.one * (crit ? 1.25f : 1f), 0.14f, Ease.OutBack);
            Tweens.Float(0f, 1f, life, k =>
            {
                if (!t) return;
                rt.anchoredPosition = new Vector2(startPos.x + dirX * k, startPos.y + rise * k - (crit ? 0f : 10f * k * k));
                var c = t.color; c.a = k < 0.6f ? 1f : Mathf.InverseLerp(1f, 0.6f, k); t.color = c;
            }, Ease.OutQuad, () => { if (t) Object.Destroy(t.gameObject); });
        }
    }

    public static class GlowBehind
    {
        public static Image Attach(RectTransform target, Color color, float sizeMultiplier = 1.6f)
        {
            var go = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(target, false); go.transform.SetAsFirstSibling();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = target.sizeDelta * sizeMultiplier;
            var img = go.GetComponent<Image>(); img.sprite = ProceduralSprites.Glow(); img.color = color; img.raycastTarget = false;
            return img;
        }
    }

    // Loops the flourishes for the demo scene.
    public sealed class PolishDemoDriver : MonoBehaviour
    {
        [SerializeField] private RectTransform minimap, damageLayer;
        [SerializeField] private Text damagePrefab;
        [SerializeField] private AnimatedBarFill animBar;
        [SerializeField] private CountUpNumber count;
        private float _pingTimer, _dmgTimer, _barTimer; private int _gold = 14250;

        private void Update()
        {
            _pingTimer -= Time.deltaTime;
            if (_pingTimer <= 0f && minimap) { _pingTimer = 1.2f; MinimapPing.Spawn(minimap, new Vector2(Random.Range(-120f,120f), Random.Range(-120f,120f)), Random.value < 0.5f ? new Color(0.5f,0.85f,0.6f) : new Color(0.9f,0.4f,0.4f)); }

            _dmgTimer -= Time.deltaTime;
            if (_dmgTimer <= 0f && damageLayer && damagePrefab)
            {
                _dmgTimer = 0.6f; bool crit = Random.value < 0.3f; int amount = Random.Range(120, crit ? 480 : 300);
                var color = crit ? new Color(1f,0.75f,0.3f) : (Random.value < 0.5f ? new Color(0.95f,0.65f,0.4f) : new Color(0.6f,0.6f,0.95f));
                ArcDamageNumber.Spawn(damageLayer, damagePrefab, new Vector2(Random.Range(-140f,140f), Random.Range(-40f,40f)), amount.ToString(), color, crit ? 34 : 24, crit);
            }

            _barTimer -= Time.deltaTime;
            if (_barTimer <= 0f && animBar) { _barTimer = 2.5f; animBar.SetTarget(Random.Range(0.15f, 1f)); _gold += Random.Range(200, 900); if (count) count.SetValue(_gold, 0.8f); }
        }
    }
}

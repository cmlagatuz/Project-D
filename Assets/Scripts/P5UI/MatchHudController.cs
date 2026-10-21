// Project D  |  Codename: Zantetsuken
// MatchHudController.cs  (P5 UI — the combined in-match HUD overlay)
//
// One controller that ties the P5 gameplay cores into a single match overlay:
//   - a live MATCH CLOCK (drives buff timers, surrender availability, minute-scaling),
//   - the OBJECTIVE / BUFF BAR (active team blessings with remaining timers + stacks),
//   - the VISION / WARD tracker (per-type counts + a minimap plot of ward positions),
//   - the EMOTE WHEEL (radial, honoring the core cooldown + spam guard),
//   - the SURRENDER panel (timed vote with a countdown + yes/needed tally),
//   - a DAMAGE-NUMBER layer (floating numbers styled by type/crit from DamageModel).
//
// Server-authoritative in a real build; this renders/acts on server-owned models. The
// demo buttons (grant buff, place ward, spawn damage, call surrender) simulate the
// events the server would push.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P5.Objectives;
using ProjectD.P5.Vision;
using ProjectD.P5.Surrender;
using ProjectD.P5.Emotes;
using ProjectD.P5.Combat;

namespace ProjectD.P5UI
{
    public sealed class MatchHudController : MonoBehaviour
    {
        [Header("Clock")]
        [SerializeField] private Text clockText;
        [SerializeField] private float startSeconds = 0f;

        [Header("Objective / buff bar")]
        [SerializeField] private RectTransform buffBar;
        [SerializeField] private RectTransform buffChipPrefab;   // [icon Image][name Text][timer Text]

        [Header("Vision / ward tracker")]
        [SerializeField] private Text stealthCountText, controlCountText;
        [SerializeField] private RectTransform minimap;
        [SerializeField] private Image wardDotPrefab;
        [SerializeField] private float worldExtent = 100f;

        [Header("Emote wheel")]
        [SerializeField] private RectTransform emoteWheelRoot;
        [SerializeField] private Button emoteSlotPrefab;
        [SerializeField] private float emoteRadius = 120f;
        [SerializeField] private Text emoteFeedback;

        [Header("Surrender panel")]
        [SerializeField] private CanvasGroup surrenderGroup;
        [SerializeField] private Text surrenderPrompt, surrenderTally, surrenderTimer;
        [SerializeField] private Button surrenderYes, surrenderNo;

        [Header("Damage numbers")]
        [SerializeField] private RectTransform damageLayer;
        [SerializeField] private Text damageNumberPrefab;

        [Header("Demo controls (simulate server events)")]
        [SerializeField] private Button grantBuffButton, placeWardButton, spawnDamageButton, callSurrenderButton, sweepButton;

        // Cores.
        private readonly TeamBuffs _buffs = new();
        private readonly VisionSystem _vision = new();
        private readonly SurrenderVote _surrender = new() { teamSize = 5, earliestSeconds = 0f };
        private readonly EmoteWheel _emotes = new();
        private float _clock;
        private bool _surrenderOpen;
        private int _demoBuffIndex, _demoWardIndex;

        private void Start()
        {
            _clock = startSeconds;
            BuildEmoteWheel();
            if (surrenderGroup) surrenderGroup.alpha = 0f;
            if (surrenderYes) surrenderYes.onClick.AddListener(() => CastSurrender(true));
            if (surrenderNo) surrenderNo.onClick.AddListener(() => CastSurrender(false));

            if (grantBuffButton) grantBuffButton.onClick.AddListener(DemoGrantBuff);
            if (placeWardButton) placeWardButton.onClick.AddListener(DemoPlaceWard);
            if (spawnDamageButton) spawnDamageButton.onClick.AddListener(DemoSpawnDamage);
            if (callSurrenderButton) callSurrenderButton.onClick.AddListener(OpenSurrender);
            if (sweepButton) sweepButton.onClick.AddListener(DemoSweep);

            // Seed a little starting state so the HUD isn't empty.
            _buffs.Grant(Blessing.Infernal, _clock, 90f);
            _buffs.Grant(Blessing.ZantetsukenEdge, _clock, 0f, permanent: true);
            _vision.Place("me", WardType.Stealth, -40, 30, _clock);
            _vision.Place("me", WardType.Control, 0, 0, _clock);

            RefreshBuffs(); RefreshVision();
        }

        private void Update()
        {
            _clock += Time.deltaTime;
            _buffs.Tick(_clock);
            _vision.Tick(_clock);

            if (clockText) clockText.text = FormatClock(_clock);
            RefreshBuffs();
            RefreshVision();

            if (_surrenderOpen) UpdateSurrender();
        }

        // ---------------- Objective / buff bar ----------------
        private void RefreshBuffs()
        {
            if (buffBar == null || buffChipPrefab == null) return;
            foreach (Transform c in buffBar) Destroy(c.gameObject);
            foreach (Blessing b in System.Enum.GetValues(typeof(Blessing)))
            {
                if (!_buffs.Has(b, _clock)) continue;
                var chip = Instantiate(buffChipPrefab, buffBar); chip.gameObject.SetActive(true);
                var img = chip.GetComponentInChildren<Image>(); if (img) img.color = BlessingColor(b);
                var texts = chip.GetComponentsInChildren<Text>();
                if (texts.Length > 0) texts[0].text = Short(b) + (_buffs.Stacks(b, _clock) > 1 ? $" x{_buffs.Stacks(b, _clock)}" : "");
                if (texts.Length > 1) { float rem = _buffs.Remaining(b, _clock); texts[1].text = rem < 0 ? "\u221E" : $"{Mathf.CeilToInt(rem)}s"; }
            }
        }

        // ---------------- Vision tracker ----------------
        private void RefreshVision()
        {
            int stealth = 0, control = 0;
            if (minimap != null && wardDotPrefab != null)
            {
                foreach (Transform c in minimap) Destroy(c.gameObject);
                foreach (var w in _vision.Wards)
                {
                    if (w.type == WardType.Control) control++; else if (w.type == WardType.Stealth) stealth++;
                    var dot = Instantiate(wardDotPrefab, minimap); dot.gameObject.SetActive(true);
                    float half = minimap.rect.width / 2f;
                    ((RectTransform)dot.transform).anchoredPosition = new Vector2(w.x / worldExtent * half, w.z / worldExtent * half);
                    dot.color = w.type == WardType.Control ? new Color(0.9f,0.4f,0.4f) : w.type == WardType.Farsight ? new Color(0.6f,0.5f,0.9f) : new Color(0.5f,0.85f,0.6f);
                }
            }
            if (stealthCountText) stealthCountText.text = $"Stealth {stealth}/{_vision.StealthCapPerPlayer}";
            if (controlCountText) controlCountText.text = $"Control {control}/{_vision.ControlCapPerPlayer}";
        }

        // ---------------- Emote wheel ----------------
        private void BuildEmoteWheel()
        {
            if (emoteWheelRoot == null || emoteSlotPrefab == null) return;
            var slots = _emotes.Slots; int n = slots.Length;
            for (int i = 0; i < n; i++)
            {
                var slot = Instantiate(emoteSlotPrefab, emoteWheelRoot); slot.gameObject.SetActive(true);
                float ang = (Mathf.PI * 2f * i / n) - Mathf.PI / 2f;
                ((RectTransform)slot.transform).anchoredPosition = new Vector2(Mathf.Cos(ang) * emoteRadius, -Mathf.Sin(ang) * emoteRadius);
                var t = slot.GetComponentInChildren<Text>(); if (t) t.text = slots[i].Replace("emote_", "");
                string id = slots[i];
                slot.onClick.AddListener(() => PlayEmote(id));
            }
        }
        private void PlayEmote(string id)
        {
            bool ok = _emotes.Play("me", id, 0, 0, _clock);
            if (emoteFeedback) { emoteFeedback.text = ok ? $"Emote: {id.Replace("emote_", "")}" : "On cooldown / muted"; emoteFeedback.color = ok ? new Color(0.55f,0.85f,0.6f) : new Color(0.9f,0.6f,0.4f); }
        }

        // ---------------- Surrender ----------------
        private void OpenSurrender()
        {
            if (!_surrender.Start("me", _clock)) return;
            _surrenderOpen = true; if (surrenderGroup) surrenderGroup.alpha = 1f;
            if (surrenderPrompt) surrenderPrompt.text = "Surrender the match?";
            UpdateSurrender();
        }
        private void CastSurrender(bool yes) { _surrender.CastVote("ally", yes); UpdateSurrender(); }
        private void UpdateSurrender()
        {
            var r = _surrender.Evaluate(_clock);
            if (surrenderTally) surrenderTally.text = $"Yes {_surrender.Yes()} / {_surrender.RequiredYes()} needed";
            if (surrenderTimer) surrenderTimer.text = $"{Mathf.CeilToInt(_surrender.RemainingWindow(_clock))}s";
            if (r == VoteResult.Passed) { if (surrenderPrompt) surrenderPrompt.text = "Surrender passed."; CloseSurrender(); }
            else if (r == VoteResult.Failed) { if (surrenderPrompt) surrenderPrompt.text = "Surrender failed."; CloseSurrender(); }
        }
        private void CloseSurrender() { StartCoroutine(CloseAfter(1.2f)); }
        private IEnumerator CloseAfter(float d) { yield return new WaitForSeconds(d); _surrenderOpen = false; if (surrenderGroup) surrenderGroup.alpha = 0f; }

        // ---------------- Damage numbers ----------------
        public void SpawnDamage(DamageInstance d, float armor, float magicResist, Vector2 anchoredPos)
        {
            if (damageLayer == null || damageNumberPrefab == null) return;
            float dealt = DamageModel.Mitigate(d, armor, magicResist);
            var t = Instantiate(damageNumberPrefab, damageLayer); t.gameObject.SetActive(true);
            ((RectTransform)t.transform).anchoredPosition = anchoredPos;
            t.text = Mathf.RoundToInt(dealt).ToString();
            t.color = d.type == DamageType.True ? new Color(0.95f,0.95f,0.95f) : d.crit ? new Color(1f,0.75f,0.3f) : d.type == DamageType.Physical ? new Color(0.95f,0.65f,0.4f) : new Color(0.6f,0.6f,0.95f);
            t.fontSize = d.crit ? 34 : (d.type == DamageType.True ? 28 : 24);
            StartCoroutine(FloatNumber(t));
        }
        private IEnumerator FloatNumber(Text t)
        {
            var rt = (RectTransform)t.transform; Vector2 start = rt.anchoredPosition; float e = 0f, life = 0.9f;
            while (e < life) { e += Time.deltaTime; float k = e / life; rt.anchoredPosition = start + new Vector2(0, 80f * k); var c = t.color; c.a = 1f - k; t.color = c; yield return null; }
            Destroy(t.gameObject);
        }

        // ---------------- demo hooks ----------------
        private void DemoGrantBuff()
        {
            var seq = new[] { Blessing.Glacial, Blessing.Tempest, Blessing.Earthen, Blessing.Tidal, Blessing.DivineFavor };
            _buffs.Grant(seq[_demoBuffIndex % seq.Length], _clock, 60f); _demoBuffIndex++; RefreshBuffs();
        }
        private void DemoPlaceWard()
        {
            var spots = new (float x, float z, WardType t)[] { (60,-40,WardType.Stealth), (-70,60,WardType.Farsight), (30,30,WardType.Stealth), (-20,-60,WardType.Stealth) };
            var s = spots[_demoWardIndex % spots.Length]; _vision.Place("me", s.t, s.x, s.z, _clock); _demoWardIndex++; RefreshVision();
        }
        private void DemoSweep() { _vision.Sweep(0, 0, 40f); RefreshVision(); }
        private void DemoSpawnDamage()
        {
            var kinds = new[]
            {
                new DamageInstance { type = DamageType.Physical, amount = Random.Range(120,260) },
                new DamageInstance { type = DamageType.Magic, amount = Random.Range(140,300) },
                new DamageInstance { type = DamageType.Physical, amount = Random.Range(200,420), crit = true, critMultiplier = 1.8f },
                new DamageInstance { type = DamageType.True, amount = Random.Range(80,180) },
            };
            var d = kinds[Random.Range(0, kinds.Length)];
            var pos = new Vector2(Random.Range(-120, 120), Random.Range(-40, 40));
            SpawnDamage(d, armor: 80, magicResist: 60, pos);
        }

        // ---------------- helpers ----------------
        private static string FormatClock(float s) { int m = (int)(s / 60), sec = (int)(s % 60); return $"{m:00}:{sec:00}"; }
        private static string Short(Blessing b) => b switch { Blessing.ZantetsukenEdge => "Edge", Blessing.DivineFavor => "Favor", _ => b.ToString() };
        private static Color BlessingColor(Blessing b) => b switch
        {
            Blessing.Infernal => new Color(0.9f,0.4f,0.3f), Blessing.Glacial => new Color(0.5f,0.75f,0.95f),
            Blessing.Tempest => new Color(0.6f,0.85f,0.7f), Blessing.Earthen => new Color(0.7f,0.6f,0.4f),
            Blessing.Tidal => new Color(0.45f,0.7f,0.9f), Blessing.Judgment => new Color(0.85f,0.8f,0.5f),
            Blessing.ZantetsukenEdge => new Color(0.95f,0.8f,0.35f), Blessing.DivineFavor => new Color(0.85f,0.7f,0.95f),
            _ => new Color(0.6f,0.7f,0.8f)
        };
    }
}

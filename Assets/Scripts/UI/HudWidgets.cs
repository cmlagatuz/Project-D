// Project D  |  Codename: Zantetsuken
// HudWidgets.cs  (HUD UI — recall bar, ability level-up bar, status legend)
//
// Three in-match HUD widgets bound to the HudSystems cores:
//   - RecallBarWidget: a channel bar that fills over the recall duration; interrupts
//     hide it. Drive with Begin()/Interrupt(); it ticks itself.
//   - AbilityLevelUpBar: shows a "+" prompt on each ability slot when a skill point is
//     available and the slot can rank up (ult gated by level); tapping ranks it up.
//   - StatusLegendPanel: lists status effects with their descriptions (symbol legend).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.HUD;

namespace ProjectD.HUDUI
{
    public sealed class RecallBarWidget : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image fill;
        [SerializeField] private Text label;
        private readonly RecallChannel _channel = new();

        private void Awake()
        {
            if (group) group.alpha = 0f;
            _channel.OnCompleted += () => { if (label) label.text = "Recalled"; };
            _channel.OnInterrupted += () => { if (group) group.alpha = 0f; };
        }

        public void SetDuration(float seconds) => _channel.ChannelSeconds = seconds;
        public void Begin() { _channel.Begin(); if (group) group.alpha = 1f; if (label) label.text = "Recalling..."; }
        public void Interrupt() => _channel.Interrupt();

        private void Update()
        {
            if (!_channel.Active) return;
            _channel.Tick(Time.deltaTime);
            if (fill) fill.fillAmount = _channel.Progress;
            if (_channel.Completed) { if (group) group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.deltaTime); }
        }
    }

    public sealed class AbilityLevelUpBar : MonoBehaviour
    {
        [System.Serializable] public struct SlotUI { public Button plusButton; public Text rankText; public GameObject plusGlow; }
        [SerializeField] private SlotUI qSlot, wSlot, eSlot, rSlot;
        [SerializeField] private Text pointsText;
        private readonly AbilityLevelUp _lvl = new();

        private void Start()
        {
            Hook(qSlot, AbilitySlot.Q); Hook(wSlot, AbilitySlot.W); Hook(eSlot, AbilitySlot.E); Hook(rSlot, AbilitySlot.Ultimate);
            Refresh();
        }

        public void ChampionLevelUp() { _lvl.OnChampionLevelUp(); Refresh(); }

        private void Hook(SlotUI ui, AbilitySlot slot)
        {
            if (ui.plusButton) ui.plusButton.onClick.AddListener(() => { if (_lvl.RankUp(slot)) Refresh(); });
        }

        private void Refresh()
        {
            SetSlot(qSlot, AbilitySlot.Q); SetSlot(wSlot, AbilitySlot.W); SetSlot(eSlot, AbilitySlot.E); SetSlot(rSlot, AbilitySlot.Ultimate);
            if (pointsText) pointsText.text = _lvl.SkillPoints > 0 ? $"{_lvl.SkillPoints} pts" : "";
        }

        private void SetSlot(SlotUI ui, AbilitySlot slot)
        {
            int rank = _lvl.RankOf(slot);
            bool can = _lvl.CanRankUp(slot);
            if (ui.rankText) ui.rankText.text = new string('|', rank);   // pip rank
            if (ui.plusButton) ui.plusButton.interactable = can;
            if (ui.plusGlow) ui.plusGlow.SetActive(can);
        }
    }

    public sealed class StatusLegendPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform listParent;
        [SerializeField] private RectTransform rowPrefab;   // has [icon][name][desc]

        private void Start() => Build();

        private void Build()
        {
            if (listParent == null || rowPrefab == null) return;
            foreach (var e in StatusEffectLegend.All())
            {
                var row = Instantiate(rowPrefab, listParent);
                row.gameObject.SetActive(true);
                var texts = row.GetComponentsInChildren<Text>();
                if (texts.Length >= 2) { texts[0].text = e.ToString(); texts[1].text = StatusEffectLegend.Desc(e); }
                var icon = row.GetComponentInChildren<Image>();
                if (icon) icon.color = ColorFor(e);
            }
        }

        private static Color ColorFor(StatusEffect e) => e switch
        {
            StatusEffect.Stun or StatusEffect.Root or StatusEffect.Silence or StatusEffect.Knockup or StatusEffect.Airborne => new Color(0.9f,0.5f,0.4f),
            StatusEffect.Slow => new Color(0.5f,0.6f,0.9f),
            StatusEffect.Shield or StatusEffect.Heal or StatusEffect.Cleansed => new Color(0.5f,0.85f,0.6f),
            StatusEffect.Burn or StatusEffect.Exposed => new Color(0.9f,0.4f,0.4f),
            _ => new Color(0.6f,0.6f,0.7f)
        };
    }
}

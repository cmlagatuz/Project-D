// Project D  |  Codename: Zantetsuken
// P8ContentViews.cs  (P8 UI — content screens bound to the P8 cores)
//
// Three content screens wired to the tested P8 systems:
//   - ItemBuildTreeView : shows a completed item + its component tree with recipe/upgrade
//     cost and an affordability state; buying grants ownership + recomputes.
//   - RunePageEditorView : pick primary/secondary paths + keystone + slot runes, live
//     validation + aggregated stat readout.
//   - CoachingReportView : renders post-match insights (praise/tip/warning) from a match
//     profile.
// Server-authoritative in a real build; these render/act on server-owned data.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.P8.ItemForge;
using ProjectD.P8.Runes;
using ProjectD.P8.Coaching;

namespace ProjectD.P8UI
{
    // ---------------- Item build tree ----------------
    public sealed class ItemBuildTreeView : MonoBehaviour
    {
        [SerializeField] private RectTransform completedParent;   // the top item node
        [SerializeField] private RectTransform componentsParent;  // its components row
        [SerializeField] private RectTransform nodePrefab;        // [name][cost]
        [SerializeField] private Text costText, affordText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyLabel;

        private ItemCatalog _cat;
        private string _itemId;
        private int _gold;
        private readonly HashSet<string> _owned = new();

        public event System.Action<string, int> OnBought;

        public void Bind(ItemCatalog cat, string itemId, int gold)
        {
            _cat = cat; _itemId = itemId; _gold = gold;
            if (buyButton) buyButton.onClick.AddListener(Buy);
            Refresh();
        }

        private void Refresh()
        {
            var def = _cat.Get(_itemId);
            if (def == null) return;

            // Completed node.
            if (completedParent != null) { foreach (Transform c in completedParent) Destroy(c.gameObject); MakeNode(completedParent, def.name ?? def.id, def.totalCost, _owned.Contains(def.id)); }
            // Component row.
            if (componentsParent != null)
            {
                foreach (Transform c in componentsParent) Destroy(c.gameObject);
                foreach (var compId in def.componentIds)
                {
                    var cd = _cat.Get(compId);
                    MakeNode(componentsParent, cd?.name ?? compId, cd?.totalCost ?? 0, _owned.Contains(compId), compId);
                }
            }

            int recipe = _cat.RecipeCost(_itemId, out var missing);
            int upgrade = _cat.UpgradeCost(_itemId, _owned);
            if (costText) costText.text = missing.Count > 0 ? $"Missing: {string.Join(",", missing)}" : $"Recipe: {recipe}g   Upgrade: {upgrade}g";
            bool owned = _owned.Contains(_itemId);
            bool affordable = missing.Count == 0 && _gold >= upgrade && !owned;
            if (affordText) { affordText.text = $"Gold: {_gold}"; affordText.color = affordable ? new Color(0.55f,0.85f,0.6f) : new Color(0.9f,0.6f,0.4f); }
            if (buyButton) buyButton.interactable = affordable;
            if (buyLabel) buyLabel.text = owned ? "OWNED" : affordable ? $"BUY  {upgrade}g" : (missing.Count > 0 ? "INCOMPLETE" : "NOT ENOUGH GOLD");
        }

        private void Buy()
        {
            int upgrade = _cat.UpgradeCost(_itemId, _owned);
            if (_gold < upgrade) return;
            _gold -= upgrade; _owned.Add(_itemId);
            var def = _cat.Get(_itemId); if (def != null) foreach (var c in def.componentIds) _owned.Add(c);   // consumed into the build
            OnBought?.Invoke(_itemId, upgrade);
            Refresh();
        }

        private RectTransform MakeNode(RectTransform parent, string name, int cost, bool owned, string clickId = null)
        {
            var node = Instantiate(nodePrefab, parent); node.gameObject.SetActive(true);
            var t = node.GetComponentsInChildren<Text>();
            if (t.Length > 0) t[0].text = name;
            if (t.Length > 1) t[1].text = cost > 0 ? $"{cost}g" : "";
            var img = node.GetComponent<Image>();
            if (img) img.color = owned ? new Color(0.12f,0.20f,0.14f,0.95f) : new Color(0.14f,0.16f,0.22f,0.95f);
            return node;
        }
    }

    // ---------------- Rune page editor ----------------
    public sealed class RunePageEditorView : MonoBehaviour
    {
        [SerializeField] private Button[] primaryPathButtons;    // one per RunePath
        [SerializeField] private Button[] secondaryPathButtons;
        [SerializeField] private RectTransform keystoneParent, primaryParent, secondaryParent;
        [SerializeField] private Button runeButtonPrefab;        // reused for keystone + slots
        [SerializeField] private Text validText, statsText;

        private System.Func<string, Rune> _lookup;
        private IReadOnlyList<Rune> _all;
        private readonly RunePage _page = new() { primaryPath = RunePath.Precision, secondaryPath = RunePath.Sorcery };

        public void Bind(IReadOnlyList<Rune> allRunes)
        {
            _all = allRunes;
            var map = new Dictionary<string, Rune>(); foreach (var r in allRunes) map[r.id] = r;
            _lookup = id => map.TryGetValue(id, out var r) ? r : null;

            HookPaths(primaryPathButtons, true);
            HookPaths(secondaryPathButtons, false);
            RebuildRuneChoices();
            Refresh();
        }

        private void HookPaths(Button[] buttons, bool primary)
        {
            if (buttons == null) return;
            for (int i = 0; i < buttons.Length; i++)
            {
                var path = (RunePath)i;
                if (buttons[i]) buttons[i].onClick.AddListener(() =>
                {
                    if (primary) { _page.primaryPath = path; _page.keystoneId = ""; _page.primaryRunes.Clear(); }
                    else { _page.secondaryPath = path; _page.secondaryRunes.Clear(); }
                    RebuildRuneChoices(); Refresh();
                });
            }
        }

        private void RebuildRuneChoices()
        {
            // Keystones of the primary path.
            Fill(keystoneParent, r => r.keystone && r.path == _page.primaryPath, id => { _page.keystoneId = id; Refresh(); }, () => new List<string> { _page.keystoneId });
            // Primary (non-keystone) runes.
            Fill(primaryParent, r => !r.keystone && r.path == _page.primaryPath, id => ToggleSlot(_page.primaryRunes, id, RunePage.PrimarySlots), () => _page.primaryRunes);
            // Secondary runes.
            Fill(secondaryParent, r => !r.keystone && r.path == _page.secondaryPath, id => ToggleSlot(_page.secondaryRunes, id, RunePage.SecondarySlots), () => _page.secondaryRunes);
        }

        private void Fill(RectTransform parent, System.Func<Rune, bool> filter, System.Action<string> onClick, System.Func<IReadOnlyList<string>> selected)
        {
            if (parent == null || runeButtonPrefab == null) return;
            foreach (Transform c in parent) Destroy(c.gameObject);
            foreach (var r in _all)
            {
                if (!filter(r)) continue;
                var b = Instantiate(runeButtonPrefab, parent); b.gameObject.SetActive(true);
                var t = b.GetComponentInChildren<Text>(); if (t) t.text = r.name ?? r.id;
                string id = r.id;
                b.onClick.AddListener(() => { onClick(id); RebuildRuneChoices(); Refresh(); });
                var img = b.GetComponent<Image>();
                bool sel = selected().Contains(r.id) || _page.keystoneId == r.id;
                if (img) img.color = sel ? new Color(0.30f,0.55f,0.40f) : new Color(0.16f,0.18f,0.24f);
            }
        }

        private void ToggleSlot(List<string> slot, string id, int cap)
        {
            if (slot.Contains(id)) slot.Remove(id);
            else if (slot.Count < cap) slot.Add(id);
        }

        private void Refresh()
        {
            bool ok = RuneValidator.Validate(_page, _lookup, out var err);
            if (validText) { validText.text = ok ? "Valid rune page \u2713" : err; validText.color = ok ? new Color(0.55f,0.85f,0.6f) : new Color(0.9f,0.6f,0.4f); }
            if (statsText)
            {
                var stats = RuneValidator.AggregateStats(_page, _lookup);
                var sb = new System.Text.StringBuilder("Stats: ");
                foreach (var kv in stats) sb.Append($"{kv.Key} +{kv.Value:0}  ");
                statsText.text = stats.Count > 0 ? sb.ToString() : "Stats: —";
            }
        }
    }

    // ---------------- Coaching report ----------------
    public sealed class CoachingReportView : MonoBehaviour
    {
        [SerializeField] private RectTransform listParent;
        [SerializeField] private RectTransform rowPrefab;   // [icon][message]

        public void Bind(MatchProfile profile)
        {
            if (listParent == null || rowPrefab == null) return;
            foreach (Transform c in listParent) Destroy(c.gameObject);
            foreach (var insight in CoachingInsights.Analyze(profile))
            {
                var row = Instantiate(rowPrefab, listParent); row.gameObject.SetActive(true);
                var t = row.GetComponentInChildren<Text>(); if (t) t.text = insight.message;
                var icon = row.GetComponentInChildren<Image>();
                if (icon) icon.color = insight.severity switch
                {
                    InsightSeverity.Praise => new Color(0.45f,0.85f,0.5f),
                    InsightSeverity.Warning => new Color(0.9f,0.4f,0.4f),
                    _ => new Color(0.95f,0.8f,0.35f)
                };
            }
        }
    }
}

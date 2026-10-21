// Project D  |  Codename: Zantetsuken
// ChampionSelectView.cs  (Champion Select screen binder)
//
// Binds a ChampionSelectController to the on-screen UI: loads all champion assets
// into the roster grid, shows the hovered champion's full detail (title, world,
// release date, lore, playstyle, strengths/weaknesses, tips, stats, and the full
// Passive/Q/W/E/Ultimate kit), drives the step timer + turn banner, fills the two
// teams' pick slots, and hands off to the match when the draft completes.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Data;

namespace ProjectD.UI.ChampionSelect
{
    public sealed class ChampionSelectView : MonoBehaviour
    {
        [Header("Roster Grid")]
        [SerializeField] private RectTransform gridContent;
        [SerializeField] private ChampionCard cardPrefab;

        [Header("Detail Panel")]
        [SerializeField] private Text detailName;
        [SerializeField] private Text detailTitleWorld;
        [SerializeField] private Text detailLore;
        [SerializeField] private Text detailPlaystyle;
        [SerializeField] private Text detailStrengths;
        [SerializeField] private Text detailWeaknesses;
        [SerializeField] private Text detailTips;
        [SerializeField] private Text detailStats;
        [SerializeField] private Text detailAbilities;
        [SerializeField] private Image detailRoleBanner;

        [Header("Draft State")]
        [SerializeField] private Text turnBanner;
        [SerializeField] private Text timerLabel;
        [SerializeField] private Button lockInButton;
        [SerializeField] private Text[] bluePickLabels = new Text[3];
        [SerializeField] private Text[] redPickLabels = new Text[3];
        [SerializeField] private Text bansLabel;

        private ChampionSelectController _controller;
        private readonly Dictionary<string, ChampionDefinition> _byId = new();
        private readonly Dictionary<string, ChampionCard> _cards = new();

        private void Start()
        {
            LoadChampions();
            BuildGrid();
            WireController();
        }

        private void LoadChampions()
        {
            // At runtime we load the generated ScriptableObjects from Resources or
            // an addressable group. For the prototype we accept a pre-populated list
            // injected by the scene builder via Resources.LoadAll.
            var all = Resources.LoadAll<ChampionDefinition>("");
            foreach (var c in all)
                if (c != null && !_byId.ContainsKey(c.championId))
                    _byId[c.championId] = c;

            // Editor scene builder may also assign directly:
            foreach (var c in _injected)
                if (c != null && !_byId.ContainsKey(c.championId))
                    _byId[c.championId] = c;
        }

        [SerializeField] private List<ChampionDefinition> _injected = new();
        public void InjectChampions(List<ChampionDefinition> list) => _injected = list;

        private void BuildGrid()
        {
            if (gridContent == null || cardPrefab == null) return;
            foreach (var kvp in _byId)
            {
                var card = Instantiate(cardPrefab, gridContent);
                card.Bind(kvp.Value);
                card.OnHovered += id => _controller?.Hover(id);
                card.OnClicked += id => _controller?.Hover(id);
                _cards[kvp.Key] = card;
            }
        }

        private void WireController()
        {
            _controller = new ChampionSelectController(secondsPerStep: 30f);
            _controller.OnStepStarted += HandleStepStarted;
            _controller.OnHoverChanged += ShowDetail;
            _controller.OnTimerTick += t => { if (timerLabel != null) timerLabel.text = Mathf.CeilToInt(t).ToString(); };
            _controller.OnPickLocked += HandlePickLocked;
            _controller.OnBanLocked += HandleBanLocked;
            _controller.OnDraftComplete += HandleComplete;

            if (lockInButton != null)
                lockInButton.onClick.AddListener(() => _controller.LockIn());

            _controller.Begin();
        }

        private void Update() => _controller?.Tick(Time.deltaTime);

        // ---- Detail panel ---- //
        private void ShowDetail(string championId)
        {
            if (!_byId.TryGetValue(championId, out var c)) return;

            foreach (var kvp in _cards) kvp.Value.SetHighlighted(kvp.Key == championId);

            if (detailName != null) detailName.text = c.displayName;
            if (detailTitleWorld != null)
                detailTitleWorld.text = $"{c.sourceWorld}  •  First released {c.sourceReleaseDate}  •  {c.primaryRole}/{c.secondaryRole}";
            if (detailLore != null) detailLore.text = c.loreSummary;
            if (detailPlaystyle != null) detailPlaystyle.text = c.playstyle;
            if (detailStrengths != null) detailStrengths.text = "Strengths:  " + c.strengths;
            if (detailWeaknesses != null) detailWeaknesses.text = "Weaknesses:  " + c.weaknesses;
            if (detailTips != null) detailTips.text = "Combos & Tips:  " + c.tipsAndCombos;
            if (detailRoleBanner != null) detailRoleBanner.color = ChampionCard.RoleColor(c.primaryRole);

            if (detailStats != null)
                detailStats.text =
                    $"HP {c.baseHealth:F0} (+{c.healthGrowth:F0})    AD {c.baseAttackDamage:F0}    AP {c.baseMagicPower:F0}\n" +
                    $"ARM {c.baseArmor:F0}    MR {c.baseMagicResist:F0}    MS {c.baseMoveSpeed:F2}    " +
                    $"RNG {c.baseAttackRange:F2}    Diff {c.mechanicalDifficulty}/5";

            if (detailAbilities != null)
                detailAbilities.text =
                    Ability("Passive", c.passive) +
                    Ability("Q", c.abilityQ) +
                    Ability("W", c.abilityW) +
                    Ability("E", c.abilityE) +
                    Ability("Ultimate (R)", c.ultimate);
        }

        private static string Ability(string slot, AbilityDefinition a)
        {
            if (a == null) return $"<b>{slot}:</b> —\n";
            string cd = a.baseCooldown > 0 ? $"  [CD {a.baseCooldown:F0}s" : "  [";
            string cost = a.resourceCost > 0 ? $" · {a.resourceCost:F0} cost]" : "]";
            return $"<b>{slot} — {a.displayName}</b>{cd}{cost}\n{a.designerNotes}\n\n";
        }

        // ---- Draft state ---- //
        private void HandleStepStarted(DraftStep step)
        {
            if (turnBanner == null) return;
            string verb = step.Type == DraftStepType.Ban ? "BAN" : "PICK";
            turnBanner.text = $"{step.Team} team to {verb}";
            turnBanner.color = step.Team == DraftTeam.Blue
                ? new Color(0.4f, 0.65f, 1f) : new Color(1f, 0.45f, 0.45f);
            RefreshSelectable();
        }

        private void RefreshSelectable()
        {
            foreach (var kvp in _cards)
                kvp.Value.SetSelectable(_controller.IsSelectable(kvp.Key));
        }

        private void HandlePickLocked(DraftTeam team, int slot, string championId)
        {
            string name = _byId.TryGetValue(championId, out var c) ? c.displayName : championId;
            var arr = team == DraftTeam.Blue ? bluePickLabels : redPickLabels;
            if (slot >= 0 && slot < arr.Length && arr[slot] != null) arr[slot].text = name;
            RefreshSelectable();
        }

        private void HandleBanLocked(DraftTeam team, string championId)
        {
            if (bansLabel != null)
            {
                string name = _byId.TryGetValue(championId, out var c) ? c.displayName : championId;
                bansLabel.text = string.IsNullOrEmpty(bansLabel.text) ? $"Bans: {name}" : bansLabel.text + ", " + name;
            }
            RefreshSelectable();
        }

        private void HandleComplete()
        {
            if (turnBanner != null) turnBanner.text = "Draft complete — entering the arena…";
            if (timerLabel != null) timerLabel.text = "0";
            if (lockInButton != null) lockInButton.interactable = false;
            // TODO: hand the locked roster to MatchBootstrap / scene load.
            Debug.Log("[Project D] Draft complete. Blue: " +
                string.Join(", ", _controller.GetPick(DraftTeam.Blue,0), _controller.GetPick(DraftTeam.Blue,1), _controller.GetPick(DraftTeam.Blue,2)));
        }
    }
}

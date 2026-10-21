// Project D  |  Codename: Zantetsuken
// LiveOpsScreen.cs  (Live-ops UI — missions + weekly chest + events)
//
// Binds MissionsModel + WeeklyChestModel to the UI: cadence tabs (Daily/Weekly/Event)
// filter the mission grid, each card claims its rewards, and a weekly-chest widget
// shows fragment progress, assembled chests + keys, and an OPEN action. Claiming a
// mission grants XP/KP (to the account) and any chest fragments (to the chest).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.LiveOps;

namespace ProjectD.LiveOpsUI
{
    public sealed class LiveOpsScreen : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private Button[] cadenceButtons;    // Daily, Weekly, Event
        [Header("Mission grid")]
        [SerializeField] private RectTransform gridParent;
        [SerializeField] private MissionCard cardPrefab;

        [Header("Weekly chest widget")]
        [SerializeField] private Image chestFragmentFill;
        [SerializeField] private Text chestFragmentText;     // "2 / 4 fragments"
        [SerializeField] private Text chestCountText;        // "Chests: 1"
        [SerializeField] private Text keyCountText;          // "Keys: 2"
        [SerializeField] private Button openChestButton;
        [SerializeField] private Text openChestLabel;
        [SerializeField] private Text chestFeedback;

        [Header("Rewards feedback")]
        [SerializeField] private Text feedbackLabel;

        private readonly MissionsModel _missions = new();
        private readonly WeeklyChestModel _chest = new();
        private readonly List<MissionCard> _cards = new();
        private MissionCadence _activeCadence = MissionCadence.Daily;

        /// <summary>Fired when mission rewards are granted (kind,amount) for the account.</summary>
        public event System.Action<MissionRewardKind, int> OnRewardGranted;

        public MissionsModel Missions => _missions;
        public WeeklyChestModel Chest => _chest;

        public void Bind(IEnumerable<MissionDefinition> defs, WeeklyChestSeed chestSeed = null)
        {
            foreach (var d in defs) _missions.Register(d);
            if (chestSeed != null)
            {
                _chest.FragmentsPerChest = chestSeed.fragmentsPerChest;
                _chest.WeeklyChestCap = chestSeed.weeklyCap;
                _chest.Fragments = chestSeed.fragments;
                _chest.Chests = chestSeed.chests;
                _chest.Keys = chestSeed.keys;
            }
            HookTabs();
            if (openChestButton) openChestButton.onClick.AddListener(OpenChest);
            SelectCadence(MissionCadence.Daily);
            RefreshChest();
        }

        private void HookTabs()
        {
            var order = new[] { MissionCadence.Daily, MissionCadence.Weekly, MissionCadence.Event };
            for (int i = 0; i < cadenceButtons.Length && i < order.Length; i++)
            {
                var cad = order[i];
                if (cadenceButtons[i]) cadenceButtons[i].onClick.AddListener(() => SelectCadence(cad));
            }
        }

        private void SelectCadence(MissionCadence cadence)
        {
            _activeCadence = cadence;
            var order = new[] { MissionCadence.Daily, MissionCadence.Weekly, MissionCadence.Event };
            for (int i = 0; i < cadenceButtons.Length && i < order.Length; i++)
                Highlight(cadenceButtons[i], order[i] == cadence);
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            foreach (var c in _cards) if (c) Destroy(c.gameObject);
            _cards.Clear();
            if (gridParent == null || cardPrefab == null) return;

            foreach (var def in _missions.All)
            {
                if (def.cadence != _activeCadence) continue;
                var card = Instantiate(cardPrefab, gridParent);
                card.Bind(def, _missions.State(def.missionId));
                card.OnClaim += ClaimMission;
                _cards.Add(card);
            }
        }

        private void ClaimMission(string missionId)
        {
            var rewards = _missions.Claim(missionId);
            if (rewards == null) return;
            foreach (var r in rewards)
            {
                if (r.kind == MissionRewardKind.ChestFragment) _chest.AddFragments(r.amount);
                OnRewardGranted?.Invoke(r.kind, r.amount);
            }
            Feedback("Rewards claimed!", true);
            RefreshGrid();
            RefreshChest();
        }

        private void OpenChest()
        {
            if (_chest.OpenChest())
            {
                ChestFeedback("Chest opened! (reward rolled server-side)", true);
                RefreshChest();
            }
            else ChestFeedback(_chest.Chests <= 0 ? "No chests to open." : "No keys.", false);
        }

        private void RefreshChest()
        {
            if (chestFragmentFill) chestFragmentFill.fillAmount = _chest.FragmentProgress;
            if (chestFragmentText) chestFragmentText.text = $"{_chest.Fragments} / {_chest.FragmentsPerChest} fragments";
            if (chestCountText) chestCountText.text = $"Chests: {_chest.Chests}";
            if (keyCountText) keyCountText.text = $"Keys: {_chest.Keys}";
            if (openChestButton) openChestButton.interactable = _chest.CanOpen;
            if (openChestLabel) openChestLabel.text = _chest.CanOpen ? "OPEN CHEST" : (_chest.Chests <= 0 ? "NO CHEST" : "NEED KEY");
        }

        // Demo helper: simulate progressing a mission (server does this from match results).
        public void SimulateProgress(string missionId, int amount) { _missions.AddProgress(missionId, amount); RefreshGrid(); }
        public void AddKeys(int n) { _chest.AddKeys(n); RefreshChest(); }

        private void Feedback(string msg, bool ok) { if (feedbackLabel) { feedbackLabel.text = msg; feedbackLabel.color = ok ? new Color(0.55f,0.85f,0.6f) : new Color(0.95f,0.5f,0.45f); } }
        private void ChestFeedback(string msg, bool ok) { if (chestFeedback) { chestFeedback.text = msg; chestFeedback.color = ok ? new Color(0.55f,0.85f,0.6f) : new Color(0.95f,0.5f,0.45f); } }
        private static void Highlight(Button b, bool on) { if (!b) return; var img = b.GetComponent<Image>(); if (img) img.color = on ? new Color(0.36f,0.62f,0.92f) : new Color(0.16f,0.18f,0.24f); }
    }

    // Optional starting chest state for the screen.
    [System.Serializable] public sealed class WeeklyChestSeed { public int fragmentsPerChest = 4, weeklyCap = 2, fragments, chests, keys; }
}

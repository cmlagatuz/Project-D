// Project D  |  Codename: Zantetsuken
// ModeSelectView.cs  (UI — mode selection: Ranked/Normal vs World Rift)
//
// The pre-queue screen. Ranked/Normal cards always show the canonical map (Crystalline
// Rift) — no map choice, per competitive fairness. The World Rift card shows a rotating
// realm; picking it rolls a random world and shows a "realm reveal" splash with the
// world's name + modifier + Manifestation before queueing. Bound to the mode + world
// cores; enforces the map policy in code.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Modes;
using ProjectD.Worlds;

namespace ProjectD.WorldRiftUI
{
    public sealed class ModeSelectView : MonoBehaviour
    {
        [Header("Mode cards")]
        [SerializeField] private Button rankedButton, normalButton, coopButton, worldRiftButton;
        [SerializeField] private Text selectedModeText, mapPolicyText, queueButtonLabel;
        [SerializeField] private Button queueButton;

        [Header("Realm reveal (World Rift)")]
        [SerializeField] private GameObject realmRevealPanel;
        [SerializeField] private CanvasGroup realmRevealGroup;
        [SerializeField] private Text realmNameText, realmModifierText, realmBossText;
        [SerializeField] private Image realmBanner;
        [SerializeField] private Button realmConfirmButton, realmRerollButton;

        private readonly WorldSelector _selector = new();
        private GameMode _selected = GameMode.RankedSolo;
        private WorldDef _rolledWorld;
        private long _seed;

        public event System.Action<GameMode, string> OnQueue;   // (mode, mapId)

        private void Start()
        {
            if (rankedButton) rankedButton.onClick.AddListener(() => Select(GameMode.RankedSolo));
            if (normalButton) normalButton.onClick.AddListener(() => Select(GameMode.NormalDraft));
            if (coopButton) coopButton.onClick.AddListener(() => Select(GameMode.CoOpVsAI));
            if (worldRiftButton) worldRiftButton.onClick.AddListener(() => Select(GameMode.WorldRift));
            if (queueButton) queueButton.onClick.AddListener(EnterQueue);
            if (realmConfirmButton) realmConfirmButton.onClick.AddListener(ConfirmRealm);
            if (realmRerollButton) realmRerollButton.onClick.AddListener(RollRealm);

            if (realmRevealPanel) realmRevealPanel.SetActive(false);
            _seed = System.DateTime.UtcNow.Ticks;
            Select(GameMode.RankedSolo);
        }

        private void Select(GameMode mode)
        {
            _selected = mode;
            Highlight(rankedButton, mode == GameMode.RankedSolo);
            Highlight(normalButton, mode == GameMode.NormalDraft);
            Highlight(coopButton, mode == GameMode.CoOpVsAI);
            Highlight(worldRiftButton, mode == GameMode.WorldRift);

            var def = GameModeRegistry.Get(mode);
            if (selectedModeText) selectedModeText.text = PrettyMode(mode);
            if (mapPolicyText)
            {
                mapPolicyText.text = def.mapPolicy == MapPolicy.CanonicalOnly
                    ? "Map: Crystalline Rift (canonical)   " + (def.rankedEligible ? "· Ranked LP" : "· No LP")
                    : "Map: Rotating Realm (World Rift)   · Casual only";
                mapPolicyText.color = def.rankedEligible ? new Color(0.95f,0.8f,0.35f) : new Color(0.68f,0.73f,0.83f);
            }
            if (queueButtonLabel) queueButtonLabel.text = mode == GameMode.WorldRift ? "ROLL REALM" : "FIND MATCH";
        }

        private void EnterQueue()
        {
            if (_selected == GameMode.WorldRift) { RollRealm(); ShowRealmReveal(true); }
            else OnQueue?.Invoke(_selected, GameModeRegistry.CanonicalMapId);   // ranked/normal: canonical only
        }

        // ---- World Rift realm reveal ----
        private void RollRealm()
        {
            _seed += 977;
            _rolledWorld = _selector.Pick(_seed);
            if (_rolledWorld == null) return;
            if (realmNameText) realmNameText.text = _rolledWorld.name;
            if (realmModifierText) realmModifierText.text = WorldModifierEngine.Describe(_rolledWorld.modifier);
            if (realmBossText) realmBossText.text = $"Manifestation: {Pretty(_rolledWorld.manifestationId)}";
            if (realmBanner) realmBanner.color = WorldTint(_rolledWorld.id);
        }

        private void ShowRealmReveal(bool show)
        {
            if (realmRevealPanel) realmRevealPanel.SetActive(show);
            if (show && realmRevealGroup) StartCoroutine(FadeIn(realmRevealGroup));
        }

        private void ConfirmRealm()
        {
            if (_rolledWorld == null) return;
            // Guardrail: World Rift must be a rotating (non-canonical) realm.
            if (!GameModeRegistry.IsValidMap(GameMode.WorldRift, _rolledWorld.id)) return;
            ShowRealmReveal(false);
            OnQueue?.Invoke(GameMode.WorldRift, _rolledWorld.id);
        }

        private System.Collections.IEnumerator FadeIn(CanvasGroup g)
        {
            float t = 0f; g.alpha = 0f;
            while (t < 0.3f) { t += Time.unscaledDeltaTime; g.alpha = Mathf.Clamp01(t / 0.3f); yield return null; }
            g.alpha = 1f;
        }

        private static void Highlight(Button b, bool on) { if (!b) return; var img = b.GetComponent<Image>(); if (img) img.color = on ? new Color(0.36f,0.62f,0.92f) : new Color(0.16f,0.18f,0.24f); }
        private static string PrettyMode(GameMode m) => m switch { GameMode.RankedSolo => "RANKED", GameMode.NormalDraft => "NORMAL", GameMode.CoOpVsAI => "CO-OP vs AI", GameMode.WorldRift => "WORLD RIFT", _ => m.ToString() };
        private static string Pretty(string id) => string.IsNullOrEmpty(id) ? "" : id.Replace("summon_", "").Replace("_", " ");
        private static Color WorldTint(string id) { int h = 0; foreach (char c in id) h = h * 31 + c; var st = Random.state; Random.InitState(h); var col = Color.HSVToRGB(Random.value, 0.5f, 0.7f); Random.state = st; return col; }
    }
}

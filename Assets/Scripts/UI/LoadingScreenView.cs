// Project D  |  Codename: Zantetsuken
// LoadingScreenView.cs  (Loading UI — screen with matchup callout + voice line)
//
// Between Preparation and In-Game: champion focus, progress bar, and a tip/trivia
// carousel that PINS a personalized MATCHUP CALLOUT for the exact enemy laner. It
// also triggers the local champion's first-encounter VOICE LINE for that matchup.
//
// The champion focus splash is a static image for now; animated splash/parallax is
// reserved for Legendary/Ultimate skins later.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.Loading
{
    public sealed class LoadingScreenView : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<ChampionTipSet> championTips = new();
        [SerializeField] private GeneralTipSet generalTips;
        [SerializeField] private string localChampionId = "kain";
        [SerializeField] private string localLane = "Jungle";
        [SerializeField] private List<EnemyPickEntry> enemyPicks = new();   // id + lane
        [SerializeField] private int seed = 12345;

        [Header("Voice interaction")]
        [SerializeField] private VoiceLinePlayer voicePlayer;   // optional; plays on the resolved laner
        [SerializeField] private float voiceDelaySeconds = 1.2f;

        [Header("Champion focus")]
        [SerializeField] private Image championSplash;
        [SerializeField] private Text championName;
        [SerializeField] private Text championSub;

        [Header("Matchup banner")]
        [SerializeField] private Text matchupBanner;    // "YOUR LANE (Jungle) vs Cecil Harvey"

        [Header("Progress")]
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressLabel;
        [SerializeField] private Text statusLabel;

        [Header("Tip carousel")]
        [SerializeField] private Text tipHeading;
        [SerializeField] private Text tipBody;
        [SerializeField] private CanvasGroup tipGroup;
        [SerializeField] private float secondsPerTip = 4.5f;
        [SerializeField] private float fadeSeconds = 0.35f;

        [Header("Behaviour")]
        [SerializeField] private bool simulateProgress = true;
        [SerializeField] private float simulateSeconds = 6f;

        [System.Serializable] public struct EnemyPickEntry { public string championId; public string lane; }

        private readonly TipRotationController _tips = new();
        private readonly LoadingProgress _progress = new();
        private Dictionary<string, ChampionTipSet> _byId;
        private float _tipTimer, _fadeT = 1f, _simT, _voiceTimer;
        private bool _serverReady, _voicePlayed;
        private string _lanerId;

        public event System.Action OnReadyToEnterMatch;

        private void Start()
        {
            _byId = new Dictionary<string, ChampionTipSet>();
            foreach (var c in championTips) if (c && !string.IsNullOrEmpty(c.championId)) _byId[c.championId] = c;

            BuildTipsWithCallout();
            ShowChampionFocus();

            _progress.Register("scene", 3f);
            _progress.Register("assets", 4f);
            _progress.Register("network", 2f);
            _progress.Register("players", 1f);

            ShowNextTip(instant: true);
            if (statusLabel) statusLabel.text = "Loading...";
            _voiceTimer = voiceDelaySeconds;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (simulateProgress) SimulateProgress(dt);

            float total = _progress.Total;
            if (progressFill) progressFill.fillAmount = total;
            if (progressLabel) progressLabel.text = Mathf.RoundToInt(total * 100f) + "%";

            bool localDone = _progress.IsComplete;
            if (statusLabel) statusLabel.text = !localDone ? "Loading..." : (_serverReady ? "Entering the arena..." : "Waiting for players...");
            if (localDone && _serverReady) OnReadyToEnterMatch?.Invoke();

            // Trigger the first-encounter voice line for the resolved laner.
            if (!_voicePlayed && voicePlayer != null && !string.IsNullOrEmpty(_lanerId))
            {
                _voiceTimer -= dt;
                if (_voiceTimer <= 0f) { voicePlayer.PlayEncounter(localChampionId, _lanerId); _voicePlayed = true; }
            }

            TickTips(dt);
        }

        // ---- external hooks ---- //
        public void Report(string taskId, float p) => _progress.Report(taskId, p);
        public void SetAllPlayersReady(bool ready) => _serverReady = ready;

        public void Configure(string localId, string lane, List<EnemyPickEntry> enemies, int matchSeed)
        {
            localChampionId = localId; localLane = lane; enemyPicks = enemies ?? new List<EnemyPickEntry>(); seed = matchSeed;
            _voicePlayed = false; _voiceTimer = voiceDelaySeconds;
            BuildTipsWithCallout(); ShowChampionFocus(); ShowNextTip(instant: true);
        }

        // ---- tips + matchup callout ---- //
        private void BuildTipsWithCallout()
        {
            var champs = new Dictionary<string, ChampionTipView>();
            foreach (var kv in _byId)
                champs[kv.Key] = new ChampionTipView
                {
                    ChampionId = kv.Value.championId, DisplayName = kv.Value.displayName,
                    PlayingTips = kv.Value.playingTips, AgainstTips = kv.Value.playingAgainstTips, Trivia = kv.Value.trivia
                };

            // Resolve the enemy laner and build the personalized callout.
            var picks = new List<EnemyPick>();
            var enemyIds = new List<string>();
            foreach (var e in enemyPicks)
                if (!string.IsNullOrEmpty(e.championId)) { picks.Add(new EnemyPick { ChampionId = e.championId, Lane = e.lane }); enemyIds.Add(e.championId); }

            var matchup = MatchupCallout.Resolve(localLane, picks, champs);
            _lanerId = matchup.HasLaner ? matchup.EnemyChampionId : null;

            if (matchupBanner)
                matchupBanner.text = matchup.HasLaner
                    ? (string.IsNullOrEmpty(matchup.Lane) ? $"Opponent: {matchup.EnemyDisplayName}" : $"Your lane ({matchup.Lane}) vs {matchup.EnemyDisplayName}")
                    : "";

            _tips.Build(localChampionId, enemyIds, champs, generalTips ? generalTips.tips : new string[0], seed,
                        matchup.HasLaner ? matchup.Callout : (TipCard?)null);
        }

        private void TickTips(float dt)
        {
            if (_fadeT < 1f && tipGroup) { _fadeT = Mathf.Min(1f, _fadeT + dt / Mathf.Max(0.01f, fadeSeconds)); tipGroup.alpha = _fadeT; }
            _tipTimer += dt;
            if (_tipTimer >= secondsPerTip) { _tipTimer = 0f; ShowNextTip(false); }
        }

        private void ShowNextTip(bool instant)
        {
            var card = _tips.Next();
            if (tipHeading) tipHeading.text = card.Heading;
            if (tipBody) tipBody.text = card.Body;
            if (tipGroup) { _fadeT = instant ? 1f : 0f; tipGroup.alpha = _fadeT; }
        }

        private void ShowChampionFocus()
        {
            _byId.TryGetValue(localChampionId, out var c);
            if (championName) championName.text = c ? c.displayName : localChampionId;
            if (championSub) championSub.text = c ? $"{c.sourceWorld} - {c.role}" : "";
            if (championSplash && championSplash.sprite == null && c) championSplash.color = RoleTint(c.role);
        }

        private static Color RoleTint(string role) => role switch
        {
            "Vanguard" => new Color(0.30f, 0.45f, 0.80f), "Fighter" => new Color(0.80f, 0.45f, 0.28f),
            "Assassin" => new Color(0.75f, 0.28f, 0.38f), "Marksman" => new Color(0.80f, 0.68f, 0.28f),
            "Mage" => new Color(0.55f, 0.32f, 0.80f), "Support" => new Color(0.30f, 0.70f, 0.48f),
            _ => new Color(0.4f, 0.4f, 0.5f)
        };

        private void SimulateProgress(float dt)
        {
            _simT += dt;
            float f = Mathf.Clamp01(_simT / Mathf.Max(0.1f, simulateSeconds));
            _progress.Report("scene", Mathf.Clamp01(f * 1.4f));
            _progress.Report("assets", Mathf.Clamp01(f * 1.1f));
            _progress.Report("network", Mathf.Clamp01(f * 1.7f));
            _progress.Report("players", Mathf.Clamp01((f - 0.2f) * 1.6f));
            if (f >= 1f) _serverReady = true;
        }
    }
}

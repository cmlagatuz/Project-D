// Project D  |  Codename: Zantetsuken
// MatchmakingView.cs  (Matchmaking UI — mode picker + queue + ready-check)
//
// Binds MatchmakingModel to the UI: mode buttons, a PLAY/queue toggle with an elapsed
// timer + widening bracket estimate, and a MATCH FOUND ready-check (Accept/Decline).
// The queue timer runs locally; the SERVER decides when a match forms (NotifyMatchFound).
// In the demo, a match is found automatically after a few seconds.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Matchmaking;

namespace ProjectD.MatchmakingUI
{
    public sealed class MatchmakingView : MonoBehaviour
    {
        [Header("Mode picker")]
        [SerializeField] private Button[] modeButtons;    // Ranked, Normal, Co-op, Custom
        [SerializeField] private GameMode[] modeValues;   // index-aligned with modeButtons
        [SerializeField] private Text selectedModeText;

        [Header("Queue")]
        [SerializeField] private Button playButton;
        [SerializeField] private Text playButtonLabel;
        [SerializeField] private Text timerText;
        [SerializeField] private Text estimateText;

        [Header("Ready check")]
        [SerializeField] private GameObject readyPanel;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;

        [Header("Demo")]
        [SerializeField] private float demoFindAfterSeconds = 4f;

        private readonly MatchmakingModel _model = new();
        private bool _searching;

        private void Start()
        {
            for (int i = 0; i < modeButtons.Length && i < modeValues.Length; i++)
            {
                var mode = modeValues[i];
                if (modeButtons[i]) modeButtons[i].onClick.AddListener(() => { _model.SelectMode(mode); RefreshMode(); });
            }
            if (playButton) playButton.onClick.AddListener(TogglePlay);
            if (acceptButton) acceptButton.onClick.AddListener(() => { _model.Accept(); ShowReady(false); Feedback("Accepted! Loading..."); });
            if (declineButton) declineButton.onClick.AddListener(() => { _model.Decline(); ShowReady(false); _searching = false; RefreshQueue(); });

            _model.OnMatchFound += () => ShowReady(true);
            ShowReady(false);
            RefreshMode();
            RefreshQueue();
        }

        private void Update()
        {
            if (!_searching) return;
            _model.Tick(Time.deltaTime);
            if (timerText) timerText.text = FormatTime(_model.Elapsed);
            var (min, max) = _model.Bracket();
            if (estimateText) estimateText.text = $"Est. wait ~{_model.Status().EstimatedWaitSeconds}s · MMR {min}-{max}";

            if (_model.State == QueueState.Searching && _model.Elapsed >= demoFindAfterSeconds)
                _model.NotifyMatchFound();
        }

        private void TogglePlay()
        {
            if (_model.State == QueueState.Idle) { _model.StartQueue(); _searching = true; }
            else { _model.Cancel(); _searching = false; }
            RefreshQueue();
        }

        private void RefreshMode()
        {
            if (selectedModeText) selectedModeText.text = _model.SelectedMode.ToString();
            for (int i = 0; i < modeButtons.Length && i < modeValues.Length; i++)
                Highlight(modeButtons[i], modeValues[i] == _model.SelectedMode);
        }

        private void RefreshQueue()
        {
            bool searching = _model.State == QueueState.Searching;
            if (playButtonLabel) playButtonLabel.text = searching ? "CANCEL" : "FIND MATCH";
            if (timerText) timerText.text = searching ? "00:00" : "";
            if (estimateText) estimateText.text = searching ? "Searching..." : "";
        }

        private void ShowReady(bool show)
        {
            if (readyPanel) readyPanel.SetActive(show);
            if (show) _searching = false;
        }

        private void Feedback(string msg) { if (estimateText) estimateText.text = msg; }
        private static string FormatTime(float s) { int m = (int)(s / 60), sec = (int)(s % 60); return $"{m:00}:{sec:00}"; }
        private static void Highlight(Button b, bool on) { if (!b) return; var img = b.GetComponent<Image>(); if (img) img.color = on ? new Color(0.36f,0.62f,0.92f) : new Color(0.16f,0.18f,0.24f); }
    }
}

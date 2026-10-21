// Project D  |  Codename: Zantetsuken
// TutorialView.cs  (Tutorial UI — step callout + progress + rewards)
//
// Presents the guided onboarding flow: the current step's title + instruction, a
// progress dots strip, and a SKIP button. Trigger buttons (or real gameplay) report
// TutorialTrigger events; when the flow completes it shows the total XP/KP earned.
// Binds a set of TutorialSteps via TutorialFlow.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Tutorial;

namespace ProjectD.TutorialUI
{
    public sealed class TutorialView : MonoBehaviour
    {
        [SerializeField] private Text stepTitle;
        [SerializeField] private Text stepBody;
        [SerializeField] private Text stepIndexText;      // "Step 2 / 6"
        [SerializeField] private RectTransform dotsParent;
        [SerializeField] private Image dotPrefab;
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject completePanel;
        [SerializeField] private Text completeRewardText; // "+350 XP  +40 KP"

        private readonly TutorialFlow _flow = new();
        private readonly List<Image> _dots = new();

        public event System.Action<int, int> OnCompleted;  // (xp, kp) to grant to account

        public void Bind(IEnumerable<TutorialStep> steps)
        {
            _flow.SetSteps(steps);
            _flow.OnStepStarted += _ => RefreshStep();
            _flow.OnStepCompleted += _ => RefreshDots();
            _flow.OnFlowCompleted += ShowComplete;

            if (skipButton) skipButton.onClick.AddListener(() => _flow.SkipAll());
            if (completePanel) completePanel.SetActive(false);

            BuildDots();
            _flow.Begin();
            RefreshStep();
        }

        /// <summary>Report a gameplay trigger (from real play or a demo button).</summary>
        public void Report(TutorialTrigger trigger, int amount = 1) => _flow.Report(trigger, amount);

        private void BuildDots()
        {
            if (dotsParent == null || dotPrefab == null) return;
            for (int i = 0; i < _flow.Count; i++)
            {
                var d = Instantiate(dotPrefab, dotsParent);
                d.gameObject.SetActive(true);
                _dots.Add(d);
            }
            RefreshDots();
        }

        private void RefreshStep()
        {
            var step = _flow.Current;
            if (step == null) return;
            if (stepTitle) stepTitle.text = string.IsNullOrEmpty(step.titleKey) ? step.stepId : step.titleKey;
            if (stepBody) stepBody.text = step.bodyKey;
            if (stepIndexText) stepIndexText.text = $"Step {_flow.Index + 1} / {_flow.Count}";
            RefreshDots();
        }

        private void RefreshDots()
        {
            for (int i = 0; i < _dots.Count; i++)
            {
                bool done = i < _flow.CompletedCount;
                bool current = i == _flow.Index && !_flow.IsComplete;
                _dots[i].color = done ? new Color(0.45f,0.85f,0.5f) : (current ? new Color(0.36f,0.62f,0.92f) : new Color(0.3f,0.32f,0.4f));
            }
        }

        private void ShowComplete()
        {
            if (completePanel) completePanel.SetActive(true);
            var (xp, kp) = _flow.TotalEarned();
            if (completeRewardText) completeRewardText.text = $"+{xp} XP" + (kp > 0 ? $"   +{kp} KP" : "");
            OnCompleted?.Invoke(xp, kp);
        }
    }
}

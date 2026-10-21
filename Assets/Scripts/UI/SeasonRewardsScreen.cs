// Project D  |  Codename: Zantetsuken
// SeasonRewardsScreen.cs  (Ranked UI — end-of-season reward reveal + claim)
//
// Shows on first login after a season ends: the player's peak rank, the tier
// emblem(s) they earned, win-milestone rewards, total honor, and a Claim button.
// Reveals reward rows one at a time for a bit of ceremony. Cosmetic-only.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Ranked;

namespace ProjectD.RankedUI
{
    public sealed class SeasonRewardsScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text seasonTitle;      // "Season 1 Rewards"
        [SerializeField] private Text peakLabel;        // "Peak: Adamantite C"
        [SerializeField] private Text honorLabel;       // "+4600 Honor"
        [SerializeField] private RectTransform rewardRowParent;
        [SerializeField] private RewardRow rewardRowPrefab;
        [SerializeField] private Button claimButton;
        [SerializeField] private Text claimButtonLabel;
        [SerializeField] private float rowRevealDelay = 0.18f;

        private readonly List<RewardRow> _rows = new();
        private SeasonRewardService _service;
        private PendingSeasonReward _pending;
        private SeasonRewardResult _result;

        public event System.Action OnClaimed;

        private void Awake()
        {
            if (group) group.alpha = 0f;
            if (claimButton) claimButton.onClick.AddListener(Claim);
        }

        /// <summary>Present the reward reveal for a computed result (server or local).</summary>
        public void Present(SeasonRewardService service, PendingSeasonReward pending, SeasonRewardResult result, string seasonName)
        {
            _service = service; _pending = pending; _result = result;

            if (seasonTitle) seasonTitle.text = $"{seasonName} Rewards";
            if (peakLabel) peakLabel.text = $"Peak: {result.PeakRankLabel}";
            if (honorLabel) honorLabel.text = $"+{result.TotalHonor} Honor";
            if (claimButtonLabel) claimButtonLabel.text = (pending != null && pending.claimed) ? "CLAIMED" : "CLAIM";
            if (claimButton) claimButton.interactable = !(pending != null && pending.claimed);

            BuildRows(result);
            StopAllCoroutines();
            StartCoroutine(Reveal());
        }

        private void BuildRows(SeasonRewardResult result)
        {
            foreach (var r in _rows) if (r) Destroy(r.gameObject);
            _rows.Clear();
            if (rewardRowParent == null || rewardRowPrefab == null) return;

            foreach (var reward in result.Rewards)
            {
                var row = Instantiate(rewardRowPrefab, rewardRowParent);
                row.Bind(reward);
                row.SetVisible(false);
                _rows.Add(row);
            }
        }

        private IEnumerator Reveal()
        {
            float t = 0f; while (t < 0.3f) { t += Time.deltaTime; if (group) group.alpha = Mathf.Clamp01(t / 0.3f); yield return null; }
            if (group) group.alpha = 1f;

            foreach (var row in _rows)
            {
                row.SetVisible(true);
                yield return new WaitForSeconds(rowRevealDelay);
            }
        }

        private void Claim()
        {
            if (_service == null || _pending == null) { Hide(); return; }
            if (_service.Claim(_pending))
            {
                if (claimButtonLabel) claimButtonLabel.text = "CLAIMED";
                if (claimButton) claimButton.interactable = false;
                OnClaimed?.Invoke();
            }
            StartCoroutine(HideAfter(0.6f));
        }

        private IEnumerator HideAfter(float s) { yield return new WaitForSeconds(s); Hide(); }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
        private IEnumerator FadeOut()
        {
            float t = 0f, start = group ? group.alpha : 1f;
            while (t < 0.25f) { t += Time.deltaTime; if (group) group.alpha = Mathf.Lerp(start, 0f, t / 0.25f); yield return null; }
            if (group) group.alpha = 0f;
        }
    }
}

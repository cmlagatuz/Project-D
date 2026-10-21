// Project D  |  Codename: Zantetsuken
// RankUpScreen.cs  (Ranked UI — the promotion / demotion celebration)
//
// Shown after a ranked match when the player promotes, demotes, or gets placed. It
// animates the badge from the old rank to the new one, shows the LP delta, and uses
// a stronger flourish for tier promotions (e.g. Gold -> Platinum) than a division
// bump. Subscribe to RankService.OnRankChanged and call Present(change).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Ranked;

namespace ProjectD.RankedUI
{
    public sealed class RankUpScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text headline;        // "PROMOTED!" / "TIER UP!" / "DEMOTED"
        [SerializeField] private Text fromLabel;       // "Gold A"
        [SerializeField] private Text toLabel;         // "Platinum E"
        [SerializeField] private Text lpDeltaLabel;    // "+22 LP"
        [SerializeField] private Image tierFlash;      // color flash keyed to the new tier
        [SerializeField] private RankBadge badge;      // shows the resulting rank
        [SerializeField] private Button continueButton;
        [SerializeField] private float holdSeconds = 2.5f;

        private void Awake()
        {
            if (group) group.alpha = 0f;
            if (continueButton) continueButton.onClick.AddListener(Hide);
        }

        /// <summary>Show the screen for a rank change. Ignored for pure LP/no-op results.</summary>
        public void Present(RankChange change, RankState resultingState)
        {
            bool worthShowing =
                change.Kind == RankChangeKind.Promotion || change.Kind == RankChangeKind.TierPromotion ||
                change.Kind == RankChangeKind.Demotion || change.Kind == RankChangeKind.TierDemotion ||
                change.Kind == RankChangeKind.Placed;
            if (!worthShowing) return;

            if (headline) headline.text = HeadlineFor(change.Kind);
            if (fromLabel) fromLabel.text = $"{RankFormatter.TierName(change.Before.Tier)} {change.Before.Division}";
            if (toLabel) toLabel.text = $"{RankFormatter.TierName(change.After.Tier)} {change.After.Division}";
            if (lpDeltaLabel) lpDeltaLabel.text = change.LpDelta == 0 ? "" : (change.LpDelta > 0 ? $"+{change.LpDelta} LP" : $"{change.LpDelta} LP");
            if (tierFlash) tierFlash.color = RankBadge.ToColor(RankFormatter.TierColor(change.After.Tier));
            if (badge) badge.Bind(resultingState);

            StopAllCoroutines();
            StartCoroutine(Animate(change.Kind == RankChangeKind.TierPromotion));
        }

        private static string HeadlineFor(RankChangeKind k) => k switch
        {
            RankChangeKind.TierPromotion => "TIER UP!",
            RankChangeKind.Promotion => "PROMOTED!",
            RankChangeKind.TierDemotion => "TIER DOWN",
            RankChangeKind.Demotion => "DEMOTED",
            RankChangeKind.Placed => "RANK PLACED",
            _ => ""
        };

        private IEnumerator Animate(bool bigFlourish)
        {
            // Fade in.
            float t = 0f, inDur = 0.3f;
            while (t < inDur) { t += Time.deltaTime; if (group) group.alpha = Mathf.Clamp01(t / inDur); yield return null; }
            if (group) group.alpha = 1f;

            // A small punch on the badge; larger for a tier-up.
            if (badge)
            {
                float scaleUp = bigFlourish ? 1.25f : 1.12f;
                yield return Punch(badge.transform, scaleUp, 0.35f);
            }

            yield return new WaitForSeconds(holdSeconds);

            // Auto-hide if the player doesn't tap Continue.
            if (group && group.alpha > 0f) Hide();
        }

        private static IEnumerator Punch(Transform tr, float peak, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                float s = 1f + (peak - 1f) * Mathf.Sin(p * Mathf.PI);
                tr.localScale = Vector3.one * s;
                yield return null;
            }
            tr.localScale = Vector3.one;
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            float t = 0f, dur = 0.25f, start = group ? group.alpha : 1f;
            while (t < dur) { t += Time.deltaTime; if (group) group.alpha = Mathf.Lerp(start, 0f, t / dur); yield return null; }
            if (group) group.alpha = 0f;
        }
    }
}

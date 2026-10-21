// Project D  |  Codename: Zantetsuken
// LevelUpPopup.cs  (Account UI — celebrate a level-up + KP reward)
//
// Shown after an XP grant (e.g. returning to the menu post-match) when the account
// leveled up. Displays the new level, the KP earned, and — for a multi-level jump —
// the range of levels reached. Subscribe to the store service and Present the result.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Account;
using ProjectD.Economy;

namespace ProjectD.StoreUI
{
    public sealed class LevelUpPopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text headline;      // "LEVEL UP!"
        [SerializeField] private Text levelLabel;    // "Level 24" (or "Levels 24-25")
        [SerializeField] private Text kpLabel;       // "+310 KP"
        [SerializeField] private Image levelRing;
        [SerializeField] private Button continueButton;
        [SerializeField] private float holdSeconds = 2.5f;

        private void Awake()
        {
            if (group) group.alpha = 0f;
            if (continueButton) continueButton.onClick.AddListener(Hide);
        }

        public void Present(XpGrantResult r)
        {
            if (!r.LeveledUp) return;

            if (headline) headline.text = "LEVEL UP!";
            if (levelLabel)
                levelLabel.text = r.LevelsGained == 1
                    ? $"Level {r.EndLevel}"
                    : $"Levels {r.StartLevel + 1}-{r.EndLevel}";
            if (kpLabel) kpLabel.text = r.KpGained > 0 ? $"+{r.KpGained} {EconomyTerms.KpDisplayName}" : "";

            StopAllCoroutines();
            StartCoroutine(Show());
        }

        private IEnumerator Show()
        {
            float t = 0f;
            while (t < 0.3f) { t += Time.deltaTime; if (group) group.alpha = Mathf.Clamp01(t / 0.3f); yield return null; }
            if (group) group.alpha = 1f;
            if (levelRing) yield return Punch(levelRing.transform, 1.2f, 0.35f);
            yield return new WaitForSeconds(holdSeconds);
            if (group && group.alpha > 0f) Hide();
        }

        private static IEnumerator Punch(Transform tr, float peak, float dur)
        {
            float t = 0f;
            while (t < dur) { t += Time.deltaTime; float s = 1f + (peak - 1f) * Mathf.Sin(t / dur * Mathf.PI); tr.localScale = Vector3.one * s; yield return null; }
            tr.localScale = Vector3.one;
        }

        public void Hide() { StopAllCoroutines(); StartCoroutine(Fade()); }
        private IEnumerator Fade()
        {
            float t = 0f, start = group ? group.alpha : 1f;
            while (t < 0.25f) { t += Time.deltaTime; if (group) group.alpha = Mathf.Lerp(start, 0f, t / 0.25f); yield return null; }
            if (group) group.alpha = 0f;
        }
    }
}

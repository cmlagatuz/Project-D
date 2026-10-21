// Project D  |  Codename: Zantetsuken
// ManifestationHudView.cs  (World Rift UI — the in-match Manifestation HUD)
//
// The in-match overlay for the contestable world boss:
//   - Spawn timer   : counts down to the next Manifestation spawn.
//   - Boss HP bar    : the Manifestation's health while alive.
//   - Contest bar    : blue-vs-red damage stake, showing who's leading the fight.
//   - Claim banner   : "BLUE CLAIMS <Manifestation>!" flash on the finishing blow.
//   - Summon Strike  : a telegraph ring + screen flash when the victor's AoE nuke fires.
//   - Buff indicator : the active Manifestation buff + its remaining time.
// Binds a ManifestationBoss; subscribes to its OnSpawned/OnClaimed events and reads its
// live state each Tick.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.WorldBoss;

namespace ProjectD.WorldRiftUI
{
    public sealed class ManifestationHudView : MonoBehaviour
    {
        [Header("Spawn / HP")]
        [SerializeField] private Text spawnTimerText, bossNameText;
        [SerializeField] private GameObject bossPanel;
        [SerializeField] private Image bossHpFill;

        [Header("Contest bar")]
        [SerializeField] private Image contestBlueFill, contestRedFill;   // opposing fills that meet in the middle
        [SerializeField] private Text contestLeaderText;

        [Header("Claim banner")]
        [SerializeField] private GameObject claimBanner;
        [SerializeField] private CanvasGroup claimGroup;
        [SerializeField] private Text claimText;

        [Header("Summon strike")]
        [SerializeField] private RectTransform strikeTelegraph;   // a ring at the pit
        [SerializeField] private Image screenFlash;               // full-screen flash

        [Header("Buff indicator")]
        [SerializeField] private GameObject buffPanel;
        [SerializeField] private Text buffText;

        [SerializeField] private string manifestationDisplayName = "Manifestation";
        private ManifestationBoss _boss;
        private float _clock;

        public void Bind(ManifestationBoss boss, string displayName)
        {
            _boss = boss; manifestationDisplayName = displayName;
            _boss.OnSpawned += HandleSpawned;
            _boss.OnClaimed += HandleClaimed;
            if (bossNameText) bossNameText.text = displayName;
            if (claimBanner) claimBanner.SetActive(false);
            if (screenFlash) { var c = screenFlash.color; c.a = 0; screenFlash.color = c; }
            if (strikeTelegraph) strikeTelegraph.gameObject.SetActive(false);
            Refresh();
        }

        /// <summary>Drive from the match loop.</summary>
        public void SetClock(float matchSeconds) { _clock = matchSeconds; Refresh(); }

        private void Refresh()
        {
            if (_boss == null) return;
            bool alive = _boss.State == BossState.Alive;

            if (bossPanel) bossPanel.SetActive(alive);
            if (alive)
            {
                if (bossHpFill) bossHpFill.fillAmount = _boss.HpPercent;
                float blue = _boss.DamageBy(BossTeam.Blue), red = _boss.DamageBy(BossTeam.Red), total = Mathf.Max(1f, blue + red);
                if (contestBlueFill) contestBlueFill.fillAmount = blue / total;
                if (contestRedFill) contestRedFill.fillAmount = red / total;
                if (contestLeaderText)
                {
                    var lead = _boss.Leading();
                    contestLeaderText.text = lead == BossTeam.None ? "CONTESTED" : $"{lead} leading";
                    contestLeaderText.color = lead == BossTeam.Blue ? new Color(0.4f,0.65f,1f) : lead == BossTeam.Red ? new Color(0.9f,0.45f,0.45f) : new Color(0.85f,0.8f,0.4f);
                }
            }

            if (spawnTimerText)
            {
                if (alive) spawnTimerText.text = $"{manifestationDisplayName} — ALIVE";
                else { float s = _boss.SecondsUntilSpawn(_clock); spawnTimerText.text = $"{manifestationDisplayName} in {Mathf.CeilToInt(s)}s"; }
            }

            // Buff indicator.
            bool buffed = _boss.CurrentBuff.Active(_clock);
            if (buffPanel) buffPanel.SetActive(buffed);
            if (buffed && buffText)
            {
                float rem = _boss.CurrentBuff.expiresAt - _clock;
                buffText.text = $"{_boss.CurrentBuff.team} Empowered  ({Mathf.CeilToInt(rem)}s)";
                buffText.color = _boss.CurrentBuff.team == BossTeam.Blue ? new Color(0.4f,0.65f,1f) : new Color(0.9f,0.45f,0.45f);
            }
        }

        private void HandleSpawned()
        {
            if (bossPanel) bossPanel.SetActive(true);
        }

        private void HandleClaimed(BossTeam team, SummonStrike strike)
        {
            StartCoroutine(ClaimSequence(team, strike));
        }

        private IEnumerator ClaimSequence(BossTeam team, SummonStrike strike)
        {
            // 1) Claim banner flash.
            if (claimBanner) claimBanner.SetActive(true);
            if (claimText) { claimText.text = $"{team} CLAIMS {manifestationDisplayName.ToUpperInvariant()}!"; claimText.color = team == BossTeam.Blue ? new Color(0.5f,0.75f,1f) : new Color(1f,0.55f,0.55f); }
            if (claimGroup) yield return Fade(claimGroup, 0f, 1f, 0.25f);

            // 2) Summon strike telegraph (ring expands at the pit).
            if (strikeTelegraph)
            {
                strikeTelegraph.gameObject.SetActive(true);
                var img = strikeTelegraph.GetComponent<Image>();
                float t = 0f, dur = 0.5f;
                while (t < dur)
                {
                    t += Time.deltaTime; float k = t / dur;
                    strikeTelegraph.sizeDelta = Vector2.one * Mathf.Lerp(20f, strike.radius * 24f, k);
                    if (img) { var c = img.color; c.a = 0.7f * (1f - k); img.color = c; }
                    yield return null;
                }
                strikeTelegraph.gameObject.SetActive(false);
            }

            // 3) Screen flash on detonation.
            if (screenFlash)
            {
                var col = team == BossTeam.Blue ? new Color(0.5f,0.75f,1f) : new Color(1f,0.55f,0.45f);
                float t = 0f, dur = 0.35f;
                while (t < dur) { t += Time.deltaTime; float k = t / dur; col.a = 0.6f * (1f - k); screenFlash.color = col; yield return null; }
                col.a = 0f; screenFlash.color = col;
            }

            yield return new WaitForSeconds(1.4f);
            if (claimGroup) yield return Fade(claimGroup, 1f, 0f, 0.3f);
            if (claimBanner) claimBanner.SetActive(false);
        }

        private static IEnumerator Fade(CanvasGroup g, float from, float to, float dur)
        {
            float t = 0f; while (t < dur) { t += Time.deltaTime; g.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
            g.alpha = to;
        }
    }
}

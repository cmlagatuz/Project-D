// Project D  |  Codename: Zantetsuken
// AnnouncerBanner.cs  (Announcer UI — in-match callout banner)
//
// Drains the AnnouncementQueue and shows one banner at a time (biggest first): a
// styled headline ("DOUBLE KILL", "FIRST TURRET", "CORE EXPOSED") with a subject line,
// a color + scale keyed to the announcement kind, and slide-in/hold/slide-out timing.
// Fires OnVoCue / OnSfxCue so the audio layer (effects pack) plays the stinger + VO.
//
// The queue itself is server-driven (the server raises announcements from authoritative
// combat/objective events); this component is the local presentation.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Announcer;

namespace ProjectD.AnnouncerUI
{
    public sealed class AnnouncerBanner : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image accentBar;
        [SerializeField] private Text headlineText;    // "DOUBLE KILL"
        [SerializeField] private Text subjectText;     // "You" / "Blue Team"
        [SerializeField] private float holdSeconds = 1.8f;
        [SerializeField] private float slideSeconds = 0.25f;

        // Resolves a subject id -> display name (bind from the match roster).
        public System.Func<string, string> ResolveSubjectName;
        // Resolves a banner text key -> localized headline (bind from localization).
        public System.Func<string, string> ResolveText;

        public event System.Action<string> OnVoCue;    // vo key
        public event System.Action<string> OnSfxCue;   // sfx key

        private readonly AnnouncementQueue _queue = new();
        private bool _showing;

        private void Awake()
        {
            if (group) group.alpha = 0f;
            _queue.OnAnnouncement += a => { }; // queue used via Present flow below
        }

        /// <summary>Raise an announcement (server pushes these). Returns false if deduped.</summary>
        public bool Raise(in Announcement a) => _queue.Raise(a);

        private void Update()
        {
            if (_showing) return;
            if (_queue.Emit(out var next)) StartCoroutine(Show(next));
        }

        private IEnumerator Show(Announcement a)
        {
            _showing = true;

            // Fire audio cues.
            if (!string.IsNullOrEmpty(a.SfxKey)) OnSfxCue?.Invoke(a.SfxKey);
            if (!string.IsNullOrEmpty(a.VoKey)) OnVoCue?.Invoke(a.VoKey);

            // Style by kind.
            var style = StyleFor(a.Kind);
            if (headlineText) { headlineText.text = ResolveHeadline(a); headlineText.color = style.textColor; headlineText.fontSize = style.fontSize; }
            if (accentBar) accentBar.color = style.accent;
            if (subjectText) subjectText.text = ResolveSubjectName != null ? (ResolveSubjectName(a.SubjectId) ?? a.SubjectId) : a.SubjectId;

            // Slide in from the top + fade.
            if (panel) panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, StartY(style));
            float t = 0f;
            while (t < slideSeconds)
            {
                t += Time.deltaTime; float k = t / slideSeconds;
                if (group) group.alpha = k;
                if (panel) panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, Mathf.Lerp(StartY(style), TargetY(style), EaseOut(k)));
                yield return null;
            }
            if (group) group.alpha = 1f;

            // Punch scale for high-priority callouts.
            if (panel && style.punch) yield return Punch(panel, 1.12f, 0.2f);

            yield return new WaitForSeconds(holdSeconds);

            // Fade + slide out.
            t = 0f;
            while (t < slideSeconds)
            {
                t += Time.deltaTime; float k = t / slideSeconds;
                if (group) group.alpha = 1f - k;
                yield return null;
            }
            if (group) group.alpha = 0f;
            _showing = false;
        }

        private string ResolveHeadline(Announcement a)
        {
            if (ResolveText != null) { var r = ResolveText(a.BannerTextKey); if (!string.IsNullOrEmpty(r)) return r; }
            return DefaultHeadline(a.Kind);   // fallback English if no localization bound
        }

        private struct Style { public Color accent, textColor; public int fontSize; public bool punch; }

        private static Style StyleFor(AnnouncementKind k)
        {
            switch (k)
            {
                case AnnouncementKind.PentaKill:
                case AnnouncementKind.CoreExposed:
                case AnnouncementKind.AceEnemyTeam:
                    return new Style { accent = new Color(0.95f,0.35f,0.55f), textColor = new Color(1f,0.9f,0.95f), fontSize = 56, punch = true };
                case AnnouncementKind.QuadraKill:
                case AnnouncementKind.OdinSlain:
                    return new Style { accent = new Color(0.95f,0.72f,0.30f), textColor = new Color(1f,0.95f,0.8f), fontSize = 50, punch = true };
                case AnnouncementKind.TripleKill:
                case AnnouncementKind.DivineSlain:
                case AnnouncementKind.SummonSlain:
                    return new Style { accent = new Color(0.65f,0.40f,0.90f), textColor = Color.white, fontSize = 46, punch = true };
                case AnnouncementKind.DoubleKill:
                case AnnouncementKind.FirstBlood:
                case AnnouncementKind.FirstTurret:
                    return new Style { accent = new Color(0.35f,0.62f,0.92f), textColor = Color.white, fontSize = 42, punch = false };
                default:
                    return new Style { accent = new Color(0.55f,0.60f,0.66f), textColor = Color.white, fontSize = 38, punch = false };
            }
        }

        private static string DefaultHeadline(AnnouncementKind k) => k switch
        {
            AnnouncementKind.FirstBlood => "FIRST BLOOD",
            AnnouncementKind.DoubleKill => "DOUBLE KILL",
            AnnouncementKind.TripleKill => "TRIPLE KILL",
            AnnouncementKind.QuadraKill => "QUADRA KILL",
            AnnouncementKind.PentaKill => "PENTA KILL",
            AnnouncementKind.KillingSpree => "KILLING SPREE",
            AnnouncementKind.Rampage => "RAMPAGE",
            AnnouncementKind.Unstoppable => "UNSTOPPABLE",
            AnnouncementKind.Dominating => "DOMINATING",
            AnnouncementKind.Legendary => "LEGENDARY",
            AnnouncementKind.FirstTurret => "FIRST TURRET",
            AnnouncementKind.TurretDestroyed => "TURRET DESTROYED",
            AnnouncementKind.InhibitorDestroyed => "LANE SEAL BROKEN",
            AnnouncementKind.SummonSlain => "SUMMON SLAIN",
            AnnouncementKind.OdinSlain => "ODIN SLAIN",
            AnnouncementKind.DivineSlain => "DIVINE SLAIN",
            AnnouncementKind.ObjectiveStolen => "OBJECTIVE STOLEN!",
            AnnouncementKind.AceEnemyTeam => "ACE!",
            AnnouncementKind.CoreExposed => "CORE EXPOSED",
            AnnouncementKind.VictoryImminent => "VICTORY IMMINENT",
            _ => k.ToString().ToUpperInvariant()
        };

        private float TargetY(Style s) => 0f;
        private float StartY(Style s) => 60f;
        private static float EaseOut(float x) => 1f - (1f - x) * (1f - x);

        private static IEnumerator Punch(Transform tr, float peak, float dur)
        {
            float t = 0f;
            while (t < dur) { t += Time.deltaTime; float k = t / dur; float sc = 1f + (peak - 1f) * Mathf.Sin(k * Mathf.PI); tr.localScale = Vector3.one * sc; yield return null; }
            tr.localScale = Vector3.one;
        }
    }
}

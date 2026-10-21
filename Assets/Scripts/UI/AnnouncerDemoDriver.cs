// Project D  |  Codename: Zantetsuken
// AnnouncerDemoDriver.cs  (Announcer UI — sandbox)
//
// Fires a scripted sequence of announcements (First Blood -> Double/Triple Kill ->
// Summon/Odin Slain -> Ace -> Core Exposed) so you can watch the banner queue, styling,
// and slide/punch animations. Buttons let you trigger specific callouts. In a real
// build the SERVER raises these from authoritative combat/objective events.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.Announcer;

namespace ProjectD.AnnouncerUI
{
    public sealed class AnnouncerDemoDriver : MonoBehaviour
    {
        [SerializeField] private AnnouncerBanner banner;
        [SerializeField] private Button doubleKillButton, aceButton, odinButton, coreButton, sequenceButton;

        private float _clock;

        private void Start()
        {
            if (banner == null) return;
            banner.ResolveSubjectName = id => id switch { "me" => "You", "blue" => "Blue Team", "red" => "Red Team", _ => id };
            banner.OnVoCue += k => Debug.Log($"[Project D] VO cue: {k}");
            banner.OnSfxCue += k => Debug.Log($"[Project D] SFX cue: {k}");

            if (doubleKillButton) doubleKillButton.onClick.AddListener(() => banner.Raise(Announcements.MultiKill(2, "me", _clock)));
            if (aceButton) aceButton.onClick.AddListener(() => banner.Raise(Announcements.Ace("blue", _clock)));
            if (odinButton) odinButton.onClick.AddListener(() => banner.Raise(Announcements.OdinSlain("blue", _clock)));
            if (coreButton) coreButton.onClick.AddListener(() => banner.Raise(Announcements.CoreExposed("red", _clock)));
            if (sequenceButton) sequenceButton.onClick.AddListener(() => StartCoroutine(Sequence()));

            StartCoroutine(Sequence());
        }

        private void Update() => _clock += Time.deltaTime;

        private IEnumerator Sequence()
        {
            banner.Raise(Announcements.FirstBlood("me", _clock)); yield return new WaitForSeconds(2.4f);
            banner.Raise(Announcements.MultiKill(2, "me", _clock)); yield return new WaitForSeconds(2.4f);
            banner.Raise(Announcements.MultiKill(3, "me", _clock)); yield return new WaitForSeconds(2.4f);
            banner.Raise(Announcements.SummonSlain("blue", _clock)); yield return new WaitForSeconds(2.4f);
            banner.Raise(Announcements.OdinSlain("blue", _clock)); yield return new WaitForSeconds(2.4f);
            banner.Raise(Announcements.Ace("blue", _clock)); yield return new WaitForSeconds(2.4f);
            banner.Raise(Announcements.CoreExposed("red", _clock));
        }
    }
}

// Project D  |  Codename: Zantetsuken
// KpStoreDemoRelay.cs  (Store UI — demo helper)
//
// Seeds a starting KP balance and wires the demo "GRANT XP" button to the store
// screen's GrantXp() so you can simulate post-match progression: level up, earn KP,
// then buy. Not part of the shipping flow — the real XP grant comes from the results
// screen after a match.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Account;

namespace ProjectD.StoreUI
{
    public sealed class KpStoreDemoRelay : MonoBehaviour
    {
        [SerializeField] private KpStoreScreen screen;
        [SerializeField] private Button grantXpButton;
        [SerializeField] private int xpPerClick = 800;
        [SerializeField] private int startingKp = 8000;

        private bool _seeded;

        private void Start()
        {
            if (grantXpButton) grantXpButton.onClick.AddListener(() => { if (screen) screen.GrantXp(xpPerClick); });
        }

        private void Update()
        {
            // Seed a starting KP balance once the service exists (after Start).
            if (_seeded || screen == null || screen.Service == null || screen.Service.Account == null) return;
            screen.Service.Account.kp = startingKp;
            _seeded = true;
        }
    }
}

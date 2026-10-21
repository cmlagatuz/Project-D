// Project D  |  Codename: Zantetsuken
// ModeSelectDemoDriver.cs  (World Rift UI — logs the queue action)

using UnityEngine;
using ProjectD.Modes;
using ProjectD.WorldRiftUI;

namespace ProjectD.WorldRiftUI
{
    public sealed class ModeSelectDemoDriver : MonoBehaviour
    {
        [SerializeField] private ModeSelectView view;
        private void Start()
        {
            if (view == null) return;
            view.OnQueue += (mode, mapId) =>
            {
                bool lp = GameModeRegistry.CanAwardRankedLp(mode, mapId);
                Debug.Log($"[Project D] Queue: {mode} on map '{mapId}' | Ranked LP: {lp}");
            };
        }
    }
}

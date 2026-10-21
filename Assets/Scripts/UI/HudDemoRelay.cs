// Project D  |  Codename: Zantetsuken
// HudDemoRelay.cs  (HUD UI — demo button wiring)
//
// Wires the HUD demo buttons to the recall bar + ability level-up widget so the scene
// runs without extra scripting. Not part of the shipping HUD.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.HUDUI;

namespace ProjectD.P2UI
{
    public sealed class HudDemoRelay : MonoBehaviour
    {
        [SerializeField] private RecallBarWidget recall;
        [SerializeField] private AbilityLevelUpBar levelUp;
        [SerializeField] private Button beginRecallButton;
        [SerializeField] private Button interruptButton;
        [SerializeField] private Button champLevelUpButton;

        private void Start()
        {
            if (beginRecallButton && recall) beginRecallButton.onClick.AddListener(() => { recall.SetDuration(5f); recall.Begin(); });
            if (interruptButton && recall) interruptButton.onClick.AddListener(recall.Interrupt);
            if (champLevelUpButton && levelUp) champLevelUpButton.onClick.AddListener(levelUp.ChampionLevelUp);
        }
    }
}

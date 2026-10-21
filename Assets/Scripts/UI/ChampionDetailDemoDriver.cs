// Project D  |  Codename: Zantetsuken
// ChampionDetailDemoDriver.cs  (Champion detail UI — sandbox)
//
// Feeds a sample champion (Kain) with stat bars, abilities, skins, and a KP unlock
// price so you can view the detail page and try the buy flow. In a real build the KP
// Store service completes the purchase and pushes the new balance/ownership back.

using UnityEngine;
using ProjectD.ChampionDetail;

namespace ProjectD.ChampionDetailUI
{
    public sealed class ChampionDetailDemoDriver : MonoBehaviour
    {
        [SerializeField] private ChampionDetailView view;
        [SerializeField] private int accountLevel = 20;
        [SerializeField] private long kpBalance = 6000;

        private void Start()
        {
            if (view == null) return;

            var input = new ChampionPreviewInput
            {
                ChampionId = "kain",
                DisplayName = "Kain Highwind",
                Title = "The Dragoon of Baron",
                PrimaryRole = "Assassin", SecondaryRole = "Fighter",
                Difficulty = 3,
                BaseAttackDamage = 64, BaseMagicPower = 0, BaseHealth = 640, BaseArmor = 34, BaseMagicResist = 32,
                HealShieldPower = 4,
                UnlockKpPrice = 4500, RequiredAccountLevel = 0,
                AbilityNames = new[] { "Dragoon's Poise", "Lancet Thrust", "Vault", "Dragon Pin", "High Jump" },
                Skins = new[]
                {
                    new SkinInfo{ skinId="kain_default", displayName="Kain Highwind", rarity="Common", owned=true, isDefault=true, kpPrice=0 },
                    new SkinInfo{ skinId="kain_ceodore", displayName="Sky Pirate Kain", rarity="Epic", owned=false, kpPrice=1350 },
                    new SkinInfo{ skinId="kain_crystal", displayName="Crystal Dragoon Kain", rarity="Legendary", owned=false, kpPrice=2700 },
                }
            };

            view.OnBuy += (champId, price) => Debug.Log($"[Project D] Buy {champId} for {price} KP -> KpStoreService.Buy(...)");
            view.Present(input, owned: false, accountLevel: accountLevel, kpBalance: kpBalance);
        }
    }
}

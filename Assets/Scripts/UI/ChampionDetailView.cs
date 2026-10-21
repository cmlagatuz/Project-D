// Project D  |  Codename: Zantetsuken
// ChampionDetailView.cs  (Champion detail UI — the preview/unlock page)
//
// Renders the batch-4 champion detail page: name + title, role tags, four stat bars
// (Damage / Toughness / Utility / Difficulty), the ability icon row, a skins carousel
// (prev/next through owned + locked skins), and a KP unlock/buy panel that reflects
// Owned / Purchasable / LevelLocked / NotEnoughKp. Binds a ChampionPreviewInput +
// account context; raises OnBuy so the KP Store service completes the purchase.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectD.ChampionDetail;

namespace ProjectD.ChampionDetailUI
{
    public sealed class ChampionDetailView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text roleLabel;          // "Vanguard / Support"
        [SerializeField] private Image splash;            // champion art (role-tinted fallback)

        [Header("Stat bars")]
        [SerializeField] private Image damageFill;
        [SerializeField] private Image toughnessFill;
        [SerializeField] private Image utilityFill;
        [SerializeField] private Image difficultyFill;

        [Header("Ability row")]
        [SerializeField] private Image[] abilityIcons = new Image[5];   // P, Q, W, E, R
        [SerializeField] private Text[] abilityLabels = new Text[5];

        [Header("Skins carousel")]
        [SerializeField] private Text skinNameLabel;
        [SerializeField] private Text skinRarityLabel;
        [SerializeField] private Image skinPreview;
        [SerializeField] private GameObject skinLockBadge;
        [SerializeField] private Button skinPrevButton;
        [SerializeField] private Button skinNextButton;
        [SerializeField] private Text skinIndexLabel;     // "2 / 5"

        [Header("Unlock / buy panel")]
        [SerializeField] private Text priceLabel;         // "3,000 KP"
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyButtonLabel;
        [SerializeField] private Text feedbackLabel;

        private readonly ChampionDetailModel _model = new();
        private ChampionPreviewInput _input;
        private int _skinIndex;
        private bool _owned;
        private int _accountLevel;
        private long _kpBalance;

        /// <summary>Fired when the player confirms a KP purchase (championId, price).</summary>
        public event System.Action<string, int> OnBuy;

        public void Present(ChampionPreviewInput input, bool owned, int accountLevel, long kpBalance)
        {
            _input = input; _owned = owned; _accountLevel = accountLevel; _kpBalance = kpBalance; _skinIndex = 0;
            _model.Load(input);

            if (nameLabel) nameLabel.text = input.DisplayName;
            if (titleLabel) titleLabel.text = input.Title;
            if (roleLabel) roleLabel.text = string.IsNullOrEmpty(input.SecondaryRole) || input.SecondaryRole == input.PrimaryRole
                ? input.PrimaryRole : $"{input.PrimaryRole} / {input.SecondaryRole}";
            if (splash && splash.sprite == null) splash.color = RoleTint(input.PrimaryRole);

            // Stat bars (0..10 -> 0..1 fill).
            SetBar(damageFill, StatAxis.Damage);
            SetBar(toughnessFill, StatAxis.Toughness);
            SetBar(utilityFill, StatAxis.Utility);
            SetBar(difficultyFill, StatAxis.Difficulty);

            // Ability row.
            for (int i = 0; i < abilityIcons.Length; i++)
            {
                string label = input.AbilityNames != null && i < input.AbilityNames.Length ? input.AbilityNames[i] : "";
                if (abilityLabels != null && i < abilityLabels.Length && abilityLabels[i]) abilityLabels[i].text = label;
                if (abilityIcons[i] && abilityIcons[i].sprite == null) abilityIcons[i].color = new Color(0.28f, 0.34f, 0.5f);
            }

            // Skins carousel.
            if (skinPrevButton) { skinPrevButton.onClick.RemoveAllListeners(); skinPrevButton.onClick.AddListener(() => StepSkin(-1)); }
            if (skinNextButton) { skinNextButton.onClick.RemoveAllListeners(); skinNextButton.onClick.AddListener(() => StepSkin(+1)); }
            RefreshSkin();

            // Buy panel.
            if (buyButton) { buyButton.onClick.RemoveAllListeners(); buyButton.onClick.AddListener(BuyChampion); }
            RefreshBuy();
        }

        private void SetBar(Image fill, StatAxis axis)
        {
            if (!fill) return;
            int v = 1;
            foreach (var b in _model.StatBars) if (b.Axis == axis) v = b.Value;
            fill.fillAmount = Mathf.Clamp01(v / 10f);
        }

        // ---- skins ----
        private int SkinCount => _input.Skins != null ? _input.Skins.Length : 0;

        private void StepSkin(int dir)
        {
            if (SkinCount == 0) return;
            _skinIndex = (_skinIndex + dir + SkinCount) % SkinCount;
            RefreshSkin();
        }

        private void RefreshSkin()
        {
            if (SkinCount == 0)
            {
                if (skinNameLabel) skinNameLabel.text = "Default";
                if (skinIndexLabel) skinIndexLabel.text = "";
                if (skinLockBadge) skinLockBadge.SetActive(false);
                return;
            }
            var skin = _input.Skins[_skinIndex];
            if (skinNameLabel) skinNameLabel.text = skin.displayName;
            if (skinRarityLabel) skinRarityLabel.text = skin.rarity;
            if (skinIndexLabel) skinIndexLabel.text = $"{_skinIndex + 1} / {SkinCount}";
            if (skinLockBadge) skinLockBadge.SetActive(!skin.owned && !skin.isDefault);
            if (skinPreview && skinPreview.sprite == null) skinPreview.color = RarityTint(skin.rarity);
        }

        // ---- buy ----
        private void RefreshBuy()
        {
            var state = _model.ResolveUnlock(_owned, _accountLevel, _kpBalance);
            if (priceLabel) priceLabel.text = _owned ? "" : $"{_input.UnlockKpPrice:N0} KP";
            if (buyButton) buyButton.interactable = state == UnlockState.Purchasable;
            if (buyButtonLabel)
                buyButtonLabel.text = state switch
                {
                    UnlockState.Owned => "OWNED",
                    UnlockState.LevelLocked => $"UNLOCK AT Lv {_input.RequiredAccountLevel}",
                    UnlockState.NotEnoughKp => $"NEED {_input.UnlockKpPrice - _kpBalance:N0} KP",
                    _ => $"UNLOCK  {_input.UnlockKpPrice:N0} KP"
                };
        }

        private void BuyChampion()
        {
            var state = _model.ResolveUnlock(_owned, _accountLevel, _kpBalance);
            if (state != UnlockState.Purchasable) return;
            OnBuy?.Invoke(_input.ChampionId, _input.UnlockKpPrice);
            _owned = true; _kpBalance -= _input.UnlockKpPrice;
            RefreshBuy();
            Feedback($"Unlocked {_input.DisplayName}!", true);
        }

        /// <summary>Call after the server confirms/denies the purchase to sync UI.</summary>
        public void ApplyPurchaseResult(bool success, long newKpBalance)
        {
            _kpBalance = newKpBalance;
            if (success) _owned = true;
            RefreshBuy();
            Feedback(success ? "Purchase confirmed." : "Purchase failed.", success);
        }

        private void Feedback(string msg, bool ok)
        {
            if (!feedbackLabel) return;
            feedbackLabel.text = msg;
            feedbackLabel.color = ok ? new Color(0.55f, 0.85f, 0.6f) : new Color(0.95f, 0.5f, 0.45f);
        }

        private static Color RoleTint(string role) => role switch
        {
            "Vanguard" => new Color(0.30f, 0.45f, 0.80f), "Fighter" => new Color(0.80f, 0.45f, 0.28f),
            "Assassin" => new Color(0.75f, 0.28f, 0.38f), "Marksman" => new Color(0.80f, 0.68f, 0.28f),
            "Mage" => new Color(0.55f, 0.32f, 0.80f), "Support" => new Color(0.30f, 0.70f, 0.48f),
            _ => new Color(0.4f, 0.4f, 0.5f)
        };

        private static Color RarityTint(string rarity) => rarity switch
        {
            "Rare" => new Color(0.35f, 0.62f, 0.92f), "Epic" => new Color(0.65f, 0.40f, 0.90f),
            "Legendary" => new Color(0.95f, 0.72f, 0.30f), "Ultimate" => new Color(0.95f, 0.35f, 0.55f),
            _ => new Color(0.55f, 0.60f, 0.66f)
        };
    }
}

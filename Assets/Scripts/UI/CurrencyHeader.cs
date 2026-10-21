// Project D  |  Codename: Zantetsuken
// CurrencyHeader.cs  (Store UI — the KP balance + account level header)
//
// Shows the player's account level, an XP progress bar toward the next level, and
// the spendable KP balance. Subscribes to the store service so it updates live after
// a purchase or an XP grant.

using UnityEngine;
using UnityEngine.UI;
using ProjectD.Account;
using ProjectD.Economy;

namespace ProjectD.StoreUI
{
    public sealed class CurrencyHeader : MonoBehaviour
    {
        [SerializeField] private Text levelText;     // "Lv 24"
        [SerializeField] private Image xpFill;       // progress toward next level
        [SerializeField] private Text xpText;        // "1,240 / 2,040 XP"
        [SerializeField] private Text kpText;        // "12,500 KP"

        public void Bind(AccountProgression account)
        {
            if (account == null) return;
            if (levelText) levelText.text = $"Lv {account.level}";
            if (xpFill) xpFill.fillAmount = account.LevelProgress;
            if (xpText) xpText.text = $"{account.xpIntoLevel:N0} / {account.XpToNext:N0} XP";
            if (kpText) kpText.text = EconomyTerms.Format(CurrencyType.KP, account.kp);
        }
    }
}

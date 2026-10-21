// Project D  |  Codename: Zantetsuken
// KpStoreService.cs  (Store — account-facing purchase hub)
//
// Ties AccountProgression (KP balance/level) + KpStoreModel (catalog/ownership) into
// one buy flow, and syncs to the account backend. For the online meta, the SERVER is
// the source of truth: the client sends a buy intent, the server runs the same
// TryPurchase against the authoritative KP/level/ownership and pushes the result back.
// This service exposes that flow with pull/push hooks so it also works offline (demo).

using System;
using ProjectD.Account;
using ProjectD.Economy;

namespace ProjectD.Store
{
    public sealed class KpStoreService
    {
        public AccountProgression Account { get; private set; } = new AccountProgression();
        public ProfileInventory Inventory { get; private set; } = new ProfileInventory();
        public KpStoreModel Store { get; } = new KpStoreModel();

        // Account backend hooks (online).
        public Func<AccountProgression> PullAccount;
        public Action<AccountProgression> PushAccount;
        public Func<ProfileInventory> PullInventory;
        public Action<ProfileInventory> PushInventory;

        /// <summary>Fired after a successful purchase (item id) so UI refreshes.</summary>
        public event Action<string> OnPurchased;
        /// <summary>Fired after any KP/level change (e.g. post-match XP grant).</summary>
        public event Action<AccountProgression> OnAccountChanged;

        public void Initialize()
        {
            var acc = PullAccount?.Invoke();
            if (acc != null) Account = acc;
            var inv = PullInventory?.Invoke();
            if (inv != null) Inventory = inv;
        }

        /// <summary>Apply post-match / event XP; grants KP on level-ups. Returns the result.</summary>
        public XpGrantResult AddXp(int xp)
        {
            var r = AccountProgressionService.AddXp(Account, xp);
            PushAccount?.Invoke(Account);
            OnAccountChanged?.Invoke(Account);
            return r;
        }

        /// <summary>Buy a Store item with KP. Server-authoritative; here it's the source of truth.</summary>
        public PurchaseResult Buy(string itemId)
        {
            var result = Store.TryPurchase(itemId, Account.kp, Account.level, Inventory, CurrencyType.KP, out int cost);
            if (result != PurchaseResult.Success) return result;

            if (!AccountProgressionService.SpendKp(Account, cost)) return PurchaseResult.NotEnoughKp;
            Store.GrantOwnership(itemId, Inventory);

            PushAccount?.Invoke(Account);
            PushInventory?.Invoke(Inventory);
            OnPurchased?.Invoke(itemId);
            OnAccountChanged?.Invoke(Account);
            return PurchaseResult.Success;
        }

        // ---- profile equip (showcase to friends) ----
        public bool EquipBorder(string itemId)
        {
            if (!Inventory.Owns(itemId)) return false;
            Inventory.EquippedBorderId = itemId;
            PushInventory?.Invoke(Inventory);
            return true;
        }

        public bool EquipIcon(string itemId)
        {
            if (!Inventory.Owns(itemId)) return false;
            Inventory.EquippedIconId = itemId;
            PushInventory?.Invoke(Inventory);
            return true;
        }

        public string KpDisplay() => EconomyTerms.Format(CurrencyType.KP, Account.kp);
    }
}

// Project D | Codename: Zantetsuken — ItemEnums.cs (Store shared enums)
namespace ProjectD.Store
{
    public enum ItemCategory { Starter, Consumable, Trinket, Boots, Basic, Epic, Legendary, ChampionExclusive }
    public enum StoreRole { Vanguard, Fighter, Assassin, Marksman, Mage, Support, Universal }
    public enum StatType {
        AttackDamage, AbilityPower, AttackSpeedPct, CritChancePct,
        Health, Armor, MagicResist, AbilityHaste, MoveSpeedFlat, MoveSpeedPct,
        Lethality, MagicPenFlat, MagicPenPct, LifestealPct, OmnivampPct, HealShieldPowerPct,
        Mana, ManaRegen, HealthRegen, TenacityPct, GoldPer10 }
    public enum SlotRestriction { GeneralOnly, BootsOnly }
}

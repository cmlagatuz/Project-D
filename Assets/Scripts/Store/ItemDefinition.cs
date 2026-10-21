// Project D | Codename: Zantetsuken — ItemDefinition.cs (Store item ScriptableObject)
using System; using UnityEngine;
namespace ProjectD.Store
{
    [Serializable] public struct StatMod { public StatType stat; public float value; }

    [CreateAssetMenu(fileName="Item_", menuName="Project D/Store Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")] public string itemId; public string displayName; [TextArea(1,3)] public string flavor; public Sprite icon;
        [Header("Classification")] public ItemCategory category=ItemCategory.Basic;
        public StoreRole primaryRole=StoreRole.Universal; public StoreRole secondaryRole=StoreRole.Universal;
        public SlotRestriction slotRestriction=SlotRestriction.GeneralOnly;
        [Header("Cost")] public int totalCost=500; public int combineCost=0; public float sellRefund=0.6f;
        [Header("Build path")] public string[] componentIds=new string[0]; public string[] buildsIntoIds=new string[0];
        [Header("Stats")] public StatMod[] stats=new StatMod[0];
        [Header("Passive / Active")] public string passiveName; [TextArea(1,3)] public string passiveDescription;
        public string activeName; [TextArea(1,3)] public string activeDescription; public float activeCooldown;
        [Header("Consumable")] public bool isConsumable; public int maxStack=1;
        [Header("Champion Exclusive")] public string exclusiveToChampionId="";
        public bool IsBoots => category==ItemCategory.Boots;
        public bool IsChampionExclusive => category==ItemCategory.ChampionExclusive || !string.IsNullOrEmpty(exclusiveToChampionId);
        public int SellValue => Mathf.RoundToInt(totalCost*Mathf.Clamp01(sellRefund));
    }
}

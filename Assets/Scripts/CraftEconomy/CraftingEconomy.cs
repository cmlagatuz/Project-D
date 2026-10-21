// Project D | Codename: Zantetsuken — CraftingEconomy.cs (shards -> cosmetics, disenchant)
using System; using System.Collections.Generic;
namespace ProjectD.P11.Crafting
{
    public enum Rarity { Common, Rare, Epic, Legendary }
    public sealed class CraftingWallet { public int essence; public readonly Dictionary<string,int> shards=new(); }
    public static class CraftingEconomy
    {
        public static int CraftCost(Rarity r)=>r switch{Rarity.Common=>90,Rarity.Rare=>150,Rarity.Epic=>250,Rarity.Legendary=>400,_=>150};
        public static int DisenchantValue(Rarity r)=>(int)(CraftCost(r)*0.4f);   // 40% back
        public static int ShardsToCraft(Rarity r)=>r switch{Rarity.Legendary=>3,Rarity.Epic=>2,_=>1};
        public static void AddShard(CraftingWallet w,string cosmeticId,int count=1){ w.shards.TryGetValue(cosmeticId,out int v); w.shards[cosmeticId]=v+count; }
        /// <summary>Craft a cosmetic from shards + essence. Returns true on success (consumes both).</summary>
        public static bool Craft(CraftingWallet w,string cosmeticId,Rarity rarity)
        {
            int needShards=ShardsToCraft(rarity), needEssence=CraftCost(rarity);
            w.shards.TryGetValue(cosmeticId,out int have);
            if(have<needShards || w.essence<needEssence) return false;
            w.shards[cosmeticId]=have-needShards; w.essence-=needEssence; return true;
        }
        /// <summary>Disenchant a duplicate cosmetic into essence.</summary>
        public static int Disenchant(CraftingWallet w,Rarity rarity){ int v=DisenchantValue(rarity); w.essence+=v; return v; }
    }
}

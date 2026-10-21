// Project D | Codename: Zantetsuken — CraftMastery.cs (crafting skill tree + quality)
using System; using System.Collections.Generic;
namespace ProjectD.P13.CraftMastery
{
    public enum Quality { Crude, Fine, Superior, Masterwork }
    public sealed class CraftRecipe { public string id, name; public int requiredMastery; public string[] inputIds=new string[0]; }
    public sealed class MasteryTree
    {
        public int masteryLevel=1; public long masteryXp;
        readonly Dictionary<string,CraftRecipe> _recipes=new();
        public static long XpForLevel(int lvl)=>500+(long)lvl*400;
        public const int MaxMastery=50;
        public void Register(CraftRecipe r){ if(r!=null&&!string.IsNullOrEmpty(r.id)) _recipes[r.id]=r; }
        public bool RecipeUnlocked(string id){ return _recipes.TryGetValue(id,out var r) && masteryLevel>=r.requiredMastery; }
        public void AddXp(long xp){ if(xp<=0) return; masteryXp+=xp; while(masteryLevel<MaxMastery&&masteryXp>=XpForLevel(masteryLevel)){ masteryXp-=XpForLevel(masteryLevel); masteryLevel++; } }
        /// <summary>Higher mastery + a quality roll yields better output quality.</summary>
        public Quality RollQuality(float roll01)
        {
            float bonus=Math.Min(0.4f, masteryLevel*0.008f);   // mastery improves odds
            float v=Math.Min(1f, roll01+bonus);
            return v>=0.9f?Quality.Masterwork:v>=0.7f?Quality.Superior:v>=0.4f?Quality.Fine:Quality.Crude;
        }
        public int UnlockedRecipeCount(){ int n=0; foreach(var r in _recipes.Values) if(masteryLevel>=r.requiredMastery) n++; return n; }
    }
}

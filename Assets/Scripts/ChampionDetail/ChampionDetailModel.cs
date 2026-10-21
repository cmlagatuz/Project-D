// Project D | Codename: Zantetsuken — ChampionDetailModel.cs (preview page + KP buy state)
using System;
namespace ProjectD.ChampionDetail
{
    public enum StatAxis { Damage, Toughness, Utility, Difficulty }
    [Serializable] public struct StatBar { public StatAxis Axis; public int Value; }
    [Serializable] public sealed class SkinInfo { public string skinId, displayName, rarity; public int kpPrice; public bool owned, isDefault; }
    public struct ChampionPreviewInput { public string ChampionId, DisplayName, Title, PrimaryRole, SecondaryRole; public int Difficulty; public float BaseAttackDamage, BaseMagicPower, BaseHealth, BaseArmor, BaseMagicResist, HealShieldPower; public int UnlockKpPrice, RequiredAccountLevel; public string[] AbilityNames; public SkinInfo[] Skins; }
    public enum UnlockState { Owned, Purchasable, LevelLocked, NotEnoughKp }
    public sealed class ChampionDetailModel
    {
        public ChampionPreviewInput Input{get;private set;}
        public System.Collections.Generic.List<StatBar> StatBars{get;}=new();
        public void Load(ChampionPreviewInput i){ Input=i; StatBars.Clear(); StatBars.Add(new StatBar{Axis=StatAxis.Damage,Value=Dmg(i)}); StatBars.Add(new StatBar{Axis=StatAxis.Toughness,Value=Tuf(i)}); StatBars.Add(new StatBar{Axis=StatAxis.Utility,Value=Uti(i)}); StatBars.Add(new StatBar{Axis=StatAxis.Difficulty,Value=Math.Clamp(i.Difficulty*2,1,10)}); }
        public UnlockState ResolveUnlock(bool owned, int accountLevel, long kp){ if(owned) return UnlockState.Owned; if(Input.RequiredAccountLevel>0 && accountLevel<Input.RequiredAccountLevel) return UnlockState.LevelLocked; if(kp<Input.UnlockKpPrice) return UnlockState.NotEnoughKp; return UnlockState.Purchasable; }
        static int Dmg(in ChampionPreviewInput c)=>C(Math.Max(c.BaseAttackDamage,c.BaseMagicPower)/70f*10f);
        static int Tuf(in ChampionPreviewInput c)=>C(c.BaseHealth/700f*5f+(c.BaseArmor+c.BaseMagicResist)/80f*5f);
        static int Uti(in ChampionPreviewInput c){ float u=c.HealShieldPower/20f*6f; if(c.PrimaryRole=="Support"||c.SecondaryRole=="Support") u+=3f; if(c.PrimaryRole=="Vanguard"||c.SecondaryRole=="Vanguard") u+=1.5f; return C(u); }
        static int C(float v)=>Math.Clamp((int)Math.Round(v),1,10);
    }
}

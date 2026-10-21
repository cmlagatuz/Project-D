// Project D | Codename: Zantetsuken — WorldRiftMatch.cs (orchestrates a full World Rift match)
using System; using System.Collections.Generic;
using ProjectD.Modes; using ProjectD.Worlds; using ProjectD.WorldBoss; using ProjectD.Events;
namespace ProjectD.Match
{
    public sealed class WorldRiftMatch
    {
        public WorldDef World{get;private set;}
        public ModifierProfile Modifier{get;private set;}
        public ManifestationBoss Boss{get;private set;}
        public WorldEventDirector Events{get;private set;}=new();
        public float Clock{get;private set;}

        /// <summary>Set up a World Rift match: pick a realm, resolve its modifier, arm the boss + events.
        /// Rejects the canonical map (World Rift is rotating-only) — the fairness guardrail.</summary>
        public bool Setup(long matchSeed, WorldSelector selector)
        {
            World=selector.Pick(matchSeed); if(World==null) return false;
            if(!GameModeRegistry.IsValidMap(GameMode.WorldRift, World.id)) return false;   // guardrail
            Modifier=WorldModifierEngine.Resolve(World.modifier);
            Boss=new ManifestationBoss(ManifestationConfig.Default(World.manifestationId));
            Events.BuildFor(World);
            Clock=0f; return true;
        }
        public void Tick(float dt){ Clock+=dt; Boss.Tick(Clock); Events.Tick(Clock); }

        /// <summary>Effective kill gold after the world's modifier (e.g., Wild Rose bonus).</summary>
        public int KillGold(int baseGold)=>(int)Math.Round(baseGold*Modifier.killGoldMult);
        /// <summary>Effective vision radius after the modifier (Umbral dims it).</summary>
        public float VisionRadius(float baseRadius)=>baseRadius*Modifier.visionRadiusMult;
        /// <summary>Whether Ranked LP can ever be awarded here (never for World Rift).</summary>
        public bool AwardsRankedLp=>GameModeRegistry.CanAwardRankedLp(GameMode.WorldRift, World?.id ?? "");
        public string Summary()=>World==null?"(no world)":$"{World.name} — {WorldModifierEngine.Describe(World.modifier)} | Boss: {World.manifestationId}";
    }
}

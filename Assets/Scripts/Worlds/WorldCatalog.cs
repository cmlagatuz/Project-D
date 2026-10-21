// Project D | Codename: Zantetsuken — WorldCatalog.cs (the 10 rotating realms)
// RIGHTS: realm/summon/champion names are PLACEHOLDERS; content must be original/licensed.
using System.Collections.Generic;
namespace ProjectD.Worlds
{
    public enum WorldModifierType { Radiant, Unstable, WildRose, Dominion, JobShift, Umbral, Twilight, LowGravity, Gale, Rootbound }
    public sealed class WorldDef { public string id, name, championId, manifestationId; public WorldModifierType modifier; public string themeBgmKey; }
    public static class WorldCatalog
    {
        static readonly List<WorldDef> _worlds=new()
        {
            new(){id="dawnspire",  name="Dawnspire Sanctum",  championId="wol",              manifestationId="summon_alexander", modifier=WorldModifierType.Radiant,    themeBgmKey="bgm_world_dawnspire"},
            new(){id="chaosrift",  name="The Chaos Rift",     championId="garland",          manifestationId="summon_ifrit",     modifier=WorldModifierType.Unstable,   themeBgmKey="bgm_world_chaosrift"},
            new(){id="rebellion",  name="Rebellion Bastion",  championId="firion",           manifestationId="summon_ramuh",     modifier=WorldModifierType.WildRose,   themeBgmKey="bgm_world_rebellion"},
            new(){id="palamecia",  name="Palamecia Throne",   championId="emperor",          manifestationId="summon_odin",      modifier=WorldModifierType.Dominion,   themeBgmKey="bgm_world_palamecia"},
            new(){id="crystalnexus",name="Crystal Nexus",     championId="onion_knight",     manifestationId="summon_shiva",     modifier=WorldModifierType.JobShift,   themeBgmKey="bgm_world_crystalnexus"},
            new(){id="voidbetween",name="The Void Between",   championId="cloud_of_darkness",manifestationId="summon_leviathan", modifier=WorldModifierType.Umbral,     themeBgmKey="bgm_world_voidbetween"},
            new(){id="baronsky",   name="Baron Skyfortress",  championId="cecil",            manifestationId="summon_bahamut",   modifier=WorldModifierType.Twilight,   themeBgmKey="bgm_world_baronsky"},
            new(){id="highwind",   name="Highwind Peaks",     championId="kain",             manifestationId="summon_garuda",    modifier=WorldModifierType.LowGravity, themeBgmKey="bgm_world_highwind"},
            new(){id="windwalker", name="Windwalker Meadows", championId="bartz",            manifestationId="summon_titan",     modifier=WorldModifierType.Gale,       themeBgmKey="bgm_world_windwalker"},
            new(){id="greattree",  name="The Great Tree",     championId="exdeath",          manifestationId="summon_odin",      modifier=WorldModifierType.Rootbound,  themeBgmKey="bgm_world_greattree"},
        };
        static readonly Dictionary<string,WorldDef> _byId=new();
        static WorldCatalog(){ foreach(var w in _worlds) _byId[w.id]=w; }
        public static IReadOnlyList<WorldDef> All=>_worlds;
        public static WorldDef Get(string id)=>_byId.TryGetValue(id,out var w)?w:null;
        public static WorldDef ForChampion(string championId){ foreach(var w in _worlds) if(w.championId==championId) return w; return null; }
        public static int Count=>_worlds.Count;
    }
}

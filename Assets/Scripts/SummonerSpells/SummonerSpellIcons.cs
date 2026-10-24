// Project D | Codename: Zantetsuken — SummonerSpellIcons.cs
// Runtime icon lookup (Unity side). Icons are original placeholder art in
// Assets/Resources/UI/SummonerSpells/ and are imported as Sprites by SummonerIconImporter.
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.SummonerSpells
{
    public static class SummonerSpellIcons
    {
        private static readonly Dictionary<SummonerSpellId, Sprite> Cache = new Dictionary<SummonerSpellId, Sprite>();

        /// <summary>Returns the spell's icon, or null (with a warning) if the PNG is missing.</summary>
        public static Sprite Load(SummonerSpellId id)
        {
            if (Cache.TryGetValue(id, out var cached) && cached != null) return cached;
            var path = SummonerSpellCatalog.Get(id).IconResourcePath;
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null) Debug.LogWarning("[Project D] Summoner spell icon not found: Resources/" + path);
            else Cache[id] = sprite;
            return sprite;
        }
    }
}

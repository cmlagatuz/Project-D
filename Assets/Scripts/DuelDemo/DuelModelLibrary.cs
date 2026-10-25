// Project D | Codename: Zantetsuken — DuelModelLibrary.cs
// Maps a unit id ("wol", "garland", "shiva", ...) to the 3D prefab used for it in the duel demo.
// The asset is filled in by the editor tool  Tools > Project D > Link Duel Demo Models  and referenced by the scene,
// so the prefabs are included in phone builds. A missing entry simply falls back to a placeholder shape.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.DuelDemo
{
    [CreateAssetMenu(fileName = "DuelModelLibrary", menuName = "Project D/Duel Model Library")]
    public sealed class DuelModelLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string id;
            public GameObject prefab;
            [Tooltip("Height in world units the model is scaled to (the arena is 28 units wide).")]
            public float height = 1.9f;
            [Tooltip("Extra rotation about Y if the model does not face +Z (forward) in its prefab.")]
            public float yawOffset;
        }

        public List<Entry> entries = new List<Entry>();

        public Entry Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i] != null && entries[i].id == id && entries[i].prefab != null) return entries[i];
            return null;
        }

        /// <summary>Default fitted height for an id: champions are human sized, summons are big.</summary>
        public static float DefaultHeight(string id)
        {
            return id == "shiva" || id == "garuda" ? 3.6f : 1.9f;
        }
    }
}

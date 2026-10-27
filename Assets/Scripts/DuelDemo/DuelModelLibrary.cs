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
            [Tooltip("Set once the yaw was tuned (by hand or with the TOOLS turn buttons), so the linker stops applying its default.")]
            public bool yawTuned;
        }

        public List<Entry> entries = new List<Entry>();

        public Entry Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i] != null && entries[i].id == id && entries[i].prefab != null) return entries[i];
            return null;
        }

        /// <summary>Default fitted height for an id: champions are human sized, summons are big, Bahamut is huge.</summary>
        public static float DefaultHeight(string id)
        {
            if (id == "bahamut") return 7f;
            return id == "shiva" || id == "garuda" ? 5f : 2.5f;
        }

        /// <summary>The height earlier versions used. A library entry still at this value was never tuned, so it gets the new default.</summary>
        public static float OldDefaultHeight(string id)
        {
            return id == "shiva" || id == "garuda" ? 3.6f : 1.9f;
        }

        /// <summary>
        /// First guess for how the rip is turned in its file (tune it with the TOOLS turn buttons, which save the answer here).
        /// Shiva came in facing away; Garuda's wings came in edge-on to the camera so it read as flat.
        /// </summary>
        public static float DefaultYaw(string id)
        {
            if (id == "shiva") return 180f;
            if (id == "garuda") return 90f;
            return 0f;
        }

        /// <summary>Remembers a yaw found by eye while playing in the editor. Does nothing in a phone build.</summary>
        public void PersistYaw(string id, float yaw)
        {
#if UNITY_EDITOR
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null || e.id != id) continue;
                e.yawOffset = yaw;
                e.yawTuned = true;
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
                return;
            }
#endif
        }
    }
}

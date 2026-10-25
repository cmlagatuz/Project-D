// Project D  |  Codename: Zantetsuken
// DuelModelLinker.cs  (Editor-only)
//
//   Tools > Project D > Link Duel Demo Models
//
// Finds your 3D models anywhere under Assets/ by name and records them in Assets/DuelDemo/DuelModelLibrary.asset,
// which the duel scene references (so the prefabs are included in phone builds).
// Looks for: Warrior of Light, Firion, Cecil, Garland, The Emperor, Golbez, Shiva, Garuda.
// Best match wins: an exact file name beats a partial one, a prefab beats a raw .fbx, and files in the project's
// convention folders (Assets/Models/Champions/<id>/..., Assets/Models/Summons/<id>/...) get a bonus. Animation-only
// files (Name@Run.fbx), LOD1+ meshes and weapon/VFX files are ignored.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using ProjectD.DuelDemo;

namespace ProjectD.EditorTools
{
    public static class DuelModelLinker
    {
        public const string LibraryPath = "Assets/DuelDemo/DuelModelLibrary.asset";

        private static readonly string[] Ids = { "wol", "firion", "cecil", "garland", "emperor", "golbez", "shiva", "garuda" };

        private static readonly Dictionary<string, string[]> Aliases = new Dictionary<string, string[]>
        {
            { "wol", new[] { "warrioroflight", "wol" } },
            { "firion", new[] { "firion" } },
            { "cecil", new[] { "cecilharvey", "cecil" } },
            { "garland", new[] { "garland" } },
            { "emperor", new[] { "theemperor", "emperor" } },
            { "golbez", new[] { "golbez" } },
            { "shiva", new[] { "shiva", "summonshiva" } },
            { "garuda", new[] { "garuda", "summongaruda" } },
        };

        private static readonly string[] Ignore = { "@", "lod1", "lod2", "lod3", "collider", "weapon", "sword", "vfx", "fx_", "anim", "icon", "portrait", "texture" };

        [MenuItem("Tools/Project D/Link Duel Demo Models", priority = 44)]
        public static void LinkMenu()
        {
            string report = LinkAll();
            Debug.Log("[Project D] Duel demo models:\n" + report);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D - duel demo models", report, "OK");
        }

        /// <summary>Returns the library (creating it if needed), refreshed from whatever models exist right now.</summary>
        public static DuelModelLibrary EnsureLibrary()
        {
            LinkAll();
            return AssetDatabase.LoadAssetAtPath<DuelModelLibrary>(LibraryPath);
        }

        public static string LinkAll()
        {
            int moved = DuelModelCleanup.MoveUnsupported();   // .mesh files from game rips are not Unity assets and only cause import errors
            EnsureFolder("Assets/DuelDemo");
            var lib = AssetDatabase.LoadAssetAtPath<DuelModelLibrary>(LibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<DuelModelLibrary>();
                AssetDatabase.CreateAsset(lib, LibraryPath);
            }

            var candidates = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:GameObject", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Assets/Scripts/") || path.Contains("/Samples/")) continue;
                candidates.Add(path);
            }

            var sb = new StringBuilder();
            if (moved > 0) sb.AppendLine("Moved " + moved + " unsupported .mesh file(s) to <project>/ModelsUnsupported/ (not usable in Unity).\n");
            int found = 0;
            foreach (var id in Ids)
            {
                string best = Best(id, candidates);
                var existing = lib.entries.Find(e => e != null && e.id == id);
                if (best == null)
                {
                    if (existing != null && existing.prefab != null) { found++; sb.AppendLine(id + ": kept " + AssetDatabase.GetAssetPath(existing.prefab)); }
                    else sb.AppendLine(id + ": NOT FOUND (placeholder shape will be used)");
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(best);
                if (existing == null)
                {
                    existing = new DuelModelLibrary.Entry { id = id, height = DuelModelLibrary.DefaultHeight(id) };
                    lib.entries.Add(existing);
                }
                existing.prefab = prefab;
                found++;
                sb.AppendLine(id + ": " + best);
            }

            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            sb.Insert(0, "Linked " + found + " of " + Ids.Length + " models.\n\n");
            sb.AppendLine();
            sb.AppendLine("If a model faces the wrong way or is the wrong size, select " + LibraryPath +
                          " and change that entry's Yaw Offset or Height. Then rebuild the duel scene.");
            return sb.ToString();
        }

        private static string Best(string id, List<string> candidates)
        {
            var aliases = Aliases[id];
            string best = null;
            int bestScore = 0;
            foreach (var path in candidates)
            {
                string ext = Path.GetExtension(path).ToLowerInvariant();
                string rawName = Path.GetFileNameWithoutExtension(path);
                string lower = rawName.ToLowerInvariant();
                bool skip = false;
                foreach (var bad in Ignore) if (lower.Contains(bad)) { skip = true; break; }
                if (skip) continue;

                // Folder names along the path, e.g. Assets/Models/Cecil/Weapon/Holy Fang.fbx -> models, cecil, weapon.
                var segments = new List<string>();
                foreach (var seg in (Path.GetDirectoryName(path) ?? "").Replace('\\', '/').Split('/')) segments.Add(Normalize(seg));
                bool inWeaponFolder = false;
                foreach (var seg in segments) if (seg.Contains("weapon")) inWeaponFolder = true;
                if (inWeaponFolder) continue;   // weapons are separate FBX files and are not attached in the demo yet

                string name = Normalize(rawName);
                if (name.StartsWith("prefab")) name = name.Substring(6);
                if (name.EndsWith("prefab")) name = name.Substring(0, name.Length - 6);

                int nameScore = 0;
                foreach (var a in aliases)
                {
                    if (name == a) nameScore = System.Math.Max(nameScore, 100);
                    else if (a.Length > 3 && name.Contains(a)) nameScore = System.Math.Max(nameScore, 50);   // short aliases ("wol") must match exactly
                }

                bool inUnitFolder = false;   // a folder called Cecil / Cecil Harvey / Shiva XVI, or the older Champions/<id>/ layout
                string joined = string.Join("", segments.ToArray());
                foreach (var a in aliases)
                {
                    foreach (var seg in segments)
                    {
                        if (seg == a || (a.Length > 3 && seg.Contains(a))) inUnitFolder = true;
                    }
                    if (joined.Contains("champions" + a) || joined.Contains("summons" + a)) inUnitFolder = true;
                }

                int score;
                if (nameScore > 0) score = nameScore + (inUnitFolder ? 30 : 0);
                else if (inUnitFolder) score = 20;   // some model file inside the unit's own folder
                else continue;

                if (ext == ".prefab") score += 20;
                else if (ext == ".fbx") score += 10;
                else score += 5;

                if (score > bestScore || (score == bestScore && best != null && path.Length < best.Length))
                {
                    best = path;
                    bestScore = score;
                }
            }
            return best;
        }

        private static string Normalize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.ToLowerInvariant()) if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.ToString();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
#endif
